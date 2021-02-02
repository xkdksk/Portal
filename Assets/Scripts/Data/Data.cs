using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UJ.Data;
using UnityEngine;

[Serializable]
public class PlayerLevel
{
    public int code,level, exp, goldReward, gemReward;
}

[Serializable]
public partial class CharacterInfo
{
    public int code;
    [MultiLanguage]
    public string name, desc;
    public string avatarPrefab;
    public int prop, firstSkillCode, secondSkillCode;

    public int price;
}

[Serializable]
public class CharacterLevel
{
    public int code, level,expCost, goldCost;
}

[Serializable]
public partial class CharacterSkill
{
    public int code,prop;

    [MultiLanguage]
    public string name, desc;
}

[Serializable]
public class CharacterSkillLevel
{
    public int code, level,goldCost;
}


[Serializable]
public class Weapon
{
    public int code;
    public string iconPrefab;
    public string name, infoDesc, effectDesc;
    public int grade;

    public enum WeaponType
    {
        main,sub
    }

    public WeaponType type;
    public int prop;
}

[Serializable]
public class WeaponUpgrade
{
    public int code ,grade, level ,  startProb ,weightProb,  goldCost;
}


[Serializable]
public class Prop : HasFormula
{
    public int code;
    public List<int> baseProp;
    public List<string> value;

    internal void Init()
    {
        foreach (var formula in value)
        {
            try
            {
                AddExpressionByFormula(formula);
            }
            catch (Exception e)
            {
                Debug.LogError("Prop Error " + code + " " + formula);
            }
        }
    }
}