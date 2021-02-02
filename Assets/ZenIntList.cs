using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ZenIntList :UI_IntList
{
    DiContainer _DiContainer;
    DiContainer DiContainer
    {
        get
        {
            if (_DiContainer == null)
            {
                var sc = GameObject.Find("SceneContext");
                var context = sc.GetComponent<SceneContext>();
                _DiContainer=context.Container;
            }

            return _DiContainer;

        }
    }

    protected override void AfterInstantiate(GameObject obj)
    {

        DiContainer.InjectGameObject(obj);
    }
}
