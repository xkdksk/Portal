using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UI_CharacterCell : TableViewCell<int>
{
    public UI_CharacterContext uI_CharacterContext;

    public override void UpdateContent(int code)
    {

        uI_CharacterContext.SetCode( code);

    }
}
