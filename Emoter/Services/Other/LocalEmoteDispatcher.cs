using Emoter.Models;
using SiraUtil.Tools.FPFC;
using UnityEngine;

namespace Emoter.Services.Other;

internal class LocalEmoteDispatcher : IEmoteDispatcher
{
    private readonly Config _config;
    private readonly MainCamera _mainCamera;
    private readonly IFPFCSettings _fpfcSettings;
    private readonly IEmoteDisplayService _emoteDisplayService;

    public LocalEmoteDispatcher(Config config, MainCamera mainCamera, IFPFCSettings fpfcSettings, IEmoteDisplayService emoteDisplayService)
    {
        _config = config;
        _mainCamera = mainCamera;
        _fpfcSettings = fpfcSettings;
        _emoteDisplayService = emoteDisplayService;
    }

    public void Dispatch(Emote emote)
    {
        _emoteDisplayService.Spawn(emote, new EmoteDisplayOptions(_config.Duration, _config.Distance, _fpfcSettings.Enabled ? new Vector3(0f, 1f, 1.75f) : _mainCamera.camera.transform.position, _mainCamera.camera.transform.forward));
    }
}