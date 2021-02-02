using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Linq;

public class UI_CharacterList : ZenMonoBase<UI_CharacterList>
{

    [Inject]
    GameData gameData;

    [Inject]
    UserData userData;

    public UI_IntList list;
    public UI_CharacterContext selectedCharacter;

    protected override void OnActive()
    {

        list.SetData( gameData.CharacterInfo.Where(l => l.code != userData.selectedCharacterCode).Select(l => l.code).ToList());
        selectedCharacter.SetCode(userData.selectedCharacterCode);
    }

}
