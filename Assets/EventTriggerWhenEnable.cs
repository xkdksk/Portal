using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventTriggerWhenEnable : MonoBehaviour
{
    public UnityEvent onEnableEvent;

    private void OnEnable()
    {
        onEnableEvent.Invoke();
    }
}
