using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UI_CurrencyTxt : ZenMonoBase<UI_CurrencyTxt>
{
    [Inject]
    UserData userData;
    TextMeshProUGUI text;

    public CurrencyType Currency;
    int preVal;

    public enum CurrencyType
    {
        Gold,Gem

    }

    protected override void Init()
    {
        text = GetComponent<TMPro.TextMeshProUGUI>();

    }

    protected override void OnActive()
    {


        switch (Currency)
        {
            case CurrencyType.Gold:
                preVal = userData.gold;         
                break;
            case CurrencyType.Gem:
                preVal = userData.gem;
                break;
        }

        text.text = preVal.ToString();

    }

    private void Update()
    {
        if (IsInitialized == false)
        {
            return;
        }

        int val=0;
        switch (Currency)
        {
            case CurrencyType.Gold:
                val = userData.gold;
                break;
            case CurrencyType.Gem:
                val = userData.gem;
                break;
        }

        if(preVal != val)
        {
            preVal = (int)Mathf.Lerp(preVal, val, Time.deltaTime * 3);
            text.text = preVal.ToString();
        }
        



    }


}
