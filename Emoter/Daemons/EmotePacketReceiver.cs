using Emoter.Models;
using Emoter.Services;
using IPA.Utilities;
using MultiplayerCore.Networking;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Emoter.Daemons;

internal class EmotePacketReceiver : IInitializable, IDisposable
{
    private readonly SiraLog _siraLog;
    private readonly IEmoteService _emoteService;
    private readonly MpPacketSerializer _mpPacketSerializer;
    private readonly IEmoteDisplayService _emoteDisplayService;
    private readonly Dictionary<string, Transform> _playerHeadMap = new();
    private readonly Dictionary<string, MultiplayerLobbyAvatarController> _playerAvatarMap;

    private readonly GameObject _mocker = new("Head Mocker");
    private readonly FieldAccessor<MultiplayerLobbyAvatarManager, Dictionary<string, MultiplayerLobbyAvatarController>>.Accessor PlayerAvatarMapAccessor = FieldAccessor<MultiplayerLobbyAvatarManager, Dictionary<string, MultiplayerLobbyAvatarController>>.GetAccessor("_playerIdToAvatarMap");
    private readonly FieldAccessor<AvatarPoseController, Transform>.Accessor HeadAccessor = FieldAccessor<AvatarPoseController, Transform>.GetAccessor("_headTransform");

    public EmotePacketReceiver(SiraLog siraLog, IEmoteService emoteService, MpPacketSerializer mpPacketSerializer, IEmoteDisplayService emoteDisplayService, MultiplayerLobbyAvatarManager multiplayerLobbyAvatarManager)
    {
        _siraLog = siraLog;
        _emoteService = emoteService;
        _mpPacketSerializer = mpPacketSerializer;
        _emoteDisplayService = emoteDisplayService;
        _playerAvatarMap = PlayerAvatarMapAccessor(ref multiplayerLobbyAvatarManager);
    }

    public void Initialize()
    {
        _mpPacketSerializer.RegisterCallback<EmoteDispatchPacket>(ReceivedEmoteDispatch);
    }

    private async void ReceivedEmoteDispatch(EmoteDispatchPacket packet, IConnectedPlayer connectedPlayer)
    {
        _siraLog.Info($"Received packet with emote id '{packet.EmoteId}' with a duration of '{packet.Time}' and a distance of {packet.Distance}");
        var head = GetPlayerHead(connectedPlayer);

        var emote = await _emoteService.GetEmoteAsync(packet.EmoteId);
        if (emote is null)
        {
            _siraLog.Error($"Could not find emote with the Id '{packet.EmoteId}'");
            emote = Emote.Empty;
        }
        _emoteDisplayService.Spawn(emote, new EmoteDisplayOptions(packet.Time, packet.Distance, head.position, head.forward));
    }

    public void Dispose()
    {
        _mpPacketSerializer.UnregisterCallback<EmoteDispatchPacket>();
    }

    private Transform GetPlayerHead(IConnectedPlayer connectedPlayer)
    {
        if (_playerHeadMap.TryGetValue(connectedPlayer.userId, out var headTransform) && headTransform != null)
            return headTransform;

        var controller = _playerAvatarMap[connectedPlayer.userId];
        var poseController = controller.GetComponentInChildren<AvatarPoseController>();
        headTransform = HeadAccessor(ref poseController);

        _playerHeadMap[connectedPlayer.userId] = headTransform;
        return headTransform;
    }
}