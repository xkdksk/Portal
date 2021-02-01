using System.IO;
using UJ.Data.Editor;
using UnityEditor;

public class GameDataImporterEditor : DataImporterEditor<GameData>
{
    [MenuItem("UJ/DataImporter/GenerateStrXlsx")]
    public static void MakeStrXlsx_()
    {
        MakeStrXlsx();
    }


    [MenuItem("UJ/DataImporter/GenerateCode")]
    public static void GenerateCode_()
    {
        GenerateCode();
    }


    [MenuItem("UJ/DataImporter/Show")]
    static void ShowGraphEditor_()
    {
        ShowGraphEditor<GameDataImporterEditor>();
    }


    [MenuItem("UJ/MakeJSON")]
    static void MakeJson()
    {
        var setting = DataImporterSetting.Instance;
        var gameData = setting.LoadXlsx<GameData>(setting.excelBasePath + setting.dataExcelName + ".xlsx");

        var json = EditorJsonUtility.ToJson(gameData);

        json = json.Replace("{\"MonoBehaviour\":", "");
        json = json.Substring(0, json.Length - 1);
        File.WriteAllText(setting.jsonOutputPath + "data.json", json);
        EditorJsonUtility.ToJson(gameData);
    }
}
