using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ExtraTexImportSettings;

public class ReimportNormalMaps : EditorWindow
{
    public int StartIndex = 0;
    public int EndIndex = -1;

    [MenuItem("Stress Level Zero/Reimport Normal Maps")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        ReimportNormalMaps window = (ReimportNormalMaps)EditorWindow.GetWindow(typeof(ReimportNormalMaps));
        window.Show();
    }

    void OnGUI()
    {
        StartIndex = EditorGUILayout.IntField("Start Importing from index", StartIndex);
        EndIndex = EditorGUILayout.IntField("End Importing at index", EndIndex);
        if (GUILayout.Button("Create Normal List"))
        {
            CreateNormalList();
        }
        if (GUILayout.Button("Reimport all normal maps"))
        {
            ReimportAllNormals();
        }

    }

    void CreateNormalList()
    {
        string[] AllTextureGUIDs = AssetDatabase.FindAssets("t:Texture2D");
        string fileOut = "";
        for (int i = 0; i < AllTextureGUIDs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("Compiling List of normal/detail maps",
                string.Format("{0} out of {1}", i+1, AllTextureGUIDs.Length), (float)i/(float)AllTextureGUIDs.Length);
            string path = AssetDatabase.GUIDToAssetPath(AllTextureGUIDs[i]);
            bool isDetail = false;
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.NormalMap && importer.userData != null && importer.userData.Length > 0)
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
            
            if (importer != null && (importer.textureType == TextureImporterType.NormalMap || isDetail))
            {
                fileOut += path;
                if (i != AllTextureGUIDs.Length - 1)
                {
                    fileOut += "\n";
                }
            }
        }
        EditorUtility.ClearProgressBar();
        StreamWriter writer = new StreamWriter("Assets/NormalMapList.txt", false);
        writer.Write(fileOut);
        writer.Close();
    }

    void ReimportAllNormals()
    {
        if (!File.Exists(Path.Combine(Application.dataPath, "NormalMapList.txt")))
        {
            EditorUtility.DisplayDialog("No file list", "No normal map file list, try regenerating the normal map list", "ok");
            return;
        }
        StreamReader reader = new StreamReader("Assets/NormalMapList.txt");
        string wholeFile = reader.ReadToEnd();
        string[] fileArray = wholeFile.Split('\n');
        Debug.Log(fileArray.Length);
        reader.Close();
        //string[] AllTextureGUIDs = AssetDatabase.FindAssets("t:Texture2D");
        int end = EndIndex > 1 ? Mathf.Min(EndIndex, fileArray.Length) : fileArray.Length;
        if (fileArray.Length == 0)
        {
            EditorUtility.DisplayDialog("Empty file list", "No normal maps in file list, try regenerating the normal map list", "ok");
            return;
        }
        
        for (int i = StartIndex; i < end; i++)
        {
            string path = fileArray[i];
            
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;


            if (importer != null)
            {
                    //Texture2D tex = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
                   
                    EditorUtility.SetDirty(importer);
                    //EditorUtility.SetDirty(tex);

                    //AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                    importer.SaveAndReimport();

            }
            else
            {
                Debug.Log("Invalid Importer");
            }
            
        }

    }
}
