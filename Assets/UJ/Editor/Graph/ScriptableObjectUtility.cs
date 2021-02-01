using UnityEngine;
using UnityEditor;
using System.IO;

//http://wiki.unity3d.com/index.php?title=CreateScriptableObjectAsset&_ga=2.89703199.568651445.1582603510-1108169413.1558660130
public static class ScriptableObjectUtility
{
	/// <summary>
	//	This makes it easy to create, name and place unique new ScriptableObject asset files.
	/// </summary>
	public static void CreateAsset<T>(string basePath) where T : ScriptableObject
	{
		T asset = ScriptableObject.CreateInstance<T>();
		/*
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (path == "")
		{
			path = "Assets";
		}
		else if (Path.GetExtension(path) != "")
		{
			path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
		}
		*/
		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath("Assets/" + basePath + "/New " + typeof(T).ToString() + ".asset");

		AssetDatabase.CreateAsset(asset, assetPathAndName);

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = asset;
	}
}