using Emoter.Components;
using Emoter.Daemons;
using Emoter.Services;
using Emoter.Services.Other;
using Emoter.Services.Online;
using Emoter.UI.Main.Controllers;
using UnityEngine;
using Zenject;

namespace Emoter.Installers;

internal class EmoterMenuInstaller : Installer
{
    private GameObject? _sourceContainer;

    public override void InstallBindings()
    {
        Container.BindInterfacesTo<EmoteScreenDaemon>().AsSingle();
        Container.BindInterfacesTo<EmotePacketReceiver>().AsSingle();
        Container.Bind<QuickEmoteViewController>().FromNewComponentAsViewController().AsSingle();

        Container.BindMemoryPool<EmoterImage, EmoterImage.Pool>().WithId(EmoterImage.Pool.Id).WithInitialSize(0).FromMethod(FactoryMethod);


        bool useLocal = true;
        if (useLocal)
        {
            Container.Bind<IEmoteDispatcher>().To<OnlineEmoteDispatcher>().AsSingle();
            Container.Bind<IEmoteDisplayService>().To<BasicEmoteDisplayService>().AsSingle();
        }
        else
        {

        }
    }

    private EmoterImage FactoryMethod(DiContainer _)
    {
        GameObject image = new("Emoter UI Image");
        image.SetActive(false);

        if (_sourceContainer == null)
            _sourceContainer = new GameObject("Emoter UI Image Pool Container");

        image.transform.SetParent(_sourceContainer.transform);
        return image.AddComponent<EmoterImage>();
    }
}