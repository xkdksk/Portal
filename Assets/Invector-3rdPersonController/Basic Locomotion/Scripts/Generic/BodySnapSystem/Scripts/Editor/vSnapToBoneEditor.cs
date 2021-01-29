using Invector;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(vSnapToBody))]
public class vSnapToBoneEditor : Editor
{
    vSnapToBody snapToBone;
    int index = 0;
    GUIStyle fontLabelStyle = new GUIStyle();
    public GUISkin skin;

    private void OnEnable()
    {
        skin = Resources.Load("vSkin") as GUISkin;
        snapToBone = target as vSnapToBody;
    }

    private void OnSceneGUI()
    {
        if (Application.isPlaying)
        {
            return;
        }

        Handles.color = Color.white;
        if (snapToBone)
        {
            snapToBone.bodySnap = snapToBone.transform.root.GetComponentInChildren<vBodySnappingControl>();
            var e = Event.current.type;

            if (snapToBone.bodySnap != null)
            {
                if (snapToBone.bodySnap && snapToBone.bodySnap.boneSnappingList != null)
                {
                    var boneList = snapToBone.bodySnap.boneSnappingList;
                    for (int i = 0; i < boneList.Count; i++)
                    {
                        var bone = boneList[i];
                        if (bone.bone)
                        {
                            var sameBone = bone.bone == snapToBone.boneToSnap;
                            Handles.color = sameBone ? Color.green : Color.white * 0.8f;
                            if (sameBone)
                            {
                                Handles.SphereHandleCap(0, bone.bone.transform.position, Quaternion.identity, 0.05f, EventType.Repaint);
                            }
                            else if (Handles.Button(bone.bone.transform.position, Quaternion.identity, sameBone ? 0.05f : 0.025f, 0.05f, Handles.SphereHandleCap))
                            {
                                Undo.RecordObject(snapToBone, "BoneSelected");
                                index = i + 1;
                                snapToBone.boneName = bone.name;
                                snapToBone.boneToSnap = bone.bone;

                                serializedObject.ApplyModifiedProperties();
                                Repaint();
                            }

                            switch (e)
                            {
                                case EventType.Repaint:
                                    {
                                        float dist = Vector2.Distance(Event.current.mousePosition, HandleUtility.WorldToGUIPoint(bone.bone.position));
                                        if (dist < 15f)
                                        {
                                            fontLabelStyle.fontSize = 15;
                                            fontLabelStyle.normal.textColor = Color.green;
                                            fontLabelStyle.fontStyle = FontStyle.Bold;
                                            fontLabelStyle.alignment = TextAnchor.MiddleCenter;
                                            GUI.color = Color.white;
                                            Handles.Label(bone.bone.position, bone.name, fontLabelStyle);
                                            break;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }

            if (index == 0 && snapToBone.boneToSnap != null)
            {
                Handles.color = Color.green;
                Handles.SphereHandleCap(0, snapToBone.boneToSnap.position, Quaternion.identity, 0.05f, EventType.Repaint);

                switch (e)
                {
                    case EventType.Repaint:
                        {
                            float dist = Vector2.Distance(Event.current.mousePosition, HandleUtility.WorldToGUIPoint(snapToBone.boneToSnap.position));
                            if (dist < 15f)
                            {
                                fontLabelStyle.fontSize = 15;
                                fontLabelStyle.normal.textColor = Color.green;
                                fontLabelStyle.fontStyle = FontStyle.Bold;
                                fontLabelStyle.alignment = TextAnchor.MiddleCenter;
                                GUI.color = Color.white;
                                Handles.Label(snapToBone.boneToSnap.position, snapToBone.boneToSnap.name, fontLabelStyle);
                                break;
                            }
                        }
                        break;
                }
            }
        }
        Handles.color = Color.white;
    }

    public override void OnInspectorGUI()
    {
        var oldSkin = GUI.skin;
        if (GUI.skin != skin)
        {
            GUI.skin = skin;
        }

        serializedObject.Update();
        GUI.enabled = false;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        GUI.enabled = true;
        if (snapToBone)
        {
            GUI.color = snapToBone.bodySnap ? Color.white : Color.red;
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Target Body Snap Control", EditorStyles.largeLabel);
            GUI.enabled = false;

            snapToBone.bodySnap = (vBodySnappingControl)EditorGUILayout.ObjectField(snapToBone.bodySnap, typeof(vBodySnappingControl), true);

            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUI.color = Color.white;
            if (snapToBone.bodySnap == null)
            {
                EditorGUILayout.HelpBox("Without vBodySnapping Control component in the Character you will need to assign the bone manually\nPlease, create a GameObject with vBodySnapping Control component to easily find the bone target", MessageType.Info);
                GUILayout.BeginHorizontal("box");

                snapToBone.boneToSnap = (Transform)EditorGUILayout.ObjectField(snapToBone.boneToSnap, typeof(Transform), true);
                GUI.enabled = false;
                EditorGUI.BeginChangeCheck();
                GUI.color = Color.red;
                EditorGUILayout.Popup(0, new string[] { snapToBone.boneName });
                GUI.color = Color.white;
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
            if (snapToBone.bodySnap)
            {
                if (snapToBone.bodySnap && snapToBone.bodySnap.boneSnappingList != null)
                {
                    try
                    {
                        string[] bones = new string[snapToBone.bodySnap.boneSnappingList.Count + 1];
                        bones[0] = vSnapToBody.manuallyAssignBone;
                        for (int i = 1; i < bones.Length; i++)
                        {
                            bones[i] = snapToBone.bodySnap.boneSnappingList[i - 1].name;
                        }

                        if (index > 0 && string.IsNullOrEmpty(snapToBone.boneName) && snapToBone.boneToSnap != null)
                        {
                            var _bodyParty = snapToBone.bodySnap.boneSnappingList.Find(b => b.bone.Equals(snapToBone.boneToSnap));
                            if (_bodyParty != null)
                            {
                                index = snapToBone.bodySnap.boneSnappingList.IndexOf(_bodyParty) + 1;
                                snapToBone.boneName = snapToBone.bodySnap.boneSnappingList[index - 1].name;
                            }
                        }
                        var bodyParty = snapToBone.bodySnap.boneSnappingList.Find(b => b.name.Equals(snapToBone.boneName));
                        if (bodyParty != null)
                        {
                            index = snapToBone.bodySnap.boneSnappingList.IndexOf(bodyParty) + 1;
                            snapToBone.boneToSnap = snapToBone.bodySnap.boneSnappingList[index - 1].bone;
                        }

                        GUILayout.BeginHorizontal("box");
                        GUI.enabled = index == 0;
                        snapToBone.boneToSnap = (Transform)EditorGUILayout.ObjectField(snapToBone.boneToSnap, typeof(Transform), true);
                        GUI.enabled = true;
                        EditorGUI.BeginChangeCheck();

                        index = EditorGUILayout.Popup(index, bones);

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(snapToBone, "BoneName");

                            if (index > 0)
                            {
                                snapToBone.boneToSnap = snapToBone.bodySnap.boneSnappingList[index - 1].bone;
                            }
                            snapToBone.boneName = bones[index];
                            serializedObject.ApplyModifiedProperties();
                        }

                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal("box");
                        GUI.enabled = snapToBone.boneToSnap;

                        if (snapToBone.boneToSnap && GUILayout.Button("SnapToPosition", EditorStyles.miniButtonLeft))
                        {
                            Undo.RecordObject(snapToBone.transform, "BoneTransformAlignment");
                            snapToBone.transform.position = snapToBone.boneToSnap.transform.position;
                        }

                        if (snapToBone.boneToSnap && GUILayout.Button("SnapToRotation", EditorStyles.miniButtonLeft))
                        {
                            Undo.RecordObject(snapToBone.transform, "BoneTransformAlignment");
                            snapToBone.transform.rotation = snapToBone.boneToSnap.transform.rotation;
                        }

                        GUILayout.EndHorizontal();
                        GUI.enabled = true;
                    }
                    catch { }
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
        if (GUI.skin != oldSkin)
        {
            GUI.skin = oldSkin;
        }
    }
}