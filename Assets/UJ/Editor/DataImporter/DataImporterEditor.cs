using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace UJ.Data.Editor
{


    public class DataImporterEditor<T> : EditorWindow  where T : ScriptableObject
    {
        static DataImporterEditor<T> mainWindow;
        DataImporterSetting setting;

     
        public static void MakeStrXlsx()
        {
            var setting = DataImporterSetting.Instance;
            var gameData=setting.LoadXlsx<T>(setting.excelBasePath + setting.dataExcelName+ ".xlsx");

            MultiLanguage.FillMultiLangDic(gameData.GetType().Assembly.GetTypes());
            setting.LoadStrs();
            Reader.FillStrs(gameData,Str.Strs);
     
            writeInfo writeInfo = new writeInfo();
            writeInfo.ObjectToExcel(setting.excelBasePath + "strs.xlsx", Str.Strs);

        }


        public static  void GenerateCode()
        {
            var setting = DataImporterSetting.Instance;
            var basePath = setting.genCodePath;
            var gameData = setting.LoadXlsx<T>(setting.excelBasePath + setting.dataExcelName + ".xlsx");

            MultiLanguage.FillMultiLangDic(gameData.GetType().Assembly.GetTypes());
            setting.LoadStrs();
            Reader.FillStrs(gameData, Str.Strs);

            File.WriteAllText(basePath + "ClassPartials.cs", MultiLanguage.GenerateParticalClassCode());
            AssetDatabase.Refresh();

        }

        public static void ShowGraphEditor<EditorType>() where EditorType : DataImporterEditor<T>
        {
            if (mainWindow == null)
            {
                mainWindow = CreateInstance<EditorType>();
                mainWindow.titleContent =  new GUIContent("Data Importer");
                 
            }
            mainWindow.Show();
        }


        private void Awake()
        {
        }





        void OnGUI()
        {

            EditorGUILayout.BeginVertical();
            setting = DataImporterSetting.Instance;

            TextFieldWrapper("dataExcelName", "Excel File Name", setting);
            TextFieldWrapper("dataAssetOutputPath", "Data Asset OutputPath", setting);
            TextFieldWrapper("genCodePath", "Code Generation Path", setting);
            TextFieldWrapper("excelBasePath", "Excel base directory Path", setting);
            TextFieldWrapper("dataScriptBasePath", "Data script base Path", setting);

            TextFieldWrapper("jsonOutputPath", "Json Output Path", setting);
            EditorGUILayout.EndVertical();
        }

        void TextFieldWrapper(string fieldPropName, string labelName, ScriptableObject obj)
        {

            var type= obj.GetType();

            var field= type.GetField(fieldPropName);

            if (field.FieldType == typeof(string))
            {
                var val = field.GetValue(obj) as string;
                var fileName = EditorGUILayout.TextField(labelName, val, GUILayout.Width(400));

                if (fileName != setting.dataExcelName)
                {
                    field.SetValue(obj, fileName);
                    EditorUtility.SetDirty(setting);
                }
            }



        }



        void Update()
        {

        }


     



    }

}