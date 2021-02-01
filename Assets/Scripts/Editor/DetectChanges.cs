using UnityEditor;
using UnityEngine;
using UJ.Data.Editor;
using UJ.Data;
using System.Collections.Generic;
using System.IO;

public class DetectChanges : AssetPostprocessor
{

    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {

        foreach(var str in deletedAssets)
        {
         //   UpdateAI_Graph(str);
        }

        foreach (string str in importedAssets)
        {

            // Debug.Log("Reimported Asset: " + str);
            string[] splitStr = str.Split('/', '.');

            string folder, fileName = "";
            if (splitStr.Length > 2)
            {
                folder = splitStr[splitStr.Length - 3];
            }
            if (splitStr.Length > 1)
            {
                fileName = splitStr[splitStr.Length - 2];
            }
            string extension = splitStr[splitStr.Length - 1];

            //   Debug.Log("File name: " + fileName);
            //    Debug.Log("File type: " + extension);
            var dataImportSetting = DataImporterSetting.Instance;

         //   UpdateAI_Graph(str);

            if (str.StartsWith(dataImportSetting.dataScriptBasePath))
            {
                if (extension.ToLower() == "cs" &&
                    (fileName != "ClassPartials") && (fileName != "KindStr"))
                {
                    GameDataImporterEditor.GenerateCode();
                }
            }

            if (str.StartsWith(dataImportSetting.excelBasePath))
            {

                if (extension.ToLower() == "xlsx")
                {
                    Debug.Log("Got " + str);

                    if (dataImportSetting.dataExcelName == fileName)
                    {
                        var obj = dataImportSetting.LoadXlsx<GameData>(str);
                        GameDataImporterEditor.MakeStrXlsx();

                        AssetDatabase.CreateAsset(obj, dataImportSetting.dataAssetOutputPath + "data.asset");

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();

                        //          var writeInfo = new writeInfo();
                        //        writeInfo.ObjectToExcel<Str>(path + "/Strs.xlsx", obj.Strs);

                    }
                    if (fileName == "Strs")
                    {
                        var setting = DataImporterSetting.Instance;

                        setting.LoadStrs();
                    }

                    if (fileName == "EtcStrs")
                    {
                        var setting = DataImporterSetting.Instance;

                        setting.LoadEtcStrs();

                    }

                }
            }

        }

        /*
        foreach (string str in deletedAssets)
            Debug.Log("Deleted Asset: " + str);

        for (int i = 0; i < movedAssets.Length; i++)
            Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
            
         */
    }
    /*
    private static void UpdateAI_Graph(string str)
    {
        if (str.StartsWith("Assets/AI_Graph/"))
        {
            var listObj = ScriptableObject.CreateInstance<AI_GraphList>();

            foreach (var f in System.IO.Directory.GetFiles("Assets/AI_Graph/"))
            {
                if (f.EndsWith(".asset") == false)
                {
                    continue;
                }
                var graph = AssetDatabase.LoadAssetAtPath<AI_Graph>(f);
                if (graph == null)
                {
                    continue;
                }
                listObj.list.Add(graph);

      

            }

          //  File.WriteAllText("Assets/AI_Graph/graphs.json", JsonUtility.ToJson(listObj, true));


            AssetDatabase.CreateAsset(listObj, "Assets/Resources/AI_GraphList.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("Update AI_GraphList ");
        }
    }
    */
    
}