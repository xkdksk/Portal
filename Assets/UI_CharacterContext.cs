using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Linq;
using UJ.Data;

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
            var lvInfo = gameData.CharacterLevel.Find(l => l.level == instanceInfo.level);
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
            hasCharacter = false;
        }

        UpdateVals();
    }

    
}
