using Emoter.Models;
using SiraUtil.Tools.FPFC;
using UnityEngine;

namespace Emoter.Services;

internal class LocalEmoteDispatcher : IEmoteDispatcher
{
    private readonly MainCamera _mainCamera;
    private readonly IFPFCSettings _fpfcSettings;
    private readonly IEmoteDisplayService _emoteDisplayService;

    public LocalEmoteDispatcher(MainCamera mainCamera, IFPFCSettings fpfcSettings, IEmoteDisplayService emoteDisplayService)
    {
        _mainCamera = mainCamera;
        _fpfcSettings = fpfcSettings;
        _emoteDisplayService = emoteDisplayService;
    }

    public void Dispatch(Emote emote)
    {
        _emoteDisplayService.Spawn(emote, _mainCamera.camera.transform.position, _mainCamera.camera.transform.forward);
    }
}