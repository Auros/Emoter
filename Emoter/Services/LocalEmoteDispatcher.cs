using Emoter.Models;
using SiraUtil.Tools.FPFC;
using UnityEngine;

namespace Emoter.Services;

internal class LocalEmoteDispatcher : IEmoteDispatcher
{
    private readonly IFPFCSettings _fpfcSettings;
    private readonly IEmoteDisplayService _emoteDisplayService;

    public LocalEmoteDispatcher(IFPFCSettings fpfcSettings, IEmoteDisplayService emoteDisplayService)
    {
        _fpfcSettings = fpfcSettings;
        _emoteDisplayService = emoteDisplayService;
    }

    public void Dispatch(Emote emote)
    {
        _emoteDisplayService.Spawn(emote, new Vector3(0f, 1.2f, 1.5f));
    }
}