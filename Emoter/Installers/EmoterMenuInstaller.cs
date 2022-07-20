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
    private GameObject? _emoterImageSourceContainer;
    private GameObject? _physicalEmoterSourceContainer;

    public override void InstallBindings()
    {
        Container.BindInterfacesTo<EmoteScreenDaemon>().AsSingle();
        Container.BindInterfacesTo<EmotePacketReceiver>().AsSingle();
        Container.Bind<QuickEmoteViewController>().FromNewComponentAsViewController().AsSingle();

        Container.BindMemoryPool<PhysicalEmote, PhysicalEmote.Pool>().WithInitialSize(0).FromMethod(FactorizePhysicalEmote);
        Container.BindMemoryPool<EmoterImage, EmoterImage.Pool>().WithId(EmoterImage.Pool.Id).WithInitialSize(0).FromMethod(FactorizeEmoterImage);

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

    private EmoterImage FactorizeEmoterImage(DiContainer _)
    {
        GameObject image = new("Emoter UI Image");
        image.SetActive(false);

        if (_emoterImageSourceContainer == null)
            _emoterImageSourceContainer = new GameObject("Emoter UI Image Pool Container");

        image.transform.SetParent(_emoterImageSourceContainer.transform);
        return image.AddComponent<EmoterImage>();
    }

    private PhysicalEmote FactorizePhysicalEmote(DiContainer _)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.GetComponent<Collider>().enabled = false;
        cube.name = "Physical Emote";
        cube.SetActive(false);

        if (_physicalEmoterSourceContainer == null)
            _physicalEmoterSourceContainer = new GameObject("Physical Emote Pool Container");

        cube.transform.SetParent(_physicalEmoterSourceContainer.transform);
        return cube.AddComponent<PhysicalEmote>();
    }
}