using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Invector.vCharacterController
{
    [InitializeOnLoad]
    public class vCheckForProjectSettings
    {
#if UNITY_EDITOR

        public static int checkLayer;
        public static GUIStyle style;

        static vCheckForProjectSettings()
        {
            SceneView.onSceneGUIDelegate -= OnScene;
            SceneView.onSceneGUIDelegate += OnScene;
        }
        public static void OnScene(SceneView sceneView)
        {
            CheckLayer();
        }

        static bool IsAxisAvailable(string axisName)
        {
            try
            {
                Input.GetAxis(axisName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void CheckLayer()
        {
            Handles.BeginGUI();

            checkLayer = LayerMask.NameToLayer("Player");
            var rect = new Rect();

            if (checkLayer != 8 || !IsAxisAvailable("LeftAnalogHorizontal"))
            {
                if (style == null)
                {
                    style = new GUIStyle(EditorStyles.whiteLabel);
                    style.fontSize = 30;
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontStyle = FontStyle.Bold;
                    style.wordWrap = true;
                    style.clipping = TextClipping.Overflow;
                }

                var color = GUI.color;
                GUI.color = Color.black * 0.5f;

                string myString = "Missing ProjectSettings\nGo to the Menu Invector/Import ProjectSettings";
                GUILayout.Space(-20);
                GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                rect = GUILayoutUtility.GetLastRect();
                var height = style.CalcHeight(new GUIContent(myString), rect.width);
                rect.y += (rect.height / 2f) - height / 2f;
                rect.height = height;

                GUI.color = color;
                GUI.Label(rect, myString, style);

                var buttonRect = rect;
                buttonRect.y += rect.height + 20f;
                buttonRect.x = rect.width / 2f;
                buttonRect.x -= (rect.width * 0.2f) / 2f;
                buttonRect.width = rect.width * 0.2f;
                buttonRect.height = 25f;

                //if (GUI.Button(buttonRect, "Import ProjectSettings"))
                //{
                //    SceneView.onSceneGUIDelegate -= OnScene;
                //    AssetDatabase.ImportPackage(vInvectorWelcomeWindow._projectSettingsPath, true);                    
                //    EditorApplication.delayCall += ResetMethod;
                //}
            }

            Handles.EndGUI();
        }

        public static void ResetMethod()
        {
            SceneView.onSceneGUIDelegate += OnScene;
        }
#endif
    }

}