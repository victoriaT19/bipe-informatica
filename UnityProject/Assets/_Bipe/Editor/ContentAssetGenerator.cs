using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bipe.EditorTools
{
    // Bipe > Gerar fichas de defeitos: turns the built-in faults into editable assets in Resources/Defeitos
    public static class ContentAssetGenerator
    {
        const string Folder = "Assets/_Bipe/Resources/Defeitos";

        [MenuItem("Bipe/Gerar fichas de defeitos")]
        static void Generate()
        {
            Directory.CreateDirectory(Folder);
            AssetDatabase.Refresh();
            int created = 0;
            foreach (var f in DefaultContent.Faults())
            {
                string path = $"{Folder}/{f.key}.asset";
                if (AssetDatabase.LoadAssetAtPath<FaultDefinition>(path) != null)
                {
                    Object.DestroyImmediate(f);
                    continue;
                }
                AssetDatabase.CreateAsset(f, path);
                created++;
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Bipe Informática", $"{created} fichas criadas em {Folder}. As que já existiam foram mantidas.", "OK");
        }
    }
}
