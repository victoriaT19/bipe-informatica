using System.Collections.Generic;
using UnityEngine;

namespace Bipe
{
    // Built-in content, used when Resources/Defeitos has no fault assets (menu Bipe > Gerar fichas de defeitos creates them)
    public static class DefaultContent
    {
        public static readonly TestDef[] Tests =
        {
            new TestDef { id = TestId.Talk,      label = "Conversar com o cliente",     minutes = 5,  command = "CONVERSA" },
            new TestDef { id = TestId.Power,     label = "Ligar o PC",                  minutes = 5,  command = "POWER ON" },
            new TestDef { id = TestId.Open,      label = "Abrir o gabinete",            minutes = 10, command = "ABRIR GABINETE" },
            new TestDef { id = TestId.Psu,       label = "Multímetro na fonte",         minutes = 10, command = "MULTIMETRO" },
            new TestDef { id = TestId.Antivirus, label = "Antivírus no disquete",       minutes = 20, command = "A:\\VACINA.EXE C:" },
            new TestDef { id = TestId.ScanDisk,  label = "Rodar o ScanDisk",            minutes = 25, command = "SCANDISK C: /SURFACE" },
            new TestDef { id = TestId.Devices,   label = "Gerenciador de Dispositivos", minutes = 10, command = "SISTEMA > DISPOSITIVOS" },
        };

        public static readonly PersonDef[] People =
        {
            P("Dona Cida", "aposentada, usa pra jogar Paciência"),
            P("Seu Valdir", "dono da mecânica da esquina"),
            P("Marquinhos", "12 anos, veio com a mãe"),
            P("Dr. Hélio", "dentista, agenda no computador"),
            P("Tia Neide", "faz convites de casamento"),
            P("Jéssica", "estuda pro vestibular"),
            P("Kleber", "tem uma banda de garagem"),
            P("Seu Ming", "da pastelaria, controla o estoque"),
            P("Pastor Edson", "imprime o boletim da igreja"),
            P("Renata", "contadora, muito apressada"),
            P("Gilmar", "acha que entende de informática"),
            P("Vó Zulmira", "ganhou do neto no Natal"),
        };

        public static readonly string[] Colors = { "#a8432f", "#2e6f9e", "#6b4f8a", "#3f7a3b", "#9a6a1e", "#7a3a5a", "#2f6e6a", "#5a5a2a" };

        public static readonly string[] GenericSays = { "Ele não funciona mais.", "Tá dando pau, moço.", "Estragou e eu preciso dele pra amanhã!", "Não sei explicar, só sei que não vai." };
        public static readonly string[] GenericTalk = { "\"Eu não mexi em nada, juro.\"", "\"Tava ótimo ontem.\"", "\"Meu sobrinho disse que é a placa-mãe.\"", "\"Quanto vai ficar isso, hein?\"" };
        // {0} = the fix, in lower case
        public static readonly string[] WarrantySays = { "Moço, aquele conserto ({0}) não durou nem dois dias!", "Voltou o mesmo defeito. Você colocou peça boa mesmo?", "Tá igualzinho a antes. Isso tem garantia, né?" };
        public static readonly string[] RedoSays = { "Da última vez você não resolveu nada. Continua igual!", "Voltei. E agora, vai acertar?", "Paguei nada, mas perdi a viagem. Olha de novo, por favor." };
        public static readonly string[] HappyGreets = { "Voltei! Da última vez você salvou minha vida.", "Indiquei vocês pra vizinha toda!", "Só confio em vocês agora." };
        public static readonly string[] TamperEvidence =
        {
            "Os parafusos do gabinete estão espanados: alguém abriu isso aqui há pouco tempo.",
            "Tem uma chave de fenda de cozinha esquecida dentro do gabinete.",
            "Falta um parafuso da tampa, e tem marca de dedo na poeira.",
        };

        // the shop opens with these; the rest are bought at the end of a day
        public static readonly TestId[] StartingTools = { TestId.Talk, TestId.Power, TestId.Open, TestId.Devices };
        public static readonly ToolOffer[] Shop =
        {
            new ToolOffer { id = TestId.Psu,       name = "Multímetro",                    price = 40, desc = "mede as tensões da fonte" },
            new ToolOffer { id = TestId.Antivirus, name = "Disquete de antivírus",         price = 30, desc = "acha vírus no setor de boot" },
            new ToolOffer { id = TestId.ScanDisk,  name = "Disquete de boot com ScanDisk", price = 35, desc = "procura setores defeituosos no HD" },
        };

        public static List<FaultDefinition> Faults()
        {
            return new List<FaultDefinition>
            {
                F("ram", "Reencaixar a memória RAM", 25, 0, 1, true,
                    new[] { "Ele fica apitando que nem alarme de carro.", "Liga, faz piiii, piiii, e não aparece nada." },
                    new[] { "\"Abri pra tirar o pó com o aspirador semana passada.\"", "\"Meu filho quis ver como era por dentro...\"" },
                    "L L L L L", "Bipes longos sem parar. Monitor sem sinal.",
                    open: "Um pente de memória está meio solto no slot, torto."),
                F("video", "Reencaixar a placa de vídeo", 25, 0, 1, true,
                    new[] { "Liga, apita diferente e a tela não acende.", "Faz um bipe comprido e uns curtinhos." },
                    new[] { "\"Mudei ele de mesa e ele deu uma batidinha no chão.\"", "\"O técnico do meu cunhado mexeu nele ontem.\"" },
                    "L S S", "1 bipe longo, 2 curtos. Monitor sem sinal.",
                    open: "A placa de vídeo está levantada, metade fora do slot PCI."),
                F("psu", "Trocar a fonte", 90, 45, 1, true,
                    new[] { "Aperto o botão e nada. Morto.", "Ele está com cheiro de churrasco." },
                    new[] { "\"Caiu um raio aqui perto ontem à noite.\"", "\"A luz piscou e ele fez um estalo.\"" },
                    "", "Nada. Nem bipe, nem LED, nem cooler girando.",
                    open: "Cheiro forte de queimado vindo da fonte. O resto parece normal.",
                    psu: "+12V: 0,0V   +5V: 0,0V   Cheiro de queimado na fonte."),
                F("virus", "Remover o vírus", 40, 0, 1, false,
                    new[] { "Aparece uma mensagem estranha quando liga.", "Meus trabalhos da faculdade sumiram!" },
                    new[] { "\"Peguei um disquete de joguinhos com meu primo.\"", "\"Instalei um programa que veio numa revista.\"" },
                    "S", "Bipe curto. Na tela: \"SEU PC AGORA E MEU ;)\" e depois o Windows 95 carrega.",
                    av: "ALERTA: vírus PAMONHA.BOOT encontrado no setor de boot!"),
                F("hd", "Trocar o HD", 180, 110, 1, false,
                    new[] { "Trava na tela das nuvenzinhas do Windows.", "Faz um barulho tec-tec e congela." },
                    new[] { "\"Ele faz um barulhinho de relógio às vezes.\"", "\"Tá ficando cada vez mais lento pra abrir.\"" },
                    "S", "Bipe curto. Começa a carregar o Windows 95 e trava na abertura.",
                    open: "Dá pra ouvir um \"tec... tec... tec\" vindo do HD.",
                    scan: "327 clusters defeituosos encontrados. Disco em estado crítico.",
                    av: "Varredura interrompida: erro de leitura na unidade C:",
                    dev: "Não dá: o Windows trava antes de abrir."),
                F("user", "Explicar pro cliente", 15, 0, 1, false,
                    new[] { "A tela fica preta o tempo todo!", "Comprei ontem e já veio com defeito." },
                    new[] { "\"O monitor eu deixo desligado no filtro de linha, pra não gastar luz.\"", "\"Esse botão grande na tela eu nunca liguei, precisa?\"" },
                    "S", "Bipe curto. Tudo liga normal aqui na bancada. O cliente parece confuso."),
                F("cooler", "Limpar e trocar o cooler", 35, 10, 2, false,
                    new[] { "Ele desliga sozinho do nada.", "Fica muito quente e apaga." },
                    new[] { "\"Ele desliga mais à tarde, quando esquenta.\"", "\"O gato adora dormir em cima dele.\"" },
                    "S", "Bipe curto. Windows carrega normal... e 2 minutos depois o PC desliga sozinho.",
                    open: "Cooler do processador travado de poeira e pelo de gato."),
                F("cmos", "Trocar a bateria da placa-mãe", 20, 5, 2, false,
                    new[] { "A hora tá sempre errada.", "Toda vez pede pra apertar F1." },
                    new[] { "\"Ele esquece a data toda vez que tiro da tomada.\"", "\"Meu relógio do Windows diz que é 1980.\"" },
                    "S", "Bipe curto. \"CMOS checksum error - Defaults loaded. Press F1\". Data: 01/01/1980.",
                    open: "A bateria redonda da placa-mãe está oxidada."),
                F("irq", "Reconfigurar IRQ nos jumpers", 45, 0, 2, false,
                    new[] { "O som parou de funcionar.", "Meu CD de pagode não toca mais." },
                    new[] { "\"Instalei uma placa de rede pra jogar com os amigos.\"", "\"Coloquei uma placa nova que comprei na feira.\"" },
                    "S", "Bipe curto. Windows carrega. Nenhum som de abertura.",
                    dev: "CONFLITO: placa de som e placa de rede usando a mesma IRQ 5."),
            };
        }

        static PersonDef P(string name, string desc) => new PersonDef { name = name, desc = desc };

        static FaultDefinition F(string key, string fix, int price, int part, int day, bool noPost,
            string[] says, string[] talk, string beep, string power,
            string open = null, string psu = null, string av = null, string scan = null, string dev = null)
        {
            var f = ScriptableObject.CreateInstance<FaultDefinition>();
            f.name = key;
            f.key = key;
            f.fix = fix;
            f.price = price;
            f.partCost = part;
            f.unlockDay = day;
            f.noPost = noPost;
            f.says = new List<string>(says);
            f.talk = new List<string>(talk);
            f.powerBeep = beep;
            f.powerText = power;
            f.open = open;
            f.psu = psu;
            f.antivirus = av;
            f.scanDisk = scan;
            f.devices = dev;
            return f;
        }
    }
}
