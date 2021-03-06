using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZenBinder : MonoBehaviour
{
    [HideInInspector]
    public MonoBehaviour zenContext;


    [HideInInspector]
    public string bindingPath,targetBindgPath;

    [HideInInspector]
    public MonoBehaviour targetMono;

    [HideInInspector]
    public string format;

    [HideInInspector]
    public bool usingToSetActive;
    [HideInInspector]
    public bool usingNotBool;

    public void UpdateVal()
    {
        object bindingVal;
        TryGetValue(zenContext, bindingPath, out bindingVal);
        if (bindingVal == null)
        {
            return;
        }

        if (bindingVal.GetType()== typeof(bool))
        {
            if (usingToSetActive)
            {
                var b = (bool)bindingVal;
                if (usingNotBool)
                {
                    b = !b;
                }
                targetMono.gameObject.SetActive(b);
                return;
            }

        }

        var fType = targetMono.GetType().GetField(targetBindgPath);

        if (fType == null)
        {
            var pType = targetMono.GetType().GetProperty(targetBindgPath);

            if (pType == null)
            {
                return;
            }

            if(pType.PropertyType == typeof(string) && bindingVal.GetType()!= typeof(string))
            {
                if (string.IsNullOrWhiteSpace(format) == false)
                {
                    bindingVal = string.Format(format, bindingVal);
                }
                else
                {
                    bindingVal = bindingVal.ToString();
                }
            }

          


            pType.SetValue(targetMono, bindingVal);
        }
        else
        {
            if (fType.FieldType == typeof(string) && bindingVal.GetType() != typeof(string))
            {
                if (string.IsNullOrWhiteSpace(format) == false)
                {
                    bindingVal = string.Format(format, bindingVal);
                }
                else
                {
                    bindingVal = bindingVal.ToString();
                }

            }

            fType.SetValue(targetMono, bindingVal);
        }
    }

    public bool TryGetValue(object obj, string path, out object result)
    {

        var pathes = path.Split('.');
        Type type;
        if (obj == null)
        {
            result = null;
            return false;
        }

        foreach (var p in pathes)
        {

            if (obj == null)
            {
                result = null;
                return false;
            }
            type = obj.GetType();
            var field = type.GetField(p);

            if (field == null)
            {
                var property = type.GetProperty(p);
                obj = property.GetValue(obj);
            }
            else
            {
                obj = field.GetValue(obj);
            }
        }

        result = obj;

        return true;

    }

    public Type GetTypeFromContext(string path)
    {

        var pathes = path.Split('.');
        Type type;
        object obj = zenContext;
        if (obj == null)
        {
            return null;
        }

        type = obj.GetType();

        foreach (var p in pathes)
        {

            if (type == null)
            {
                return null;
            }
            var field = type.GetField(p);

            if (field == null)
            {
                var property = type.GetProperty(p);
                if (property == null)
                {
                    return null;
                }
                type = property.PropertyType;
            }
            else
            {
                type = field.FieldType;
            }
        }


        return type;

    }
}
