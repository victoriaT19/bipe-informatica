using System;
using System.Collections.Generic;
using System.Linq;

namespace Bipe
{
    // All the rules of the shop. Knows nothing about Unity UI, so it can be tested on its own.
    public class Game
    {
        public const int Open = 9 * 60, Close = 18 * 60, Rent = 60, FixMinutes = 30, WarrantyMinutes = 20;
        const double PyFactor = 0.4, PyFail = 0.35, ReturnChance = 0.5, RegularChance = 0.4, DoubleCaughtChance = 0.4, GenericSaysChance = 0.45;

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
            if (savedIsUsable) S = saved;
            else NewGame();
        }

        public FaultDefinition Fault(string key) => faults[key];
        public List<FaultDefinition> Unlocked() => faultList.Where(f => f.unlockDay <= S.day).ToList();
        public KnownCustomer Known(string name) => S.known.Find(k => k.name == name);
        public bool CustomerIsBurned => Known(S.customer.name)?.burned ?? false;
        public static int PyCost(FaultDefinition f) => (int)Math.Round(f.partCost * PyFactor);

        T Pick<T>(IList<T> list) => list[rng.Next(list.Count)];
        bool Chance(double p) => rng.NextDouble() < p;
        void Notify() => Changed?.Invoke();

        public void NewGame()
        {
            S = new GameState { day = 1, clock = Open, cash = 100, rep = 3, osNumber = 41 };
            NextCustomer();
            Notify();
        }

        void NextCustomer()
        {
            S.osNumber++;
            S.phase = Phase.Investigate;
            S.last = null;
            S.usePyPart = false;
            S.chosenFix = "";

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
            };
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
            if (S.phase != Phase.Investigate || c.done.Contains(id)) return;
            var t = Array.Find(DefaultContent.Tests, x => x.id == id);
            var f = faults[c.faultKey];
            c.done.Add(id);
            S.clock += t.minutes;
            c.log.Add(new LogLine { time = FormatTime(S.clock), command = t.command, text = ResultFor(f, id) });
            if (id == TestId.Power)
            {
                c.ledOn = !string.IsNullOrEmpty(f.powerBeep);
                Beeped?.Invoke(f.powerBeep);
            }
            Notify();
        }

        string ResultFor(FaultDefinition f, TestId id)
        {
            const string noRun = "Não dá pra rodar: o PC não passa do teste de inicialização.";
            switch (id)
            {
                case TestId.Talk: return f.talk.Count > 0 && Chance(0.65) ? Pick(f.talk) : Pick(DefaultContent.GenericTalk);
                case TestId.Power: return f.powerText;
                case TestId.Open: return Or(f.open, "Tudo encaixado. Poeira normal pra 1996.");
                case TestId.Psu: return Or(f.psu, "+12V: 12,1V   +5V: 5,0V   Fonte OK.");
                case TestId.Antivirus: return Or(f.antivirus, f.noPost ? noRun : "Nenhum vírus encontrado. Disco limpo.");
                case TestId.ScanDisk: return Or(f.scanDisk, f.noPost ? noRun : "0 clusters defeituosos. Disco OK.");
                case TestId.Devices: return Or(f.devices, f.noPost ? noRun : "Nenhum conflito de dispositivo.");
            }
            return "";
        }

        static string Or(string a, string b) => string.IsNullOrEmpty(a) ? b : a;

        // --- diagnosis and payment ---

        public void OpenDiagnosis()
        {
            if (S.phase != Phase.Investigate) return;
            S.phase = Phase.ChooseFix;
            Notify();
        }

        public void BackToInvestigate()
        {
            if (S.phase != Phase.ChooseFix) return;
            S.phase = Phase.Investigate;
            Notify();
        }

        public void ChooseFix(string key)
        {
            if (S.phase != Phase.ChooseFix) return;
            S.chosenFix = key;
            S.usePyPart = false;
            S.phase = Phase.ChoosePrice;
            Notify();
        }

        public void SetPyPart(bool py)
        {
            S.usePyPart = py;
            Notify();
        }

        public void Charge(bool doublePrice)
        {
            if (S.phase != Phase.ChoosePrice) return;
            var c = S.customer;
            var chosen = faults[S.chosenFix];
            bool correct = S.chosenFix == c.faultKey;
            var mem = Remember(c);
            bool usedPy = chosen.partCost > 0 && S.usePyPart;
            int partCost = usedPy ? PyCost(chosen) : chosen.partCost;
            if (mem.burned) doublePrice = false;

            S.clock += FixMinutes;
            S.stats.served++;
            mem.visits++;
            var o = new Outcome { good = correct };

            if (correct)
            {
                S.stats.right++;
                o.delta = (doublePrice ? chosen.price * 2 : chosen.price) - partCost;
                o.lines.Add($"Acertou: {chosen.fix.ToLower()} resolveu.");
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
                else
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
                // the gamble stays hidden until the customer walks back in
                if (usedPy && Chance(PyFail))
                    S.returns.Add(ReturnOf(c, CustomerKind.Warranty, S.day + (Chance(0.5) ? 0 : 1)));
            }
            else
            {
                S.stats.wrong++;
                o.delta = -partCost;
                o.repDelta = -1;
                mem.happy = false;
                o.lines.Add($"{c.name} testou em casa e nada. Foi embora sem pagar e disse que volta amanhã.");
                o.lines.Add($"O problema real era: {faults[c.faultKey].fix.ToLower()}.");
                if (partCost > 0) o.lines.Add($"Você perdeu {Brl(partCost)} na peça.");
                S.returns.Add(ReturnOf(c, CustomerKind.Redo, S.day + 1));
            }
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

        PendingReturn ReturnOf(Customer c, CustomerKind kind, int dueDay) =>
            new PendingReturn { name = c.name, desc = c.desc, colorHex = c.colorHex, faultKey = c.faultKey, kind = kind, dueDay = dueDay };

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
