using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class ScriptGeneratorWindow :  EditorWindow
{
    static ScriptGeneratorWindow mainWindow;

    public struct CodeGenOption
    {
        public FieldInfo fieldInfo;
    }



    System.Type type = null;
    [MenuItem("UJ/ScriptGenerator/Show")]
    static void ShowGraphEditor_()
    {
        if (mainWindow == null)
        {
            mainWindow = CreateInstance<ScriptGeneratorWindow>();
            mainWindow.titleContent = new GUIContent("Data Importer");

        }

        foreach (Assembly a in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            if (a.FullName.StartsWith("Unity"))
                continue;
            if (a.FullName.StartsWith("mscorlib"))
            {
                continue;
            }
            if (a.FullName.StartsWith("System"))
            {
                continue;
            }
            if (a.FullName.StartsWith("Mono"))
            {
                continue;
            }
            if (a.FullName.Contains("VisualStudio"))
            {
                continue;
            }

            foreach (var t in a.GetTypes())
            {
                if (t.Name == "GameData")
                {
                    mainWindow.type = t;
                }
            }

        }


        mainWindow.Show();


       
    }


    int selectedFieldIdx;


    void OnGUI()
    {

        EditorGUILayout.BeginVertical();

    
        if (type == null)
        {
            Debug.LogError("GameData 타입이 없다.");
            return;
        }


        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);


        selectedFieldIdx = EditorGUILayout.Popup("Fields",selectedFieldIdx, fields.Select(l=>l.Name).ToArray());

        //detail ,cell

        if (GUILayout.Button("코드생성"))
        {

            makeCode(new CodeGenOption() {

                fieldInfo = fields[selectedFieldIdx]
            });
        }

        EditorGUILayout.EndVertical();
    }

    void CheckAndCreateDirectory(string path)
    {
        if (Directory.Exists(path) == false)
        {
            Directory.CreateDirectory(path);
        }

    }

    public void makeCode(CodeGenOption option)
    {
        CheckAndCreateDirectory("Assets/Scripts");
        CheckAndCreateDirectory("Assets/Scripts/Data");
        CheckAndCreateDirectory("Assets/Scripts/Data/Gen");
        CheckAndCreateDirectory("Assets/Scripts/UI");

        var so = ScriptGenratorObj.Instance;
        var name = option.fieldInfo.Name;
        var txt = so.containerTemplate.text;
      

        TryToGenCodes(name, txt, "Assets/Scripts/Data/Gen/" + name + "Container.cs");
        TryToGenCodes(name, so.instanceTemplate.text, "Assets/Scripts/Data/Gen/" + name + "Instance.cs");                
        TryToGenCodes(name, so.itemListTemplate.text, "Assets/Scripts/UI/UI_" + name + "List.cs");
        AssetDatabase.Refresh();

    }

    private static void TryToGenCodes(string name, string txt, string filePath)
    {
        if (File.Exists(filePath) == false)
        {
            var str = txt.Replace("{{0}}", name);
            using (var fs = File.CreateText(filePath))
            {
                fs.Write(str);
            }
            Debug.Log(filePath + "생성");
        }
        else
        {
            Debug.Log(filePath + "이미 있음");
        }
    }
}