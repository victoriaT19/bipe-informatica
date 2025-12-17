using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Bipe.EditorTools
{
    // Bipe > Gerar executável (Windows) and Bipe > Gerar versão web: builds into UnityProject/Builds.
    // The game builds its screen from code, so a build only needs one empty scene.
    static class BuildTool
    {
        const string ScenePath = "Assets/_Bipe/Scenes/Loja.unity";

        [MenuItem("Bipe/Gerar executável (Windows)")]
        static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "Builds/Windows/BipeInformatica.exe", BuildOptions.None);

        // Opens in the browser through Unity's local server when done; the folder itself is what goes to itch.io
        [MenuItem("Bipe/Gerar versão web")]
        static void BuildWeb()
        {
            // works on any host, including ones that don't send compression headers
            PlayerSettings.WebGL.decompressionFallback = true;
            Build(BuildTarget.WebGL, "Builds/Web", BuildOptions.AutoRunPlayer);
        }

        static void Build(BuildTarget target, string outputPath, BuildOptions options)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EnsureScene();

            PlayerSettings.productName = "Bipe Informática";
            PlayerSettings.companyName = "BipeInformatica";

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputPath,
                target = target,
                options = options,
            });

            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Bipe: build gerado em {Path.GetFullPath(outputPath)} ({summary.totalSize / (1024 * 1024)} MB).");
                EditorUtility.RevealInFinder(outputPath);
            }
            else
            {
                Debug.LogError($"Bipe: o build falhou ({summary.result}, {summary.totalErrors} erro(s)). Veja o Console.");
            }
        }

        static void EnsureScene()
        {
            if (!File.Exists(ScenePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }
    }
}
