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
 

        GameObject go=null;

        if (host.zenContext != null)
        {
            go= host.zenContext.gameObject;
        }
        else
        {
            if (GUILayout.Button("AutoFind"))
            {
                go = AutoZenContextFind(host.transform);
            }
        }

        go = EditorGUILayout.ObjectField("ZenContextObj",go, typeof(GameObject), true) as GameObject;


 
        if (go == null)
        {
            return;
        }

        var context= FindZenContext(go);
        if (host.zenContext != context) {
            host.zenContext = context;
            EditorUtility.SetDirty(target);
        }
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
            var zc= host.zenContext as IZenMonoContext;
            if (zc!=null&& zc.AddBinder(host))
            {
                EditorUtility.SetDirty(target);
            }

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



       var bPath = EditorGUILayout.TextField("BindingPath",host.bindingPath);

        if (bPath != host.bindingPath)
        {
            host.bindingPath = bPath;
            EditorUtility.SetDirty(target);
        }

        if (PathOptions.Any(l=>l== host.bindingPath)==false)
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

                    EditorUtility.SetDirty(target);
                }
            }
        }


        {
            var type = host.GetTypeFromContext(host.bindingPath);

            if (type == null)
            {
                return;
            }



           var mono = EditorGUILayout.ObjectField("TargetMono", host.targetMono, typeof(MonoBehaviour), true) as MonoBehaviour;

            if (host.targetMono != mono)
            {
                host.targetMono = mono;
                EditorUtility.SetDirty(target);
            }
      
            if (host.targetMono == null)
            {
                if (GUILayout.Button("AutoFind"))
                {
                    AutoFindTargetMono(host, type);
                    EditorUtility.SetDirty(target);
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
                EditorUtility.SetDirty(target);
            }


            var fType = host.targetMono.GetType().GetField(host.targetBindgPath);

            Type targetValType=null;
            if (fType == null)
            {
                if(type == typeof(bool)){
                   var b= EditorGUILayout.Toggle("SetActive", host.usingToSetActive);

                    if (b != host.usingToSetActive)
                    {
                        host.usingToSetActive = b;
                        EditorUtility.SetDirty(target);
                    }


                    if (host.usingToSetActive)
                    {
                        host.usingNotBool = EditorGUILayout.Toggle("usingNot", host.usingNotBool);
                        if (b != host.usingNotBool)
                        {
                            host.usingNotBool = b;
                            EditorUtility.SetDirty(target);
                        }
                        return;
                    }

                }

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



            var path = EditorGUILayout.TextField("TargetBindingPath", host.targetBindgPath);

            if (host.targetBindgPath != path)
            {
                host.targetBindgPath = path;
                EditorUtility.SetDirty(target);
            }

            if (targetValType != null && targetValType == typeof(string))
            {

                bool usingFormat=EditorGUILayout.Toggle("usingFormat",host.format != null);

                string newFormat;
                if(usingFormat && string.IsNullOrEmpty( host.format))
                {
                    host.format = "{0}";
                }

                if (usingFormat == false)
                {
                    newFormat = null;
                }
                else
                {
                    newFormat = EditorGUILayout.TextField("Format", host.format);
                }
                if(newFormat!= host.format)
                {
                    host.format = newFormat;

                    EditorUtility.SetDirty(target);
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

                    EditorUtility.SetDirty(target);
                }
                foreach (var v in candidates.Where(l => l.StartsWith(host.targetBindgPath)))
                {
                    if (GUILayout.Button(v, style))
                    {
                        host.targetBindgPath = v;

                        EditorUtility.SetDirty(target);
                    }
                }
            }
        }


    }

    private void AutoFindTargetMono(ZenBinder host, Type type)
    {

        {
            var t= host.GetComponent<TMPro.TextMeshProUGUI>();
            if (t != null)
            {
                host.targetMono = t;
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
