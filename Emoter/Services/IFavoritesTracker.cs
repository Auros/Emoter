using Emoter.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emoter.Services;

internal interface IFavoritesTracker
{ 
    event Action? FavoritesChanged; 
    Task AddAsync(Emote emote);
    Task RemoveAsync(Emote emote);
    Task<IReadOnlyList<Emote>> GetFavoritesAsync();
}