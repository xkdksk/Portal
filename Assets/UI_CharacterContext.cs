using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Linq;
using UJ.Data;
using System;

public class UI_CharacterEvent{
    public enum EventType
    {
        ClickDetail,        
    }
    public EventType eventType;
    public int charCode;
}

public class UI_CharacterContext : ZenMonoContext<UI_CharacterContext>
{

    [Inject]
    GameData gameData;

    [Inject]
    UserData userData;

    public CharacterInfo CharacterInfo;
    public CharacterInstanceInfo instanceInfo;
    public int code;
    public float lvRate;
    public string lvStateStr;


    public bool isForDetail;
    public int atkPower;
    public float hearts;
    public int floatHeartWidth;


    public CharacterLevel lvInfo;

    PropertiesData properties = new PropertiesData();
    public bool hasCharacter;

    public void SetCode(int code)
    {
        this.code = code;
        if (IsInitialized)
        {
            OnActive();
        }

    }

    protected override void OnActive()
    {
        CharacterInfo = gameData.CharacterInfo.Find(l => l.code == code);

        if (CharacterInfo == null)
        {
            return;
        }
        instanceInfo = userData.characters.FirstOrDefault(l => l.code == code);
       
     

        if (instanceInfo != null)
        {
            hasCharacter = true;
            lvInfo = gameData.CharacterLevel.Find(l => l.level == instanceInfo.level);
            if (lvInfo == null)
            {
                lvStateStr = EtcStr.FindStr("CharacterLevelTextWhenMaxLevel");
                lvRate = 1;
            }
            else
            {
                lvStateStr = instanceInfo.exp + "/" + lvInfo.expCost;
                lvRate = instanceInfo.exp / (float)lvInfo.expCost;
            }

        }
        else
        {
            hasCharacter = true;
            lvInfo = gameData.CharacterLevel.Find(l => l.level == 1);
            hasCharacter = false;
        }


        if (isForDetail)
        {
            SettingForDetail();
        }


        UpdateVals();
    }

    private void SettingForDetail()
    {
        properties.Clear();
        properties.ApplyPropsFromCode(gameData, 1, CharacterInfo.prop);


        if (instanceInfo != null)
        {
            var item1=userData.CharacterItem.Find(l => l.code == instanceInfo.eqMainCode);
            var item2 = userData.CharacterItem.Find(l => l.code == instanceInfo.eqSubCode);

            if (item1 != null)
            {
                ApplyItem(item1,100);
            }

            if (item2 != null)
            {
                ApplyItem(item2, 1001);
            }
            properties.SetParam(1, "CharacterLevel", instanceInfo.level);
        }
        else
        {
            properties.SetParam(1, "CharacterLevel", 1);
        }

        atkPower = (int)properties.Get("AttackPower");
        hearts = (float)properties.Get("hearts");

    }

    private void ApplyItem(CharacterItem item1,int kindCode)
    {
        var w = gameData.Weapon.Find(l => l.code == item1.code);
        properties.SetParam(kindCode, "WeaponLevel", item1.level,false);
        properties.ApplyPropsFromCode(gameData, kindCode, w.prop);
    }

    public void ClickDetail()
    {
        EventBus.Publish(new UI_CharacterEvent()
        {
            eventType= UI_CharacterEvent.EventType.ClickDetail,
            charCode= code
        });
    }
    
}
