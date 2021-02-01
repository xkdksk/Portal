using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UJ.Data.Editor
{
    public class DataImporterSetting :ScriptableObject
    {
        public string excelBasePath ="Assets/xlsx/";
        public string dataExcelName,dataAssetOutputPath="Assets/Resources/";
        public string genCodePath = "Assets/Scripts/Data/Gen/";
        public string dataScriptBasePath = "Asset/Scripts/Data/";
        public string jsonOutputPath = "";

        public static DataImporterSetting _instance;
        public static DataImporterSetting Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = AssetDatabase.LoadAssetAtPath<DataImporterSetting>("Assets/UJ/DataImportSetting.asset");

                    if (_instance == null)
                    {
                        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath("Assets/UJ/DataImportSetting.asset");
                        _instance = ScriptableObject.CreateInstance<DataImporterSetting>();
                        AssetDatabase.CreateAsset(_instance, assetPathAndName);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                    
                }

                return _instance;
            }
        }

        public void LoadStrs( )
        {
            using (var workbook = readInfo.GetWorkbookFromPath(excelBasePath+ "Strs.xlsx"))
            {


                List<Str> strs;
                if (workbook != null)
                {

                    strs = Reader.ReadFromExcel<Str>(workbook,"Str");
                }
                else
                {
                    strs = new List<Str>();
                }

                Str.SetStrs(strs);

                var strList = ScriptableObject.CreateInstance<StrList>();
                strList.strs = strs;
                EditorUtility.SetDirty(strList);
                Debug.Log("CreateAsset " + dataAssetOutputPath + "strs.asset");
                AssetDatabase.CreateAsset(strList, dataAssetOutputPath + "strs.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

            }
        }

        public void LoadEtcStrs()
        {
           
            Debug.Log("LoadEtcStrs");
            using (var workbook = readInfo.GetWorkbookFromPath(excelBasePath + "/EtcStrs.xlsx"))
            {
                List<EtcStr> strs;
                if (workbook != null)
                {

                    strs = Reader.ReadFromExcel<EtcStr>(workbook, "EtcStr");
                }
                else
                {
                    strs = new List<EtcStr>();
                }

                EtcStr.SetEtcStrs(strs);
 

                var strList = ScriptableObject.CreateInstance<EtcStrList>();
                strList.strs = strs;
                EditorUtility.SetDirty(strList);
                Debug.Log("CreateAsset " + dataAssetOutputPath + "EtcStrs.asset");
                AssetDatabase.CreateAsset(strList, dataAssetOutputPath + "EtcStrs.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

            }
        }

        public T LoadXlsx<T>(string path) where T: ScriptableObject
        {

            try
            {
                var obj = CreateInstance<T>();

                MultiLanguage.FillMultiLangDic(typeof(T).Assembly.GetTypes());
                Reader.LoadFromFile<T>(obj, path);
                return obj;
                //            Reader.FillStrs(obj);
            }
            catch(Exception exception)
            {
                
                var errMsg=
                       "문제 시트" + readInfo.lastWorkSheet + "\n" +
                        "문제 위치  " + readInfo.lastWorkRow + "행," + ((char)('a' - 1 + readInfo.lastWorkColumn)) + "열 \n"
                       + " " + exception.Message + " " + exception.StackTrace;
                Debug.LogError(errMsg);

            }

            return null;
      


      



        }
    }
}
