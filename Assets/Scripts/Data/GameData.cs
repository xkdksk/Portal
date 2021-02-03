using System.Collections;
using System.Collections.Generic;
using UJ.Data;
using UnityEngine;
using System.Linq;

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


        foreach (var p in prop)
        {
            p.Init();
        }

    }


}




public static class PropsExt
{
    public static void ApplyProps(this PropertiesData properties, GameData gameData, int paramKindCode, IEnumerable<Prop> props)
    {
        foreach (var p in props)
        {
            foreach (var bp in p.baseProp)
            {
                properties.ApplyPropsFromCode(gameData, paramKindCode, bp);
            }

            properties.ApplyPassive(p, paramKindCode);
        }

    }
    public static void ApplyPropsFromCode(this PropertiesData properties, GameData gameData, int paramKindCode, int propCode)
    {
        ApplyProps(properties, gameData, paramKindCode, gameData.prop.Where(l => l.code == propCode));

    }

    static PropertiesData tempProps = new PropertiesData();
    public static void ApplyToLayerPropsFromCode(this PropertiesData properties, GameData gameData, System.Action<PropertiesData> settingFunc, int layerCode, int propCode)
    {
        tempProps.Clear();
        tempProps.ApplyPropsFromCode(gameData, 1, propCode);

        settingFunc(tempProps);

        var props = tempProps.properties;


        var iter = props.GetEnumerator();


        while (iter.MoveNext())
        {
            properties.SetLayerValue(layerCode, iter.Current.Key, iter.Current.Value);
        }

        tempProps.Clear();
    }
}
