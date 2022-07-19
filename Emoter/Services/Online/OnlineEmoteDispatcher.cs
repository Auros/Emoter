using Emoter.Models;
using SiraUtil.Tools.FPFC;
using UnityEngine;

namespace Emoter.Services.Online;

internal class OnlineEmoteDispatcher : IEmoteDispatcher
{
    private readonly Config _config;
    private readonly MainCamera _mainCamera;
    private readonly IFPFCSettings _fpfcSettings;
    private readonly IEmoteDisplayService _emoteDisplayService;
    private readonly IMultiplayerSessionManager _multiplayerSessionManager;

    private float _lastDispatched;

    public OnlineEmoteDispatcher(Config config, MainCamera mainCamera, IFPFCSettings fpfcSettings, IEmoteDisplayService emoteDisplayService, IMultiplayerSessionManager multiplayerSessionManager)
    {
        _config = config;
        _mainCamera = mainCamera;
        _fpfcSettings = fpfcSettings;
        _emoteDisplayService = emoteDisplayService;
        _multiplayerSessionManager = multiplayerSessionManager;
    }

    public void Dispatch(Emote emote)
    {
        if (_config.MaximumEmoteRatePerPlayer != default)
        {
            if (_lastDispatched + _config.MaximumEmoteRatePerPlayer > Time.time)
                return;
            _lastDispatched = Time.time;
        }    

        _emoteDisplayService.Spawn(emote, new EmoteDisplayOptions(_config.Duration, _config.Distance, _fpfcSettings.Enabled ? new Vector3(0f, 1f, 1.75f) : _mainCamera.camera.transform.position, _mainCamera.camera.transform.forward));
        _multiplayerSessionManager.Send(new EmoteDispatchPacket(emote, _config.Duration, _config.Distance));
    }
}