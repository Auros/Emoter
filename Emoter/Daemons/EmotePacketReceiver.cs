using Emoter.Models;
using Emoter.Services;
using IPA.Utilities;
using MultiplayerCore.Networking;
using MultiplayerCore.Players;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Emoter.Daemons;

internal class EmotePacketReceiver : IInitializable, IDisposable
{
    private readonly Config _config;
    private readonly SiraLog _siraLog;
    private readonly IEmoteService _emoteService;
    private readonly MpPlayerManager _mpPlayerManager;
    private readonly IPlayerValidator _playerValidator;
    private readonly MpPacketSerializer _mpPacketSerializer;
    private readonly IEmoteDisplayService _emoteDisplayService;
    private readonly Dictionary<string, Transform> _playerHeadMap = new();
    private readonly Dictionary<string, float> _lastEmotesReceived = new();
    private readonly Dictionary<string, MultiplayerLobbyAvatarController> _playerAvatarMap;
    private readonly FieldAccessor<AvatarPoseController, Transform>.Accessor HeadAccessor = FieldAccessor<AvatarPoseController, Transform>.GetAccessor("_headTransform");
    private readonly FieldAccessor<MultiplayerLobbyAvatarManager, Dictionary<string, MultiplayerLobbyAvatarController>>.Accessor PlayerAvatarMapAccessor = FieldAccessor<MultiplayerLobbyAvatarManager, Dictionary<string, MultiplayerLobbyAvatarController>>.GetAccessor("_playerIdToAvatarMap");

    public EmotePacketReceiver(Config config, SiraLog siraLog, IEmoteService emoteService, MpPlayerManager mpPlayerManager, IPlayerValidator playerValidator, MpPacketSerializer mpPacketSerializer, IEmoteDisplayService emoteDisplayService, MultiplayerLobbyAvatarManager multiplayerLobbyAvatarManager)
    {
        _config = config;
        _siraLog = siraLog;
        _emoteService = emoteService;
        _mpPlayerManager = mpPlayerManager;
        _playerValidator = playerValidator;
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
        _siraLog.Debug($"Received packet with emote id '{packet.EmoteId}' with a duration of '{packet.Time}' and a distance of {packet.Distance}");
        
        if (_config.MaximumEmoteRatePerPlayer != default)
        {
            if (_lastEmotesReceived.TryGetValue(connectedPlayer.userId, out var lastUsedTime))
                if (lastUsedTime + _config.MaximumEmoteRatePerPlayer > Time.time)
                    return;
            _lastEmotesReceived[connectedPlayer.userId] = Time.time;
        }

        var head = GetPlayerHead(connectedPlayer);

        var emote = await _emoteService.GetEmoteAsync(packet.EmoteId);
        if (emote is null)
        {
            _siraLog.Error($"Could not find emote with the Id '{packet.EmoteId}'");
            emote = Emote.Empty;
        }
        else if (_mpPlayerManager.TryGetPlayer(connectedPlayer.userId, out var player))
        {
            var platformId = player.PlatformId;
            if (!await _playerValidator.ValidateAsync(emote, platformId))            
                emote.Source = "Emoter.Resources.Errors.InvalidPermissions.png";
        }

        _emoteDisplayService.Spawn(emote, new EmoteDisplayOptions(packet.Time, packet.Distance, head.position + head.forward * 0.2f, head.forward * 0.2f)); // Move the emote spawnpoint 0.2m in front of the head
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