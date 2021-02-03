using System;
using System.Collections;
using System.Collections.Generic;
using UJ.Data;
using UnityEngine;

public class UI_LobbyEventHandler : MonoBehaviour
{
    UJ.Data.SubscribeObj subsc = new UJ.Data.SubscribeObj();

    public UI_CharacterContext ui_charDetailObj;


    private void OnEnable()
    {
        subsc.AddUnsub(EventBus.Subscribe<UI_CharacterEvent>(CharacterEventHandler));
    }

    private void OnDisable()
    {
        subsc.UnsubAndClear();
    }

    private void CharacterEventHandler(UI_CharacterEvent obj)
    {
        ui_charDetailObj.SetCode(obj.charCode);
        ui_charDetailObj.gameObject.SetActive(true);
    }
}
