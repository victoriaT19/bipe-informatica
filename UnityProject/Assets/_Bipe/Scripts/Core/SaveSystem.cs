using System;
using System.IO;
using UnityEngine;

namespace Bipe
{
    public static class SaveSystem
    {
        static string FilePath => Path.Combine(Application.persistentDataPath, "bipe-save.json");

        public static void Save(GameState s)
        {
            try { File.WriteAllText(FilePath, JsonUtility.ToJson(s, true)); }
            catch (Exception e) { Debug.LogWarning($"Não consegui salvar o jogo: {e.Message}"); }
        }

        public static GameState Load()
        {
            try { return File.Exists(FilePath) ? JsonUtility.FromJson<GameState>(File.ReadAllText(FilePath)) : null; }
            catch (Exception e) { Debug.LogWarning($"Save ignorado: {e.Message}"); return null; }
        }
    }
}
