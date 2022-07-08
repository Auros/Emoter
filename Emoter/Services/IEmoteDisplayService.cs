using Emoter.Models;
using UnityEngine;

namespace Emoter.Services;

internal interface IEmoteDisplayService
{
    void Spawn(Emote emote, Vector3 position);
}