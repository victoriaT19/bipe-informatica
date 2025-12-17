using System;
using System.Collections.Generic;

namespace Bipe
{
    public enum TestId { Talk, Power, Open, Psu, Antivirus, ScanDisk, Devices }
    public enum CustomerKind { New, Regular, Redo, Warranty }
    public enum Phase { Investigate, ChooseFix, ChoosePrice, Warranty, Result, DayEnd, GameOver }

    [Serializable]
    public class TestDef
    {
        public TestId id;
        public string label;
        public int minutes;
        public string command;
    }

    [Serializable]
    public class ToolOffer
    {
        public TestId id;
        public string name;
        public int price;
        public string desc;
    }

    [Serializable]
    public class PersonDef
    {
        public string name;
        public string desc;
    }

    [Serializable]
    public class LogLine
    {
        public string time;
        public string command;
        public string text;
    }

    [Serializable]
    public class Customer
    {
        public string name;
        public string desc;
        public string colorHex;
        public string faultKey;
        // hidden second defect; empty when there is only one
        public string secondKey = "";
        // tells a misleading story when you talk; opening the case shows someone tampered with it
        public bool liar;
        public CustomerKind kind;
        public string greet = "";
        public string says = "";
        public bool ledOn;
        public List<TestId> done = new List<TestId>();
        public List<LogLine> log = new List<LogLine>();
    }

    // What the neighbourhood remembers about someone you served
    [Serializable]
    public class KnownCustomer
    {
        public string name;
        public string desc;
        public string colorHex;
        public int visits;
        public bool happy;
        public bool burned;
    }

    [Serializable]
    public class PendingReturn
    {
        public string name;
        public string desc;
        public string colorHex;
        public string faultKey;
        public CustomerKind kind;
        public int dueDay;
    }

    [Serializable]
    public class DayStats
    {
        public int served;
        public int right;
        public int wrong;
        public int earned;
    }

    [Serializable]
    public class Outcome
    {
        public bool good;
        public int delta;
        public int repDelta;
        public List<string> lines = new List<string>();
    }

    // Everything needed to resume a game; saved as JSON after every change
    [Serializable]
    public class GameState
    {
        public int day;
        public int clock;
        public int cash;
        public int rep;
        public int osNumber;
        public Phase phase;
        public Customer customer;
        public DayStats stats = new DayStats();
        public Outcome last;
        public List<string> chosenFixes = new List<string>();
        public List<TestId> tools = new List<TestId>();
        public bool usePyPart;
        public string gameOverReason = "";
        public List<KnownCustomer> known = new List<KnownCustomer>();
        public List<PendingReturn> returns = new List<PendingReturn>();
    }
}
