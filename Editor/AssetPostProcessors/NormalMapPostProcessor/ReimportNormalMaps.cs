using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ExtraTexImportSettings;

public class ReimportNormalMaps : EditorWindow
{
    [MenuItem("Stress Level Zero/Reimport Normal Maps")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        ReimportNormalMaps window = (ReimportNormalMaps)EditorWindow.GetWindow(typeof(ReimportNormalMaps));
        window.Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Reimport all normal maps"))
        {
            ReimportAllNormals();
        }

    }

    void ReimportAllNormals()
    {
        string[] AllTextureGUIDs = AssetDatabase.FindAssets("t:Texture2D");
        
        for (int i = 0; i < AllTextureGUIDs.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(AllTextureGUIDs[i]);
            
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;


            if (importer != null)
            {
                bool isDetail = false;
                if (importer.userData != null && importer.userData.Length > 0)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ExtraTextureSettings));
                    StringReader sr = new StringReader(importer.userData);
                    ExtraTextureSettings texSettings;

                    try
                    {
                        texSettings = (ExtraTextureSettings)serializer.Deserialize(sr);
                        isDetail = texSettings.detailMap;
                    }
                    catch
                    {
                        isDetail = Path.GetFileNameWithoutExtension(path).ToLower().EndsWith("_detailmap");
                    }
                }
                else
                {
                    isDetail = Path.GetFileNameWithoutExtension(path).ToLower().EndsWith("_detailmap");
                }

                if (importer.textureType == TextureImporterType.NormalMap || isDetail)
                {
                    Texture2D tex = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
                   
                    EditorUtility.SetDirty(importer);
                    EditorUtility.SetDirty(tex);

                    AssetDatabase.ImportAsset(path);
                    //importer.SaveAndReimport();
                }
            }
        }

    }
}
