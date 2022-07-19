using Emoter.Models;
using System.Threading.Tasks;

namespace Emoter.Services.Offline;

internal class AlwaysValidPlayerValidator : IPlayerValidator
{
    public Task<bool> ValidateAsync(Emote emote, string platformId)
    {
        return Task.FromResult(true);
    }
}