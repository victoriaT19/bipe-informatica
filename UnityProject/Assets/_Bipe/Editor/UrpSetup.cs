using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Bipe.EditorTools
{
    // Switches the project to URP with the 2D renderer the first time the editor loads it.
    // Also available as Bipe > Configurar URP 2D.
    [InitializeOnLoad]
    static class UrpSetup
    {
        const string Folder = "Assets/_Bipe/Rendering";
        const string PipelinePath = Folder + "/BipeURP.asset";
        const string RendererPath = Folder + "/BipeURP_Renderer2D.asset";

        static UrpSetup()
        {
            EditorApplication.delayCall += () =>
            {
                if (!(GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset)) Configure();
            };
        }

        [MenuItem("Bipe/Configurar URP 2D")]
        static void Configure()
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Bipe", "Rendering");
                pipeline = UniversalRenderPipelineAsset.Create(Create2DRenderer());
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            int current = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(current, false);

            AssetDatabase.SaveAssets();
            Debug.Log($"Bipe: projeto configurado com URP 2D ({PipelinePath}).");
        }

        static ScriptableRendererData Create2DRenderer()
        {
            // same internal call URP's own "URP Asset (with 2D Renderer)" menu uses, so default materials are filled in
            var menus = Type.GetType("UnityEditor.Rendering.Universal.Renderer2DMenus, Unity.RenderPipelines.Universal.Editor");
            var create = menus?.GetMethod("CreateRendererAsset", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (create != null)
            {
                var rendererType = Enum.Parse(create.GetParameters()[1].ParameterType, "_2DRenderer");
                return (ScriptableRendererData)create.Invoke(null, new object[] { RendererPath, rendererType, false, "Renderer" });
            }

            Debug.LogWarning("Bipe: menu interno do URP não encontrado; criando o renderizador 2D direto.");
            var data = ScriptableObject.CreateInstance<Renderer2DData>();
            AssetDatabase.CreateAsset(data, RendererPath);
            ResourceReloader.ReloadAllNullIn(data, "Packages/com.unity.render-pipelines.universal");
            return data;
        }
    }
}
