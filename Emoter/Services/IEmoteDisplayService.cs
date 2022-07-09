using Emoter.Models;

namespace Emoter.Services;

internal interface IEmoteDisplayService
{
    void Spawn(Emote emote, EmoteDisplayOptions options);
}