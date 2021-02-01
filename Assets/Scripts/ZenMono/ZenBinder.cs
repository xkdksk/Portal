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





    public void UpdateVal()
    {
        object bindingVal;
        TryGetValue(zenContext, bindingPath, out bindingVal);


        var fType = targetMono.GetType().GetField(targetBindgPath);
        if (fType == null)
        {
            var pType = targetMono.GetType().GetProperty(targetBindgPath);

            if (pType == null)
            {
                return;
            }
            pType.SetValue(targetMono, bindingVal);
        }
        else
        {
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
