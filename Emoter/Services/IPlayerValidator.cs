using Emoter.Models;
using System.Threading.Tasks;

namespace Emoter.Services;

internal interface IPlayerValidator
{
    Task<bool> ValidateAsync(Emote emote, string platformId);
}