using LiteNetLib.Utils;
using MultiplayerCore.Networking.Abstractions;
using System;

namespace Emoter.Models;

internal class EmoteDispatchPacket : MpPacket
{
    public Guid EmoteId { get; private set; }

    public float Time { get; private set; }
    public float Distance { get; private set; }

    public EmoteDispatchPacket()
    {

    }

    public EmoteDispatchPacket(Emote emote, float time, float distance)
    {
        Distance = distance;
        EmoteId = emote.Id;
        Time = time;
    }

    public override void Serialize(NetDataWriter writer)
    {
        writer.Put(EmoteId.ToString(), 36);
        writer.Put(Distance);
        writer.Put(Time);
    }

    public override void Deserialize(NetDataReader reader)
    {
        Guid.TryParse(reader.GetString(36), out Guid guid);
        EmoteId = guid;
        
        Distance = reader.GetFloat();
        Time = reader.GetFloat();

        if (Distance > 10 || Distance < 0)
            Distance = 2.5f;

        if (Time > 5 || Time < 0)
            Time = 4f;
    }
}