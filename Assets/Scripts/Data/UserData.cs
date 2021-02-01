using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

public class CharacterInstanceInfo
{
    public int code;
    public int level;
    public int exp;
    public int eqMainCode,eqSubCode;
}

public class CharacterItem
{
    public int code;
    public int count;
    public int level;
}


public class UserData
{

    [Inject]
    GameData gameData;
    [Inject]
    PlayData playData;


    public int gold, gem;
    public int level=1, exp;

    public int selectedCharacterCode;


    public List<CharacterItem> CharacterItem;
    public List<CharacterInstanceInfo> characters;
    public HashSet<int> skinCodes;


    internal void Init()
    {
        gold = (int)playData.properties.Get("StartGold");
        gem = (int)playData.properties.Get("StartGem");

        var startChars = gameData.CharacterInfo.Where(l => l.price == 0);
        foreach(var c in startChars)
        {
            characters.Add(new CharacterInstanceInfo()
            {
                code = c.code,
                level = (int)playData.properties.Get("StartCharacterLevel")
            });
        }
         


    }

    public bool CanLvUp()
    {
        var pl=gameData.PlayerLevel.Find(l => l.level == level);
        if (pl == null)
        {
            return false;
        }

        return pl.exp >= exp;
    }


    public void LvUp()
    {
        var pl = gameData.PlayerLevel.Find(l => l.level == level);
        if (pl == null)
        {
            return;
        }

        exp -= pl.exp;
        level++;

    }


}