using UnityEditor;
using UnityEngine;

public class ScriptGenratorObj : ScriptableObject
{
    public TextAsset itemListTemplate, instanceTemplate,containerTemplate;


    public static ScriptGenratorObj _instance;
    public static ScriptGenratorObj Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = AssetDatabase.LoadAssetAtPath<ScriptGenratorObj>("Assets/UJ/CodeGen/ScriptGenratorObj.asset");

                if (_instance == null)
                {
                    string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath("Assets/UJ/CodeGen/ScriptGenratorObj.asset");
                    _instance = ScriptableObject.CreateInstance<ScriptGenratorObj>();
                    AssetDatabase.CreateAsset(_instance, assetPathAndName);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

            }

            return _instance;
        }
    }
}
