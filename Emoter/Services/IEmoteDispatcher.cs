using Emoter.Models;

namespace Emoter.Services;

internal interface IEmoteDispatcher
{
    void Dispatch(Emote emote);
}