using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bipe
{
    // Starts the game in any scene: press Play and the shop opens, no scene setup needed
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Start()
        {
            if (Object.FindAnyObjectByType<GameUI>() != null) return;

            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)) { tag = "MainCamera" };
                var cam = camGo.GetComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = GameUI.Wall;
                cam.orthographic = true;
            }
            if (Object.FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var assets = Resources.LoadAll<FaultDefinition>("Defeitos");
            var faults = assets.Length > 0 ? assets.ToList() : DefaultContent.Faults();
            var game = new Game(faults, SaveSystem.Load());

            var go = new GameObject("Bipe Informática");
            var speaker = go.AddComponent<BeepSpeaker>();
            go.AddComponent<GameUI>().Init(game, speaker);
        }
    }
}
