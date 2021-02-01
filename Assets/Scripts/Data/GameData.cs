using System.Collections;
using System.Collections.Generic;
using UJ.Data;
using UnityEngine;

public class GameData :ScriptableObject
{
    public List<PlayerLevel> PlayerLevel;
    public List<CharacterInfo> CharacterInfo;
    public List<CharacterLevel> CharacterLevel;
    public List<CharacterSkill> CharacterSkill;
    public List<CharacterSkillLevel> CharacterSkillLevel;
    public List<Weapon> Weapon;
    public List<WeaponUpgrade> weaponUpgrade;
    public List<Prop> prop;


    public static GameData Load()
    {
        Debug.Log("GameData Initialized");
        var data = LoadFromFile();

        Str.SetStrs(Resources.Load<StrList>("strs").strs);
        EtcStr.SetEtcStrs(Resources.Load<EtcStrList>("EtcStrs").strs);


        return data;


    }

    private static GameData LoadFromFile()
    {

        var data = Resources.Load<GameData>("data");

        data.Init();

        return data;

    }

    private void Init()
    {
        prop.Clear();


        foreach (var p in prop)
        {
            p.Init();
        }

    }


}
