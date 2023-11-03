using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SLZ.SLZEditorTools
{
    public class CreateNMPPAssetBundles
    {
        const string dir = "Packages/com.unity.render-pipelines.core/Editor/AssetPostProcessors/NormalMapPostProcessor";
#if SLZ_RENDERPIPELINE_DEV
        [MenuItem("Tools/ErrorTools/Build Normal Map Importer Asset Bundles")]
#endif
        static void CreateBundles()
        {
            string sysDir = Path.Combine(Path.GetDirectoryName(Application.dataPath), dir);
            if (!Directory.Exists(sysDir))
            {
                Debug.LogError("Can only rebuild NMPP bundle if core pipeline is in local packages folder");
                return;
            }
            AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
            buildMap[0].assetBundleName = "normalencoderbundle";
            //buildMap[1].assetBundleName = "normalencoderbundle";
            buildMap[0].assetNames = new string[] { 
                "Packages/com.unity.render-pipelines.core/Editor/AssetPostProcessors/NormalMapPostProcessor/NormalEncoder.compute",
                "Packages/com.unity.render-pipelines.core/Editor/AssetPostProcessors/NormalMapPostProcessor/NMPP_Blit_LOD.shader" };

            BuildPipeline.BuildAssetBundles(dir, buildMap, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows);
            buildMap[0].assetBundleName = "normalencoderbundle_android";
            BuildPipeline.BuildAssetBundles(dir, buildMap, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.Android);

            File.Delete(Path.Combine(sysDir, "NormalMapPostProcessor"));
            File.Delete(Path.Combine(sysDir, "NormalMapPostProcessor.manifest"));
        }
    }
}
