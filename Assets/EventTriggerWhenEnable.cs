using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTriggerWhenEnable : MonoBehaviour
{
    public Animator anim;

    public string triggerName;

    private void OnEnable()
    {

        anim.SetTrigger(triggerName);
    }
}
