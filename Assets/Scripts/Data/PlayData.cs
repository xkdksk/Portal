using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UJ.Data;
using Zenject;

public class PlayData
{
    [Inject]
    GameData gameData;

    public PropertiesData properties = new PropertiesData();

    public void Init()
    {
        properties.ApplyPassive(gameData.prop.Where(l => l.code == 1), 0);
    }
}