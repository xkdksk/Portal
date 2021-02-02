using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;


[CustomEditor(typeof(ZenBinder))]
public class ZenBinderInspector : Editor
{

    ZenBinder host = null;



    protected List<string> GetSetableFields(Type type, System.Func<Type, bool> typeFilter)
    {
        List<string> fieldPath = new List<string>();

        foreach (var f in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance ))
        {
            if (typeFilter(f.FieldType))
            {
                fieldPath.Add(f.Name);
            }

        }

        foreach (var f in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance ))
        {
            if (f.CanWrite == false)
            {
                continue;
            }

            if (typeFilter(f.PropertyType))
            {
                fieldPath.Add(f.Name);
            }

        }

        return fieldPath;
    }


    protected List<string> GetFieldOptions(Type type, System.Func<Type, bool> typeFilter)
    {
        List<string> fieldPath = new List<string>();
        
        foreach (var f in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
        {
            if (typeFilter(f.FieldType))
            {
                fieldPath.Add(f.Name);
            }

            FillFieldPath(fieldPath, f.Name, f.FieldType, typeFilter, 0);
        }

        foreach (var f in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
        {
            if (typeFilter(f.PropertyType))
            {
                fieldPath.Add(f.Name);
            }

            FillFieldPath(fieldPath, f.Name, f.PropertyType, typeFilter, 0);
        }

        return fieldPath;
    }

    private void FillFieldPath(List<string> fieldPath, string prefix, Type fieldType, Func<Type, bool> typeFilter, int depth)
    {
        if (depth > 1)
        {
            return;
        }

        if (fieldType.Assembly.FullName.Contains("UnityEngine"))
        {
            return;
        }

        if (fieldType.Assembly.FullName.Contains("mscorlib,"))
        {
            return;
        }

        foreach (var f in fieldType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {

            if (f.FieldType.IsSubclassOf(typeof(MonoBehaviour)))
            {
                continue;
            }


            if (typeFilter(f.FieldType))
            {
                fieldPath.Add(prefix + "." + f.Name);
            }
            if (f.FieldType.Assembly.FullName.Contains("UnityEngine"))
            {
                continue;
            }
            FillFieldPath(fieldPath, prefix + "." + f.Name, f.FieldType, typeFilter, depth + 1);

        }

        foreach (var f in fieldType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {

            if (typeFilter(f.PropertyType))
            {
                fieldPath.Add(prefix + "." + f.Name);
            }
            if (f.PropertyType.Assembly.FullName.Contains("UnityEngine"))
            {
                continue;
            }
            FillFieldPath(fieldPath, prefix + "." + f.Name, f.PropertyType, typeFilter, depth + 1);
        }

    }


    void OnEnable()
    {
        //Character 컴포넌트를 얻어오기
        host = (ZenBinder)target;
    }

    string[] PathOptions = null;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorUtility.SetDirty(target);

        GameObject go=null;

        if (host.zenContext != null)
        {
            go= host.zenContext.gameObject;
        }
        else
        {
            if (GUILayout.Button("AutoFind"))
            {
                go = AutoZenContextFind(host.transform.parent);
            }
        }

        go = EditorGUILayout.ObjectField("ZenContextObj",go, typeof(GameObject), true) as GameObject;


 
        if (go == null)
        {
            return;
        }

        host.zenContext = FindZenContext(go);

        GUIStyle style = new GUIStyle(EditorStyles.textField);


        if ( host.zenContext == null)
        {
            style.normal.textColor = Color.red;
            EditorGUILayout.LabelField("Not Valid Context", style);
            return;
        }
        else
        {

            style.normal.textColor = new Color(0, 0.5f, 0); ;
            host.zenContext.GetType().GetMethod("AddBinder").Invoke(host.zenContext, new object[] { host });
            host.zenContext.GetType().GetMethod("ValidateBinders").Invoke(host.zenContext,null);

            EditorGUILayout.LabelField("Type " + host.zenContext.GetType().Name, style);
        }



        int selected = 0;

        if (PathOptions == null || PathOptions.Length == 0)
        {
            PathOptions = GetFieldOptions(host.zenContext.GetType(), (t) =>
            {
                return true;
            }).OrderBy(l => l.Count(c=>c=='.') ).ToArray();
        }

        selected = Array.IndexOf(PathOptions, host.bindingPath);



        host.bindingPath = EditorGUILayout.TextField("BindingPath",host.bindingPath);

        if(PathOptions.Any(l=>l== host.bindingPath)==false)
        {
            style.normal.textColor = Color.red;
            EditorGUILayout.LabelField("NotValid Path", style);
            style.normal.textColor = new Color(0, 0.5f, 0);

            if (host.bindingPath == null)
            {
                host.bindingPath = "";
            }
            foreach (var v in PathOptions.Where(l => l.StartsWith(host.bindingPath)))
            {
                if(GUILayout.Button(v, style))
                {
                    host.bindingPath = v;
                }
            }
        }


        {
            var type = host.GetTypeFromContext(host.bindingPath);

            if (type == null)
            {
                return;
            }

            host.targetMono = EditorGUILayout.ObjectField("TargetMono", host.targetMono, typeof(MonoBehaviour), true) as MonoBehaviour;

            if (host.targetMono == null)
            {
                if (GUILayout.Button("AutoFind"))
                {
                    AutoFindTargetMono(host, type);
                }

                return;
            }

            var candidates=GetSetableFields(host.targetMono.GetType(), candiType =>
            {
                return candiType == type || candiType== typeof(string);
            });

            if (host.targetBindgPath == null)
            {
                host.targetBindgPath="";
            }


            var fType = host.targetMono.GetType().GetField(host.targetBindgPath);

            Type targetValType=null;
            if (fType == null)
            {
                var pType = host.targetMono.GetType().GetProperty(host.targetBindgPath);
                if (pType != null)
                {
                    targetValType = pType.PropertyType;
                }
            }
            else
            {
                targetValType = fType.FieldType;
            }



            host.targetBindgPath = EditorGUILayout.TextField("TargetBindingPath", host.targetBindgPath);

            if (targetValType != null && targetValType == typeof(string))
            {

                bool usingFormat=EditorGUILayout.Toggle("usingFormat",host.format != null);

                if(usingFormat && string.IsNullOrEmpty( host.format))
                {
                    host.format = "{0}";
                }

                if (usingFormat == false)
                {
                    host.format = null;
                }
                else
                {
                    host.format = EditorGUILayout.TextField("Format", host.format);
                }
            }


            if (candidates.Any(l => l == host.targetBindgPath) == false)
            {
                style.normal.textColor = Color.red;
                EditorGUILayout.LabelField("NotValid Path", style);
                style.normal.textColor = new Color(0, 0.5f, 0);
                if (host.targetBindgPath == null)
                {
                    host.targetBindgPath = "";
                }
                foreach (var v in candidates.Where(l => l.StartsWith(host.targetBindgPath)))
                {
                    if (GUILayout.Button(v, style))
                    {
                        host.targetBindgPath = v;
                    }
                }
            }
        }


    }

    private void AutoFindTargetMono(ZenBinder host, Type type)
    {
        if(type== typeof(string))
        {
            host.targetMono = host.GetComponent<UnityEngine.UI.Text>();
            host.targetBindgPath = "text";
            if (host.targetMono == null)
            {
                host.targetMono = host.GetComponent<TMPro.TextMeshProUGUI>();
                host.targetBindgPath = "text";
            }

        }
        if (type == typeof(Sprite))
        {
            host.targetMono = host.GetComponent<UnityEngine.UI.Image>();
            host.targetBindgPath = "sprite";
     

        }

    }

    MonoBehaviour FindZenContext(GameObject go) {
        foreach (var c in go.GetComponents(typeof(Component)))
        {
            var type = c.GetType();


            if (type.BaseType.IsGenericType == false || type.BaseType.GetGenericTypeDefinition() != typeof(ZenMonoContext<>))

            {
                continue;
            }


            return c as MonoBehaviour;

        }
        return null;
    }

    private GameObject AutoZenContextFind(Transform iter)
    {
        while (iter != null){

            if (FindZenContext(iter.gameObject) != null)
            {
                return iter.gameObject;
            }
            iter = iter.parent;
        } 


        return null;

    }
}
