using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Bipe
{
    // Builds the whole screen from code and redraws it from Game.S whenever the state changes.
    // Layout: top bar; then counter (left), CRT bench (middle), tool rack (right).
    public class GameUI : MonoBehaviour
    {
        public static readonly Color Wall = Hex("#1d5f5c");
        static readonly Color Plastic = Hex("#d8d1bc"), PlasticHi = Hex("#efe9d8"), PlasticLo = Hex("#9d9580");
        static readonly Color Ink = Hex("#1d1b16"), InkSoft = Hex("#4a463c"), Crt = Hex("#08140c"), Phosphor = Hex("#8cf7a4");
        static readonly Color Amber = Hex("#f2b33d"), Alert = Hex("#c4392c"), Good = Hex("#2f8a4a"), Postit = Hex("#f3e27a");
        const int ScreenLines = 17;

        Game game;
        BeepSpeaker speaker;
        Font font;
        // monospace for numbers and the CRT: the built-in font's 5 reads like an S at this size
        Font mono;
        bool soundOn = true;

        Text dayT, clockT, cashT, repT, soundT;
        Image avatar, led;
        Text avatarT, nameT, descT, complaintT, osT, screenT, hintT;
        Transform toolsRoot, sheetRoot;
        GameObject sheetPanel;
        Button diagBtn;

        public void Init(Game g, BeepSpeaker s)
        {
            game = g;
            speaker = s;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            mono = Font.CreateDynamicFontFromOSFont(new[] { "Consolas", "Lucida Console", "Courier New" }, 20);
            Build();
            game.Changed += OnChanged;
            game.Beeped += p => { if (soundOn) speaker.Play(p); };
            Render();
        }

        void OnDestroy()
        {
            if (game != null) game.Changed -= OnChanged;
        }

        void OnChanged()
        {
            SaveSystem.Save(game.S);
            Render();
        }

        // ---------- build ----------

        void Build()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = 0.5f;

            var root = Panel(canvasGo.transform, Wall, "Root");
            Stretch(root);
            Vertical(root, 16, 12);

            // top bar
            var bar = Panel(root, Plastic, "TopBar");
            Horizontal(bar, 10, 12, true);
            Size(bar, h: 56);
            var title = Label(bar, "<b>BIPE INFORMÁTICA</b>  <size=14>CONSERTO · MONTAGEM · VACINA DE VÍRUS</size>", 26, Ink);
            Size(title.rectTransform, flexW: 1);
            dayT = Stat(bar);
            clockT = Stat(bar);
            cashT = Stat(bar);
            repT = Stat(bar);
            var soundBtn = MakeButton(bar, "", PlasticHi, ToggleSound, 16);
            soundT = soundBtn.GetComponentInChildren<Text>();
            Size(soundBtn.transform, w: 150);
            var newBtn = MakeButton(bar, "Novo jogo", PlasticHi, game.NewGame, 16);
            Size(newBtn.transform, w: 130);

            var main = Panel(root, Color.clear, "Main");
            Horizontal(main, 0, 12, false);
            Size(main, flexH: 1);

            // counter
            var left = Panel(main, Plastic, "Balcao");
            Vertical(left, 14, 10);
            Size(left, w: 380);
            Label(left, "BALCÃO", 14, InkSoft, FontStyle.Bold);
            var who = Panel(left, Color.clear, "Who");
            Horizontal(who, 0, 12, false);
            avatar = Panel(who, Ink, "Avatar").GetComponent<Image>();
            Size(avatar.transform, w: 64, h: 64);
            avatarT = Label(avatar.transform, "", 24, PlasticHi, FontStyle.Bold, TextAnchor.MiddleCenter);
            Stretch(avatarT.rectTransform);
            var whoText = Panel(who, Color.clear, "WhoText");
            Vertical(whoText, 0, 2);
            Size(whoText, flexW: 1);
            nameT = Label(whoText, "", 20, Ink, FontStyle.Bold);
            descT = Label(whoText, "", 15, InkSoft);
            var bubble = Panel(left, PlasticHi, "Complaint");
            Vertical(bubble, 14, 0);
            complaintT = Label(bubble, "", 18, Ink);
            osT = Label(left, "", 15, InkSoft);
            var postit = Panel(left, Postit, "Postit");
            Vertical(postit, 14, 4);
            Label(postit, "<b>Cola dos bipes (não tirar!!)</b>", 16, Ink);
            Label(postit, "1 curto: passou no teste, hardware OK\nlongo, sem parar: memória RAM\n1 longo + 2 curtos: placa de vídeo\nsilêncio total: energia / fonte", 15, Ink);
            Size(Panel(left, Color.clear, "Spacer"), flexH: 1);

            // bench
            var bench = Panel(main, Plastic, "Bancada");
            Vertical(bench, 14, 10);
            Size(bench, flexW: 1);
            var monitor = Panel(bench, Crt, "Monitor");
            monitor.gameObject.AddComponent<RectMask2D>();
            Size(monitor, minH: 260, flexH: 1);
            screenT = Label(monitor, "", 19, Phosphor, FontStyle.Normal, TextAnchor.UpperLeft);
            screenT.font = mono;
            Stretch(screenT.rectTransform, 16, 12);
            var deskRow = Panel(bench, Color.clear, "Desk");
            Horizontal(deskRow, 0, 10, false);
            led = Panel(deskRow, PlasticLo, "Led").GetComponent<Image>();
            Size(led.transform, w: 14, h: 14);
            hintT = Label(deskRow, "", 15, InkSoft);
            Size(hintT.rectTransform, flexW: 1);
            sheetPanel = Panel(bench, PlasticHi, "Sheet").gameObject;
            Vertical(sheetPanel.transform, 14, 10);
            sheetRoot = sheetPanel.transform;

            // tool rack
            var right = Panel(main, Plastic, "Ferramentas");
            Vertical(right, 14, 8);
            Size(right, w: 320);
            Label(right, "FERRAMENTAS", 14, InkSoft, FontStyle.Bold);
            var tools = Panel(right, Color.clear, "Tools");
            Vertical(tools, 0, 8);
            toolsRoot = tools;
            diagBtn = MakeButton(right, "<b>Dar o diagnóstico</b>", Amber, game.OpenDiagnosis, 19);
            Size(diagBtn.transform, h: 52);
            Size(Panel(right, Color.clear, "Spacer"), flexH: 1);
        }

        // ---------- render ----------

        void Render()
        {
            var S = game.S;
            var c = S.customer;

            dayT.text = $"DIA <b>{S.day}</b>";
            clockT.text = $"HORA <b>{Game.FormatTime(Mathf.Min(S.clock, 23 * 60))}</b>";
            clockT.color = S.clock >= Game.Close - 60 ? Alert : Ink;
            cashT.text = $"CAIXA <b>{Game.Brl(S.cash)}</b>";
            repT.text = $"FAMA <b>{S.rep}/5</b>";
            soundT.text = soundOn ? "Som: ligado" : "Som: desligado";

            avatar.color = Hex(c.colorHex);
            avatarT.text = Initials(c.name);
            nameT.text = c.name;
            var mem = game.Known(c.name);
            string tag = c.kind == CustomerKind.Warranty ? "voltou pela garantia"
                : c.kind == CustomerKind.Redo ? "voltou: o defeito continua"
                : mem != null ? $"{mem.visits + 1}ª visita" : "cliente novo";
            descT.text = $"{c.desc} · {tag}";
            complaintT.text = $"\"{(string.IsNullOrEmpty(c.greet) ? "" : c.greet + " ")}{c.says}\"";
            osT.text = $"ORDEM DE SERVIÇO Nº {S.osNumber:0000}";
            led.color = c.ledOn ? Phosphor : PlasticLo;

            var lines = new List<string> { "BIPE BIOS v1.06 · BANCADA 1", $"PC de {c.name} conectado.", "" };
            foreach (var l in c.log)
            {
                lines.Add($"<color=#4da865>[{l.time}]</color> <color=#f2b33d>> {l.command}</color>");
                lines.Add(l.text);
                lines.Add("");
            }
            lines.Add("C:\\>_");
            screenT.text = string.Join("\n", lines.Skip(Mathf.Max(0, lines.Count - ScreenLines)));

            bool investigating = S.phase == Phase.Investigate;
            Clear(toolsRoot);
            foreach (var t in DefaultContent.Tests)
            {
                bool done = c.done.Contains(t.id);
                var id = t.id;
                var b = MakeButton(toolsRoot, $"{t.label}   <color=#4a463c>{(done ? "feito" : t.minutes + " min")}</color>", done ? PlasticLo : PlasticHi, () => game.RunTest(id), 16);
                b.interactable = investigating && !done;
            }
            diagBtn.interactable = investigating;

            int n = c.done.Count;
            hintT.text = !investigating ? ""
                : n == 0 ? "Cada teste gasta tempo do seu dia. Investigue o suficiente e dê o diagnóstico."
                : $"{n} teste{(n > 1 ? "s" : "")} feito{(n > 1 ? "s" : "")}. Já dá pra cravar, ou investiga mais?";

            RenderSheet();
        }

        void RenderSheet()
        {
            var S = game.S;
            var c = S.customer;
            Clear(sheetRoot);
            sheetPanel.SetActive(S.phase != Phase.Investigate);

            switch (S.phase)
            {
                case Phase.ChooseFix:
                {
                    Label(sheetRoot, "<b>Qual é o defeito?</b>", 20, Ink);
                    var grid = Panel(sheetRoot, Color.clear, "Options");
                    var g = grid.gameObject.AddComponent<GridLayoutGroup>();
                    g.cellSize = new Vector2(390, 44);
                    g.spacing = new Vector2(8, 8);
                    g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    g.constraintCount = 2;
                    foreach (var f in game.Unlocked())
                    {
                        var key = f.key;
                        MakeButton(grid, $"{f.fix}   <color=#4a463c>{Game.Brl(f.price)}</color>", Plastic, () => game.ChooseFix(key), 16);
                    }
                    MakeButton(sheetRoot, "Voltar e investigar mais", Plastic, game.BackToInvestigate, 15);
                    break;
                }
                case Phase.ChoosePrice:
                {
                    var f = game.Fault(S.chosenFix);
                    bool burned = game.CustomerIsBurned;
                    Label(sheetRoot, $"<b>{f.fix}. Quanto cobrar de {c.name}?</b>", 20, Ink);
                    if (f.partCost > 0)
                    {
                        var parts = Row(sheetRoot);
                        MakeButton(parts, $"Peça original · {Game.Brl(f.partCost)}", S.usePyPart ? Plastic : Postit, () => game.SetPyPart(false), 16);
                        MakeButton(parts, $"Peça do Paraguai · {Game.Brl(Game.PyCost(f))}", S.usePyPart ? Postit : Plastic, () => game.SetPyPart(true), 16);
                    }
                    var prices = Row(sheetRoot);
                    MakeButton(prices, $"<b>Preço justo · {Game.Brl(f.price)}</b>", Amber, () => game.Charge(false), 17);
                    var dbl = MakeButton(prices, burned ? "Cobrar o dobro (não cai mais nessa)" : $"Cobrar o dobro · {Game.Brl(f.price * 2)}", Plastic, () => game.Charge(true), 17);
                    dbl.interactable = !burned;
                    Label(sheetRoot, (f.partCost > 0 ? "Peça do Paraguai sai barata, mas nem sempre dura. " : "") + "Cobrar o dobro rende mais, mas o bairro conversa.", 15, InkSoft);
                    break;
                }
                case Phase.Warranty:
                {
                    var f = game.Fault(c.faultKey);
                    Label(sheetRoot, "<b>Garantia: o conserto não durou</b>", 20, Ink);
                    Label(sheetRoot, $"A peça do Paraguai que você usou em \"{f.fix.ToLower()}\" queimou. {c.name} quer o conserto de graça.", 16, InkSoft);
                    var row = Row(sheetRoot);
                    MakeButton(row, $"<b>Honrar a garantia · peça original {Game.Brl(f.partCost)}</b>", Amber, () => game.Warranty(true), 16);
                    MakeButton(row, "Negar: \"foi mau uso\"", Plastic, () => game.Warranty(false), 16);
                    break;
                }
                case Phase.Result:
                {
                    var r = S.last;
                    var verdict = Panel(sheetRoot, r.good ? Tint(Good) : Tint(Alert), "Verdict");
                    Vertical(verdict, 12, 4);
                    Label(verdict, string.Join("\n", r.lines), 17, Ink);
                    var row = Row(sheetRoot);
                    Label(row, $"{(r.delta >= 0 ? "+" : "")}{Game.Brl(r.delta)} · fama {(r.repDelta > 0 ? "+" : "")}{r.repDelta}", 20, Ink, FontStyle.Bold);
                    MakeButton(row, game.ClosingSoon ? "<b>Fechar a loja</b>" : "<b>Chamar o próximo</b>", Amber, game.Continue, 18);
                    break;
                }
                case Phase.DayEnd:
                {
                    var st = S.stats;
                    Label(sheetRoot, $"<b>Fim do dia {S.day}</b>", 22, Ink);
                    Label(sheetRoot,
                        $"Atendidos: {st.served}    Acertos: {st.right}/{st.served}    Faturou: {Game.Brl(st.earned)}\n" +
                        $"Aluguel: -{Game.Brl(Game.Rent)}    Caixa: {Game.Brl(S.cash)}    Clientes conhecidos: {S.known.Count}", 17, Ink);
                    if (S.day == 1)
                        Label(sheetRoot, "Amanhã chegam defeitos novos: superaquecimento, bateria da placa-mãe e conflito de IRQ. E quem você atendeu hoje pode voltar.", 15, InkSoft);
                    MakeButton(sheetRoot, $"<b>Abrir a loja no dia {S.day + 1}</b>", Amber, game.NextDay, 18);
                    break;
                }
                case Phase.GameOver:
                {
                    Label(sheetRoot, $"<b>Fim de jogo no dia {S.day}</b>", 22, Ink);
                    Label(sheetRoot, S.gameOverReason, 17, Alert);
                    MakeButton(sheetRoot, "<b>Abrir uma loja nova</b>", Amber, game.NewGame, 18);
                    break;
                }
            }
        }

        void ToggleSound()
        {
            soundOn = !soundOn;
            Render();
        }

        // ---------- helpers ----------

        static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
        static Color Tint(Color c) => Color.Lerp(c, Color.white, 0.75f);

        static string Initials(string name)
        {
            foreach (var prefix in new[] { "Dona ", "Seu ", "Dr. ", "Tia ", "Vó ", "Pastor " })
                if (name.StartsWith(prefix)) name = name.Substring(prefix.Length);
            return name.Substring(0, Mathf.Min(2, name.Length)).ToUpper();
        }

        RectTransform Panel(Transform parent, Color color, string name = "Panel")
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = color.a > 0;
            return (RectTransform)go.transform;
        }

        Text Label(Transform parent, string text, int size, Color color, FontStyle style = FontStyle.Normal, TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = anchor;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        Text Stat(Transform parent)
        {
            var t = Label(parent, "", 19, Ink, FontStyle.Normal, TextAnchor.MiddleCenter);
            t.font = mono;
            Size(t.rectTransform, w: 170);
            return t;
        }

        Button MakeButton(Transform parent, string label, Color bg, UnityAction onClick, int size)
        {
            var rt = Panel(parent, bg, "Button");
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = rt.GetComponent<Image>();
            var colors = b.colors;
            colors.highlightedColor = new Color(0.92f, 0.92f, 0.92f);
            colors.disabledColor = new Color(0.75f, 0.75f, 0.75f, 0.7f);
            b.colors = colors;
            var t = Label(rt, label, size, Ink);
            Stretch(t.rectTransform, 12, 4);
            Size(rt, h: 44, flexW: 1);
            b.onClick.AddListener(onClick);
            return b;
        }

        Transform Row(Transform parent)
        {
            var row = Panel(parent, Color.clear, "Row");
            Horizontal(row, 0, 8, false);
            return row;
        }

        static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                var child = t.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }

        static void Stretch(RectTransform rt, float padX = 0, float padY = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padX, padY);
            rt.offsetMax = new Vector2(-padX, -padY);
        }

        static void Vertical(Transform t, int padding, float spacing)
        {
            var g = t.gameObject.AddComponent<VerticalLayoutGroup>();
            g.padding = new RectOffset(padding, padding, padding, padding);
            g.spacing = spacing;
            g.childControlWidth = true;
            g.childControlHeight = true;
            g.childForceExpandWidth = true;
            g.childForceExpandHeight = false;
        }

        static void Horizontal(Transform t, int padding, float spacing, bool center)
        {
            var g = t.gameObject.AddComponent<HorizontalLayoutGroup>();
            g.padding = new RectOffset(padding, padding, padding / 2, padding / 2);
            g.spacing = spacing;
            g.childAlignment = center ? TextAnchor.MiddleLeft : TextAnchor.UpperLeft;
            g.childControlWidth = true;
            g.childControlHeight = true;
            g.childForceExpandWidth = false;
            g.childForceExpandHeight = !center;
        }

        static void Size(Transform t, float w = -1, float h = -1, float flexW = -1, float flexH = -1, float minH = -1)
        {
            // no ?? here: Unity's fake-null objects slip through it
            var le = t.GetComponent<LayoutElement>();
            if (le == null) le = t.gameObject.AddComponent<LayoutElement>();
            if (w >= 0) { le.preferredWidth = w; le.minWidth = w; }
            if (h >= 0) { le.preferredHeight = h; le.minHeight = h; }
            if (flexW >= 0) le.flexibleWidth = flexW;
            if (flexH >= 0) le.flexibleHeight = flexH;
            if (minH >= 0) le.minHeight = minH;
        }
    }
}
