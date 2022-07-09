using Emoter.Daemons;
using Emoter.Services.Other;
using Emoter.Services.Offline;
using Emoter.Services.Sprites;
using Zenject;

namespace Emoter.Installers;

internal class EmoterCoreInstaller : Installer
{
    public override void InstallBindings()
    {
        bool useCachedService = true;

        Container.BindInterfacesTo<EmoterInputService>().AsSingle();
        Container.BindInterfacesTo<OfflineEmoteService>().AsSingle();
        Container.BindInterfacesTo<EmoterResourceService>().AsSingle();
        Container.BindInterfacesTo<EmoteErrorSpriteService>().AsSingle();
    
        if (useCachedService)
        {
            Container.Bind(typeof(AssemblySpriteSourceBuilder), typeof(ILateDisposable)).To<AssemblySpriteSourceBuilder>().AsSingle();
            Container.BindInterfacesAndSelfTo<CachedSpriteSourceBuilder>().AsSingle();
        }
        else
        {
            Container.BindInterfacesAndSelfTo<AssemblySpriteSourceBuilder>().AsSingle();
        }
    }
}