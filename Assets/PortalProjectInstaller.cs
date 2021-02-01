using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PortalProjectInstaller : MonoInstaller<PortalProjectInstaller>
{
    public override void InstallBindings()
    {

        var gd=GameData.Load();
        Container.Bind<GameData>().FromInstance(gd);
        Container.Bind<PlayData>().AsSingle();
        Container.Bind<UserData>().AsSingle();

        var playData = Container.Resolve<PlayData>();
        playData.Init();
        var ud = Container.Resolve<UserData>();
        

    }
}
