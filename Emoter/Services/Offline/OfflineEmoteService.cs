using Emoter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emoter.Services.Offline;

internal class OfflineEmoteService : IEmoteService, IFavoritesTracker
{
    private bool _loadedFavorites;
    private readonly Config _config;
    public event Action? FavoritesChanged;
    private readonly List<Emote> _favorites = new();

    private readonly EmoteCategory _favoritesCategory;

    public OfflineEmoteService(Config config)
    {
        _config = config;
        _favoritesCategory = new()
        {
            Id = Plugin.FavoritesCategoryId,
            Emotes = _favorites,
            Name = "Favorites",
        };
    }

    public Task AddAsync(Emote emote)
    {
        if (_favorites.Contains(emote))
            return Task.CompletedTask;

        _favorites.Add(emote);
        FavoritesChanged?.Invoke();
        _config.Favorites.Add(emote.Id);
        _config.Changed();
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<EmoteCategory>> GetCategoriesAsync()
    {
        _ = await GetFavoritesAsync();
        List<EmoteCategory> categories = new()
        {
            _favoritesCategory
        };
        categories.AddRange(OfflineEmoteStore.OfflineCategories);
        return categories;
    }

    public Task<EmoteCategory?> GetCategoryAsync(Guid categoryId)
    {
        return Task.FromResult<EmoteCategory?>(categoryId == Plugin.FavoritesCategoryId ? _favoritesCategory : OfflineEmoteStore.OfflineCategories.FirstOrDefault(o => o.Id == categoryId));
    }

    public Task<Emote?> GetEmoteAsync(Guid emoteId)
    {
        var emotes = OfflineEmoteStore.OfflineCategories.SelectMany(o => o.Emotes);
        foreach (var emote in emotes)
            if (emote.Id == emoteId)
                return Task.FromResult<Emote?>(emote);
        return Task.FromResult<Emote?>(null);
    }

    public Task<IReadOnlyList<Emote>> GetFavoritesAsync()
    {
        if (!_loadedFavorites)
        {
            _loadedFavorites = true;
            var emotes = OfflineEmoteStore.OfflineCategories.SelectMany(o => o.Emotes);
            foreach (var emote in emotes)
            {
                if (!_config.Favorites.Contains(emote.Id))
                    continue;
                _favorites.Add(emote);
            }
        }
        return Task.FromResult<IReadOnlyList<Emote>>(_favorites);
    }

    public Task RemoveAsync(Emote emote)
    {
        if (!_favorites.Contains(emote))
            return Task.CompletedTask;

        _favorites.Remove(emote);
        FavoritesChanged?.Invoke();
        _config.Favorites.Remove(emote.Id);
        _config.Changed();
        return Task.CompletedTask;
    }
}