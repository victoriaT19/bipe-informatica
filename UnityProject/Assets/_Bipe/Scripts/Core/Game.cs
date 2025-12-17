using System;
using System.Collections.Generic;
using System.Linq;

namespace Bipe
{
    // All the rules of the shop. Knows nothing about Unity UI, so it can be tested on its own.
    public class Game
    {
        public const int Open = 9 * 60, Close = 18 * 60, Rent = 60, FixMinutes = 30, WarrantyMinutes = 20, MaxFixes = 2;
        const int LiarDay = 2, DualDay = 3;
        const double PyFactor = 0.4, PyFail = 0.35, ReturnChance = 0.5, RegularChance = 0.4, DoubleCaughtChance = 0.4, GenericSaysChance = 0.45;
        const double LiarChance = 0.3, DualChance = 0.3;
        // "Explicar pro cliente" is not a hardware defect, so it never pairs with another one
        const string UserFaultKey = "user";

        public GameState S { get; private set; }
        public event Action Changed;
        public event Action<string> Beeped;

        readonly List<FaultDefinition> faultList;
        readonly Dictionary<string, FaultDefinition> faults;
        readonly Random rng = new Random();

        public Game(IEnumerable<FaultDefinition> defs, GameState saved = null)
        {
            faultList = defs.OrderBy(f => f.unlockDay).ToList();
            faults = faultList.ToDictionary(f => f.key);
            bool savedIsUsable = saved != null && saved.customer != null && faults.ContainsKey(saved.customer.faultKey ?? "");
            if (savedIsUsable)
            {
                S = saved;
                // saves from v0.3 have no tool list
                if (S.tools.Count == 0) S.tools.AddRange(DefaultContent.StartingTools);
            }
            else NewGame();
        }

        public FaultDefinition Fault(string key) => faults[key];
        public List<FaultDefinition> Unlocked() => faultList.Where(f => f.unlockDay <= S.day).ToList();
        public List<FaultDefinition> ChosenFaults() => S.chosenFixes.Select(k => faults[k]).ToList();
        public KnownCustomer Known(string name) => S.known.Find(k => k.name == name);
        public bool CustomerIsBurned => Known(S.customer.name)?.burned ?? false;
        public bool HasTool(TestId id) => S.tools.Contains(id);
        public static int PyCost(FaultDefinition f) => (int)Math.Round(f.partCost * PyFactor);
        public static string Names(IEnumerable<FaultDefinition> list) => string.Join(" e ", list.Select(f => f.fix.ToLower()));

        T Pick<T>(IList<T> list) => list[rng.Next(list.Count)];
        bool Chance(double p) => rng.NextDouble() < p;
        void Notify() => Changed?.Invoke();

        public void NewGame()
        {
            S = new GameState { day = 1, clock = Open, cash = 100, rep = 3, osNumber = 41 };
            S.tools.AddRange(DefaultContent.StartingTools);
            NextCustomer();
            Notify();
        }

        void NextCustomer()
        {
            S.osNumber++;
            S.phase = Phase.Investigate;
            S.last = null;
            S.usePyPart = false;
            S.chosenFixes.Clear();

            // someone coming back: a failed Paraguay part or a wrong diagnosis
            int due = S.returns.FindIndex(r => r.dueDay <= S.day);
            if (due >= 0 && Chance(ReturnChance))
            {
                var r = S.returns[due];
                S.returns.RemoveAt(due);
                var f = faults[r.faultKey];
                S.customer = new Customer { name = r.name, desc = r.desc, colorHex = r.colorHex, faultKey = r.faultKey, kind = r.kind };
                if (r.kind == CustomerKind.Warranty)
                {
                    S.customer.says = string.Format(Pick(DefaultContent.WarrantySays), f.fix.ToLower());
                    S.customer.log.Add(new LogLine { time = FormatTime(S.clock), command = "CONFERIR NOTA", text = $"Garantia: {f.fix.ToLower()} com peça do Paraguai. A peça queimou." });
                    S.phase = Phase.Warranty;
                }
                else
                {
                    S.customer.says = Pick(DefaultContent.RedoSays);
                }
                return;
            }

            var busy = new HashSet<string>(S.returns.Select(r => r.name));
            var free = DefaultContent.People.Where(p => !busy.Contains(p.name)).ToList();
            if (free.Count == 0) free = DefaultContent.People.ToList();
            var regulars = free.Where(p => Known(p.name) != null).ToList();
            var person = regulars.Count > 0 && Chance(RegularChance) ? Pick(regulars) : Pick(free);
            var mem = Known(person.name);
            var fault = Pick(Unlocked());

            S.customer = new Customer
            {
                name = person.name,
                desc = person.desc,
                colorHex = mem != null ? mem.colorHex : Pick(DefaultContent.Colors),
                faultKey = fault.key,
                kind = mem != null ? CustomerKind.Regular : CustomerKind.New,
                greet = mem != null ? GreetFor(mem) : "",
                says = fault.says.Count == 0 || Chance(GenericSaysChance) ? Pick(DefaultContent.GenericSays) : Pick(fault.says),
                liar = S.day >= LiarDay && Chance(LiarChance),
            };

            // a second defect only hides behind a PC that still boots, or it could never be found
            if (S.day >= DualDay && Pairable(fault) && Chance(DualChance))
            {
                var candidates = Unlocked().Where(f => Pairable(f) && f.key != fault.key).ToList();
                if (candidates.Count > 0) S.customer.secondKey = Pick(candidates).key;
            }
        }

        static bool Pairable(FaultDefinition f) => !f.noPost && f.key != UserFaultKey;

        List<string> ActualFaults(Customer c)
        {
            var keys = new List<string> { c.faultKey };
            if (!string.IsNullOrEmpty(c.secondKey)) keys.Add(c.secondKey);
            return keys;
        }

        string GreetFor(KnownCustomer m)
        {
            if (m.burned) return "Meu cunhado viu a nota da última vez. Hoje eu quero preço justo, hein.";
            if (m.happy) return Pick(DefaultContent.HappyGreets);
            return "Oi de novo.";
        }

        KnownCustomer Remember(Customer c)
        {
            var k = Known(c.name);
            if (k == null)
            {
                k = new KnownCustomer { name = c.name, desc = c.desc, colorHex = c.colorHex };
                S.known.Add(k);
            }
            return k;
        }

        // --- investigation ---

        public void RunTest(TestId id)
        {
            var c = S.customer;
            if (S.phase != Phase.Investigate || c.done.Contains(id) || !HasTool(id)) return;
            var t = Array.Find(DefaultContent.Tests, x => x.id == id);
            var f = faults[c.faultKey];
            c.done.Add(id);
            S.clock += t.minutes;
            c.log.Add(new LogLine { time = FormatTime(S.clock), command = t.command, text = ResultFor(c, id) });
            if (id == TestId.Power)
            {
                c.ledOn = !string.IsNullOrEmpty(f.powerBeep);
                Beeped?.Invoke(f.powerBeep);
            }
            Notify();
        }

        string ResultFor(Customer c, TestId id)
        {
            var f = faults[c.faultKey];
            var second = string.IsNullOrEmpty(c.secondKey) ? null : faults[c.secondKey];

            if (id == TestId.Talk)
            {
                if (c.liar)
                {
                    // the story points at some other defect
                    var others = faultList.Where(x => !ActualFaults(c).Contains(x.key) && x.talk.Count > 0).ToList();
                    if (others.Count > 0) return Pick(Pick(others).talk);
                }
                return f.talk.Count > 0 && Chance(0.65) ? Pick(f.talk) : Pick(DefaultContent.GenericTalk);
            }
            if (id == TestId.Power) return f.powerText;

            string own = Specific(f, id);
            string extra = second != null ? Specific(second, id) : null;
            string text = own == null ? (extra ?? Normal(f, id))
                : extra == null ? own
                : $"{own} Também: {extra}";
            if (id == TestId.Open && c.liar) text += " " + Pick(DefaultContent.TamperEvidence);
            return text;
        }

        // what this defect shows on this test, or null when the test looks normal for it
        static string Specific(FaultDefinition f, TestId id)
        {
            string s = null;
            switch (id)
            {
                case TestId.Open: s = f.open; break;
                case TestId.Psu: s = f.psu; break;
                case TestId.Antivirus: s = f.antivirus; break;
                case TestId.ScanDisk: s = f.scanDisk; break;
                case TestId.Devices: s = f.devices; break;
            }
            return string.IsNullOrEmpty(s) ? null : s;
        }

        static string Normal(FaultDefinition f, TestId id)
        {
            const string noRun = "Não dá pra rodar: o PC não passa do teste de inicialização.";
            switch (id)
            {
                case TestId.Open: return "Tudo encaixado. Poeira normal pra 1996.";
                case TestId.Psu: return "+12V: 12,1V   +5V: 5,0V   Fonte OK.";
                case TestId.Antivirus: return f.noPost ? noRun : "Nenhum vírus encontrado. Disco limpo.";
                case TestId.ScanDisk: return f.noPost ? noRun : "0 clusters defeituosos. Disco OK.";
                case TestId.Devices: return f.noPost ? noRun : "Nenhum conflito de dispositivo.";
            }
            return "";
        }

        // --- diagnosis and payment ---

        public void OpenDiagnosis()
        {
            if (S.phase != Phase.Investigate) return;
            S.chosenFixes.Clear();
            S.phase = Phase.ChooseFix;
            Notify();
        }

        public void BackToInvestigate()
        {
            if (S.phase != Phase.ChooseFix) return;
            S.phase = Phase.Investigate;
            Notify();
        }

        public void ToggleFix(string key)
        {
            if (S.phase != Phase.ChooseFix) return;
            if (S.chosenFixes.Contains(key)) S.chosenFixes.Remove(key);
            else if (S.chosenFixes.Count < MaxFixes) S.chosenFixes.Add(key);
            Notify();
        }

        public void ConfirmFixes()
        {
            if (S.phase != Phase.ChooseFix || S.chosenFixes.Count == 0) return;
            S.usePyPart = false;
            S.phase = Phase.ChoosePrice;
            Notify();
        }

        public void BackToFixes()
        {
            if (S.phase != Phase.ChoosePrice) return;
            S.phase = Phase.ChooseFix;
            Notify();
        }

        public void SetPyPart(bool py)
        {
            S.usePyPart = py;
            Notify();
        }

        public void Charge(bool doublePrice)
        {
            if (S.phase != Phase.ChoosePrice || S.chosenFixes.Count == 0) return;
            var c = S.customer;
            var mem = Remember(c);
            var chosen = ChosenFaults();
            var actual = ActualFaults(c);
            var right = chosen.Where(f => actual.Contains(f.key)).ToList();
            var wasted = chosen.Where(f => !actual.Contains(f.key)).ToList();
            var missing = actual.Where(k => !S.chosenFixes.Contains(k)).Select(k => faults[k]).ToList();
            bool usedPy = S.usePyPart && chosen.Any(f => f.partCost > 0);
            int partCost = chosen.Sum(f => usedPy ? PyCost(f) : f.partCost);
            if (mem.burned) doublePrice = false;

            S.clock += FixMinutes * chosen.Count;
            S.stats.served++;
            mem.visits++;
            var o = new Outcome();

            if (right.Count == 0)
            {
                S.stats.wrong++;
                o.delta = -partCost;
                o.repDelta = -1;
                mem.happy = false;
                o.lines.Add($"{c.name} testou em casa e nada. Foi embora sem pagar e disse que volta amanhã.");
                o.lines.Add($"O problema real era: {Names(actual.Select(k => faults[k]))}.");
                if (partCost > 0) o.lines.Add($"Você perdeu {Brl(partCost)} em peças.");
                S.returns.Add(ReturnOf(c, CustomerKind.Redo, c.faultKey, S.day + 1));
                Apply(o);
                return;
            }

            bool solved = missing.Count == 0;
            o.good = solved && wasted.Count == 0;
            o.delta = right.Sum(f => f.price) * (doublePrice ? 2 : 1) - partCost;
            if (solved) S.stats.right++;
            else S.stats.wrong++;

            o.lines.Add($"Acertou: {Names(right)}.");
            if (wasted.Count > 0) o.lines.Add($"{c.name} não pagou por {Names(wasted)}: não era isso.");
            if (!solved)
            {
                o.lines.Add($"Mas o PC tinha outro defeito: {Names(missing)}. {c.name} volta amanhã.");
                S.returns.Add(ReturnOf(c, CustomerKind.Redo, missing[0].key, S.day + 1));
            }

            if (doublePrice)
            {
                if (Chance(DoubleCaughtChance))
                {
                    o.repDelta = -2;
                    mem.burned = true;
                    mem.happy = false;
                    o.lines.Add($"{c.name} mostrou a nota pro cunhado, que \"entende de computador\". A notícia de que você cobra caro já corre o bairro.");
                }
                else
                {
                    o.lines.Add($"{c.name} pagou sem piscar. Por enquanto.");
                }
            }
            else if (o.good)
            {
                o.repDelta = 1;
                mem.happy = true;
                o.lines.Add(c.kind == CustomerKind.Redo ? $"Desta vez resolveu. {c.name} voltou a confiar na loja." : $"{c.name} saiu feliz e prometeu indicar a loja.");
                if (S.rep >= 4)
                {
                    o.delta += 10;
                    o.lines.Add("Deixou R$ 10 de gorjeta.");
                }
            }
            else
            {
                mem.happy = false;
                o.lines.Add($"{c.name} pagou, mas saiu desconfiando da loja.");
            }

            // the gamble stays hidden until the customer walks back in
            var withPart = right.FirstOrDefault(f => f.partCost > 0);
            if (solved && usedPy && withPart != null && Chance(PyFail))
                S.returns.Add(ReturnOf(c, CustomerKind.Warranty, withPart.key, S.day + (Chance(0.5) ? 0 : 1)));

            Apply(o);
        }

        public void Warranty(bool honor)
        {
            if (S.phase != Phase.Warranty) return;
            var c = S.customer;
            var f = faults[c.faultKey];
            var mem = Remember(c);
            S.clock += WarrantyMinutes;
            S.stats.served++;
            mem.visits++;
            var o = new Outcome { good = honor };
            if (honor)
            {
                o.delta = -f.partCost;
                o.repDelta = 1;
                mem.happy = true;
                mem.burned = false;
                o.lines.Add($"Você trocou de graça, agora com peça original ({Brl(f.partCost)}).");
                o.lines.Add($"{c.name} agradeceu e disse que garantia assim não existe em lugar nenhum.");
            }
            else
            {
                o.repDelta = -2;
                mem.happy = false;
                mem.burned = true;
                o.lines.Add("Você disse que a garantia não cobre \"mau uso\".");
                o.lines.Add($"{c.name} saiu xingando e prometeu contar pra rua inteira.");
            }
            Apply(o);
        }

        PendingReturn ReturnOf(Customer c, CustomerKind kind, string faultKey, int dueDay) =>
            new PendingReturn { name = c.name, desc = c.desc, colorHex = c.colorHex, faultKey = faultKey, kind = kind, dueDay = dueDay };

        void Apply(Outcome o)
        {
            S.cash += o.delta;
            S.stats.earned += o.delta;
            S.rep = Math.Max(0, Math.Min(5, S.rep + o.repDelta));
            S.last = o;
            S.phase = Phase.Result;
            Notify();
        }

        // --- day flow ---

        public bool ClosingSoon => S.clock >= Close - 15;

        public void Continue()
        {
            if (S.phase != Phase.Result) return;
            if (S.rep <= 0) { GameOver("Ninguém mais entra na loja. A fama acabou."); return; }
            if (ClosingSoon) { EndDay(); return; }
            NextCustomer();
            Notify();
        }

        void EndDay()
        {
            S.cash -= Rent;
            if (S.cash < 0) { GameOver("O aluguel venceu e o caixa não fechou. A loja baixou as portas."); return; }
            S.phase = Phase.DayEnd;
            Notify();
        }

        public void BuyTool(TestId id)
        {
            if (S.phase != Phase.DayEnd || HasTool(id)) return;
            var offer = Array.Find(DefaultContent.Shop, t => t.id == id);
            if (offer == null || S.cash < offer.price) return;
            S.cash -= offer.price;
            S.tools.Add(id);
            Notify();
        }

        void GameOver(string why)
        {
            S.phase = Phase.GameOver;
            S.gameOverReason = why;
            Notify();
        }

        public void NextDay()
        {
            if (S.phase != Phase.DayEnd) return;
            S.day++;
            S.clock = Open;
            S.stats = new DayStats();
            NextCustomer();
            Notify();
        }

        public static string FormatTime(int m) => $"{m / 60:00}:{m % 60:00}";
        public static string Brl(int v) => (v < 0 ? "-R$ " : "R$ ") + Math.Abs(v);
    }
}
