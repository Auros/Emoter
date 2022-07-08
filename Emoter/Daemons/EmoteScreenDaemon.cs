using Emoter.UI.Main.Controllers;
using HMUI;
using System;
using UnityEngine;
using UnityEngine.UI;
using VRUIControls;
using Zenject;
using Screen = HMUI.Screen;

namespace Emoter.Daemons;

internal class EmoteScreenDaemon : IInitializable, IDisposable
{
    private readonly DiContainer _container;
    private readonly QuickEmoteViewController _quickEmoteViewController;

    public EmoteScreenDaemon(DiContainer container, QuickEmoteViewController quickEmoteViewController)
    {
        _container = container;
        _quickEmoteViewController = quickEmoteViewController;
    }

    public void Initialize()
    {
        GameObject emoterContainer = new("Emote Screen Container");
        emoterContainer.transform.localScale = new(.02f, .02f, .02f);

        GameObject emoterScreen = new("Emoter Screen");
        emoterScreen.transform.SetParent(emoterContainer.transform);
        emoterScreen.transform.localScale = Vector3.one;

        var canvas = emoterScreen.AddComponent<Canvas>();
        var graphicsRaycaster = _container.InstantiateComponent<VRGraphicRaycaster>(emoterScreen);
        var screen = emoterScreen.AddComponent<Screen>();
        var curvedCanvasSettings = emoterScreen.AddComponent<CurvedCanvasSettings>();
        var canvasScaler = emoterScreen.AddComponent<CanvasScaler>();
        var rectTransform = emoterScreen.GetComponent<RectTransform>();

        canvasScaler.defaultSpriteDPI = 96;
        canvasScaler.dynamicPixelsPerUnit = 3.44f;
        canvasScaler.referencePixelsPerUnit = 10f;

        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord2;
        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Tangent;
        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Normal;

        screen.SetRootViewController(_quickEmoteViewController, ViewController.AnimationType.In);

        rectTransform.sizeDelta = new Vector2(48f, 48f);

        emoterScreen.layer = 5;

        emoterContainer.transform.localPosition += new Vector3(0f, 1.2f, 1f);
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 5000;
    }

    public void Dispose()
    {

    }
}