using Emoter.Services.Other;
using Emoter.Services.Offline;
using Emoter.Services.Sprites;
using Zenject;
using Emoter.Services.Online;

namespace Emoter.Installers;

internal class EmoterCoreInstaller : Installer
{
    public override void InstallBindings()
    {
        bool useCachedService = true;
        bool useEmoteCoreWebAPI = true;

        Container.BindInterfacesTo<EmoterInputService>().AsSingle();
        Container.BindInterfacesTo<EmoterResourceService>().AsSingle();
        Container.BindInterfacesTo<EmoteErrorSpriteService>().AsSingle();
    
        if (useEmoteCoreWebAPI)
        {
            Container.Bind<OfflineEmoteService>().AsSingle();
            Container.BindInterfacesTo<CachedOnlineEmoteService>().AsSingle();
        }
        else
        {
            Container.BindInterfacesTo<OfflineEmoteService>().AsSingle();
        }

        if (useCachedService)
        {
            Container.Bind(typeof(AssemblySpriteSourceBuilder), typeof(ILateDisposable)).To<AssemblySpriteSourceBuilder>().AsSingle();
            Container.Bind(typeof(InternetSpriteSourceBuilder), typeof(ILateDisposable)).To<InternetSpriteSourceBuilder>().AsSingle();
            Container.BindInterfacesAndSelfTo<CachedSpriteSourceBuilder>().AsSingle();
        }
        else
        {
            Container.BindInterfacesAndSelfTo<InternetSpriteSourceBuilder>().AsSingle();
            Container.BindInterfacesAndSelfTo<AssemblySpriteSourceBuilder>().AsSingle();
        }
    }
}