using SiraUtil.Tools.FPFC;
using UnityEngine;
using Zenject;

namespace Emoter.Services.Other;

internal interface IEmoterInputService
{
    bool IsHoldingDownAlternateAction { get; }
}

internal class EmoterInputService : IEmoterInputService, ILateTickable
{
    private readonly IFPFCSettings _fpfcSettings;
    private readonly VRControllersInputManager _vrControllersInputManager;

    private bool? _valueThisFrame;

    public EmoterInputService(IFPFCSettings fpfcSettings, VRControllersInputManager vrControllersInputManager)
    {
        _fpfcSettings = fpfcSettings;
        _vrControllersInputManager = vrControllersInputManager;
    }

    public bool IsHoldingDownAlternateAction
    {
        get
        {
            if (_valueThisFrame.HasValue)
                return _valueThisFrame.Value;

            _valueThisFrame = _fpfcSettings.Enabled ? Input.GetKey(KeyCode.N) : _vrControllersInputManager.MenuButton();
            return _valueThisFrame.Value;
        }
    }

    public void LateTick()
    {
        _valueThisFrame = null;
    }
}