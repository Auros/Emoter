using Emoter.UI.Main.Controllers;
using HMUI;
using SiraUtil.Logging;
using SiraUtil.Services;
using SiraUtil.Tools.FPFC;
using System;
using Tweening;
using UnityEngine;
using UnityEngine.UI;
using VRUIControls;
using Zenject;
using Screen = HMUI.Screen;

namespace Emoter.Daemons;

internal class EmoteScreenDaemon : IInitializable, ITickable, IDisposable
{
    private readonly SiraLog _siraLog;
    private readonly DiContainer _container;
    private readonly IFPFCSettings _fpfcSettings;
    private readonly IVRPlatformHelper _vrPlatformHelper;
    private readonly TimeTweeningManager _timeTweeningManager;
    private readonly IMenuControllerAccessor _menuControllerAccessor;
    private readonly QuickEmoteViewController _quickEmoteViewController;
    private readonly MultiplayerLobbyController _multiplayerLobbyController;

    private GameObject _emoterContainer = null!;
    private CanvasGroup _emoterCanvasGroup = null!;
    private Screen _emoterScreen = null!;

    private bool _multiLobbyState;
    private float _holdValue;
    private bool _didToggle;

    public EmoteScreenDaemon(SiraLog siraLog, DiContainer container, IFPFCSettings fpfcSettings, IVRPlatformHelper vrPlatformHelper, TimeTweeningManager timeTweeningManager, IMenuControllerAccessor menuControllerAccessor, QuickEmoteViewController quickEmoteViewController, MultiplayerLobbyController multiplayerLobbyController)
    {
        _siraLog = siraLog;
        _container = container;
        _fpfcSettings = fpfcSettings;
        _vrPlatformHelper = vrPlatformHelper;
        _timeTweeningManager = timeTweeningManager;
        _menuControllerAccessor = menuControllerAccessor;
        _quickEmoteViewController = quickEmoteViewController;
        _multiplayerLobbyController = multiplayerLobbyController;
    }

    public void Initialize()
    {
        _emoterContainer = new("Emote Screen Container");
        _emoterContainer.transform.localScale = new(.0075f, .0075f, .0075f);

        GameObject emoterScreen = new("Emoter Screen");
        emoterScreen.transform.SetParent(_emoterContainer.transform);
        emoterScreen.transform.localScale = Vector3.one;

        var canvas = emoterScreen.AddComponent<Canvas>();
        _container.InstantiateComponent<VRGraphicRaycaster>(emoterScreen);
        var screen = emoterScreen.AddComponent<Screen>();
        var curvedCanvasSettings = emoterScreen.AddComponent<CurvedCanvasSettings>();
        var canvasScaler = emoterScreen.AddComponent<CanvasScaler>();
        var rectTransform = emoterScreen.GetComponent<RectTransform>();
        _emoterCanvasGroup = emoterScreen.AddComponent<CanvasGroup>();
        _emoterScreen = screen;

        curvedCanvasSettings.SetRadius(80f);

        canvasScaler.defaultSpriteDPI = 96;
        canvasScaler.dynamicPixelsPerUnit = 3.44f;
        canvasScaler.referencePixelsPerUnit = 10f;

        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord2;
        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Tangent;
        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Normal;

        //screen.SetRootViewController(_quickEmoteViewController, ViewController.AnimationType.None);

        rectTransform.sizeDelta = new Vector2(48f, 48f);

        emoterScreen.layer = 5;

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 5000;

        _emoterContainer.SetActive(false);
    }

    public void Tick()
    {
        if (!_multiplayerLobbyController.lobbyActivated && _multiLobbyState)
        {
            _multiLobbyState = false;
            if (_emoterContainer.activeSelf)
            {
                _emoterContainer.SetActive(false);
                _emoterContainer.transform.SetParent(null);
            }

            return;
        }
        _multiLobbyState = _multiplayerLobbyController.lobbyActivated;
        if (!_multiLobbyState)
            return;

        if (IsHoldingTrigger())
            _holdValue += Time.deltaTime;
        else
        {
            _holdValue = 0f;
            _didToggle = false;
        }

        if (!_didToggle && _holdValue > 1f)
        {
            _didToggle = true;
            ToggleUI();
        }

        if (_didToggle)
            return;
    }

    public void Dispose()
    {

    }

    private void ToggleUI()
    {
        _vrPlatformHelper.TriggerHapticPulse(_menuControllerAccessor.LeftController.node, 0.2f, 1f, default);
        var initialValue = _emoterContainer.activeSelf;
        if (!initialValue)
        {
            // Enabling the container
            _emoterCanvasGroup.alpha = 0f;
            _emoterContainer.SetActive(true);
            _timeTweeningManager.AddTween(new FloatTween(0f, 1f, o => _emoterCanvasGroup.alpha = o, 0.5f, EaseType.OutCirc), _emoterCanvasGroup);

            SetTransforms();
        }
        else
        {
            // Disabling the container
            var tween = _timeTweeningManager.AddTween(new FloatTween(1f, 0f, o => _emoterCanvasGroup.alpha = o, 0.5f, EaseType.InOutSine), _emoterCanvasGroup);
            tween.onCompleted += () =>
            {
                _emoterContainer.SetActive(false);
                _emoterContainer.transform.SetParent(null);
            };
        }
    }

    private bool IsHoldingTrigger()
    {
        return _fpfcSettings.Enabled ? Input.GetKey(KeyCode.L) : _menuControllerAccessor.LeftController.triggerValue > 0.9f;
    }

    private void SetTransforms()
    {
        if (!_quickEmoteViewController.isInViewControllerHierarchy)
            _emoterScreen.SetRootViewController(_quickEmoteViewController, ViewController.AnimationType.None);

        if (_fpfcSettings.Enabled)
        {
            _emoterContainer.transform.SetParent(null);
            _emoterContainer.transform.localPosition = new Vector3(0f, 1.2f, 1.5f);
            _emoterContainer.transform.localScale = new(.02f, .02f, .02f);
            _emoterContainer.transform.localRotation = Quaternion.identity;
        }
        else
        {
            _emoterContainer.transform.SetParent(_menuControllerAccessor.LeftController.transform);
            _emoterContainer.transform.localPosition = new Vector3(0.2f, 0.2f, 0.1f);
            _emoterContainer.transform.localScale = new(.0075f, .0075f, .0075f);
            _emoterContainer.transform.localRotation = Quaternion.identity;
        }
    }
}