using System.Collections.Generic;
using UnityEngine;

namespace Bipe
{
    // One defect the shop can receive. Empty test answers fall back to the "everything is fine" text in Game.ResultFor.
    [CreateAssetMenu(menuName = "Bipe/Defeito", fileName = "Defeito")]
    public class FaultDefinition : ScriptableObject
    {
        [Header("Identificação")]
        public string key;
        public string fix;
        public int price;
        public int partCost;
        public int unlockDay = 1;

        [Tooltip("O PC morre antes de terminar o teste de inicialização, então antivírus, ScanDisk e Gerenciador não rodam.")]
        public bool noPost;

        [Header("O que o cliente diz")]
        [TextArea] public List<string> says = new List<string>();
        [TextArea] public List<string> talk = new List<string>();

        [Header("Ligar o PC")]
        [Tooltip("S = bipe curto, L = bipe longo, separados por espaço. Vazio = silêncio total.")]
        public string powerBeep;
        [TextArea] public string powerText;

        [Header("Respostas dos testes (vazio = resultado normal)")]
        [TextArea] public string open;
        [TextArea] public string psu;
        [TextArea] public string antivirus;
        [TextArea] public string scanDisk;
        [TextArea] public string devices;
    }
}
