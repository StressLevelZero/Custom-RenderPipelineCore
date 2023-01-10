using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor;
using ExtraTexImportSettings;

namespace NormalImportUtil
{
    /// <summary>
    /// Stupid hack to get around unity not garbage collecting until editor scripts have completely finished executing,
    /// this class contains a static method to be called by the editor update loop that does a countdown to give the
    /// editor time to start garbage collection, then executes a static method to reimport a block of normals from the
    /// normal list that also re-adds the countdown method to the editor update loop if there are more items in the
    /// list.
    /// </summary>
    public class ReimportBreakpoint
    {
        public static int blockStart;
        public static float breakTime;
        public static float time;

        public static void importBreakpoint()
        {
            if (time > breakTime)
            {
                EditorApplication.update -= importBreakpoint;
                time = 0.0f;


                EditorUtility.UnloadUnusedAssetsImmediate();
                UnityEngine.Scripting.GarbageCollector.CollectIncremental(1000000000uL);
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers();
                System.GC.Collect();

                normalImporter.ReimportNormals(blockStart);
            }

            if (!EditorApplication.isCompiling && !EditorApplication.isUpdating)
            {
                time += 1.0f;
            }
        }
    }

    public class normalImporter
    {
        public static int blockSize;
        public static int startIndex;
        public static int endIndex;
        public static void ReimportNormals(int index)
        {
            int EndIndex = index + blockSize;
            
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
            reader.Dispose();
            //string[] AllTextureGUIDs = AssetDatabase.FindAssets("t:Texture2D");
            int end = endIndex > 1 ? Mathf.Min(Mathf.Min(EndIndex, endIndex), fileArray.Length): Mathf.Min(EndIndex, fileArray.Length);
            if (fileArray.Length == 0)
            {
                EditorUtility.DisplayDialog("Empty file list", "No normal maps in file list, try regenerating the normal map list", "ok");
                return;
            }
            int minEnd = endIndex > 1 ? Mathf.Min(endIndex, fileArray.Length) : fileArray.Length;
            for (int i = index; i < end; i++)
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
                   
                    Debug.Log(string.Format("{0:F2}%, Imported {1}", 100.0f * (float)(i - startIndex + 1) / (minEnd - startIndex), path));
                }
                else
                {
                    Debug.Log("Invalid Importer");
                }

            }

            index += blockSize;
            
            if ((index < minEnd) && (blockSize > 1))
            {
                Debug.Log("Readding timer");
                ReimportBreakpoint.blockStart = index;
                EditorApplication.update += ReimportBreakpoint.importBreakpoint;
            }


        }
    }

    public class ReimportNormalMaps : EditorWindow
    {
        public int ImportBlockSize = 10; 
        public int WaitTicks = 2000;

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
            ImportBlockSize = EditorGUILayout.IntField("Num files to import at once", ImportBlockSize);
            WaitTicks = EditorGUILayout.IntField("Pause time (editor ticks)", WaitTicks);
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
            string artifactOut = "";
            for (int i = 0; i < AllTextureGUIDs.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Compiling List of normal/detail maps",
                    string.Format("{0} out of {1}", i + 1, AllTextureGUIDs.Length), (float)i / (float)AllTextureGUIDs.Length);
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
                        // artifactOut += "\n";
                    }
                }
            }
            EditorUtility.ClearProgressBar();
            StreamWriter writer = new StreamWriter("Assets/NormalMapList.txt", false);
            writer.Write(fileOut);
            writer.Close();
            /*
            StreamWriter writer2 = new StreamWriter("Assets/NormalMapArtifacts.txt", false);
            writer2.Write(artifactOut);
            writer2.Close();
            */
        }

        void ReimportAllNormals()
        {

            normalImporter.blockSize = ImportBlockSize;
            normalImporter.startIndex = StartIndex;
            normalImporter.endIndex = EndIndex;
            ReimportBreakpoint.time = 0;
            ReimportBreakpoint.blockStart = StartIndex;
            ReimportBreakpoint.breakTime = WaitTicks;
            normalImporter.ReimportNormals(StartIndex);
            /*
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
                    Resources.UnloadUnusedAssets();
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                }
                else
                {
                    Debug.Log("Invalid Importer");
                }

            }
            */

        }
    }
}
