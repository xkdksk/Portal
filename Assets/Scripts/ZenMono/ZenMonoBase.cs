using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class ZenMonoBase<T> :MonoBehaviour where T:Component
{
    DiContainer DiContainer;
    protected bool IsInitialized;
    private void Start()
    {
        var sc= GameObject.Find("SceneContext");

        var context = sc.GetComponent<SceneContext>();


        var container = context.Container;

        container.InjectGameObjectForComponent<T>(this.gameObject);

        Init();
        IsInitialized = true;
        OnActive();
    }

    private void OnEnable()
    {
        if (IsInitialized == false)
        {
            return;
        }
        OnActive();
    }

    protected virtual void Init() { }

    protected virtual void OnActive() { }

}