using Emoter.Models;
using Emoter.Services.Offline;
using Newtonsoft.Json;
using SiraUtil.Logging;
using SiraUtil.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emoter.Services.Online;

internal class CachedOnlineEmoteService : IEmoteService, IFavoritesTracker
{
    private bool _loadedFavorites;
    public event Action? FavoritesChanged;
    private List<EmoteCategory>? _cachedCategories;
    private readonly List<Emote> _cachedEmotes = new();

    private readonly Config _config;
    private readonly SiraLog _siraLog;
    private readonly IHttpService _httpService;
    private readonly IPlatformUserModel _platformUserModel;
    private readonly OfflineEmoteService _offlineEmoteService;

    public CachedOnlineEmoteService(Config config, SiraLog siraLog, IHttpService httpService, IPlatformUserModel platformUserModel, OfflineEmoteService offlineEmoteService)
    {
        _config = config;
        _siraLog = siraLog;
        _httpService = httpService;
        _platformUserModel = platformUserModel;
        _offlineEmoteService = offlineEmoteService;
    }

    public Task AddAsync(Emote emote)
    {
        if (_offlineEmoteService.FavoritesCategory.Emotes.Contains(emote))
            return Task.CompletedTask;

        _offlineEmoteService.FavoritesCategory.Emotes.Add(emote);
        FavoritesChanged?.Invoke();
        _config.Favorites.Add(emote.Id);
        _config.Changed();
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<EmoteCategory>> GetCategoriesAsync()
    {
        // If we've already loaded the categories, load them here.
        if (_cachedCategories is not null)
            return _cachedCategories;

        // Load the offline emotes, as we want to include them in the categories list.
        var offlines = await _offlineEmoteService.GetCategoriesAsync();

        // Hit the API for the verified categories.
        var verifiedCategoriesUrl = $"{_config.OnlineEmoteRepositoryAPI}/v1/categories/verified";
        var verifiedCategoriesResponse = await _httpService.GetAsync(verifiedCategoriesUrl);
        if (!verifiedCategoriesResponse.Successful)
        {
            _siraLog.Error($"Unable to get the verified categories list from '{verifiedCategoriesUrl}'");
            return offlines;
        }

        var userInfo = await _platformUserModel.GetUserInfo();

        bool addedRemaining = false;
        List<EmoteCategory> categories = new();

        if (offlines.Count > 0 && offlines[0].Id == Plugin.FavoritesCategoryId)
        {
            // Make sure the favorites category is first.
            categories.Add(offlines[0]);
        }
        else
        {
            addedRemaining = true;
            categories.AddRange(offlines);
        }

        // Hit the API to see if the current user has uploaded any emotes.
        var specificUserCategoryUrl = $"{_config.OnlineEmoteRepositoryAPI}/v1/categories/from-platform-user/{userInfo.platformUserId}";
        var specificUserCategoryResponse = await _httpService.GetAsync(specificUserCategoryUrl);
        if (specificUserCategoryResponse.Successful)
        {
            // Add the specific user's uploaded emotes.
            var category = JsonConvert.DeserializeObject<EmoteCategory>(await specificUserCategoryResponse.ReadAsStringAsync());
            categories.Add(category);
            _siraLog.Debug($"Acquired emote page specific to ({userInfo.userName} / {userInfo.platformUserId}) (Emotes: {category.Emotes.Count})");
        }

        // If we haven't added the remaining offlines yet, we do so here.
        if (!addedRemaining)
        {
            for (int i = 0; i < offlines.Count; i++)
            {
                var offlineCategory = offlines[i];
                if (!categories.Contains(offlineCategory))
                    categories.Add(offlineCategory);
            }
        }

        // Finally, we add the verified online emotes.
        var onlineCategories = JsonConvert.DeserializeObject<EmoteCategory[]>(await verifiedCategoriesResponse.ReadAsStringAsync());
        _siraLog.Debug($"Acquired online categories (Count: {onlineCategories.Length})");
        categories.AddRange(onlineCategories);
        _cachedCategories = categories;

        // For all the emotes we just gathered, add them to the cache.
        foreach (var emote in categories.SelectMany(c => c.Emotes))
            if (!_cachedEmotes.Any(e => e.Id == emote.Id))
                _cachedEmotes.Add(emote);

        _ = await GetFavoritesAsync();

        return categories;
    }

    public async Task<EmoteCategory?> GetCategoryAsync(Guid categoryId)
    {
        var categories = await GetCategoriesAsync();
        return categories.FirstOrDefault(c => c.Id == categoryId);
    }

    public async Task<Emote?> GetEmoteAsync(Guid emoteId)
    {
        // First we check the cache.
        var emote = _cachedEmotes.FirstOrDefault(e => e.Id == emoteId);
        if (emote is not null)
            return emote;

        // Just in case, check the offline service (usually will have the same result as above, but there are some edge cases).
        emote = await _offlineEmoteService.GetEmoteAsync(emoteId);
        if (emote is not null)
            return emote;

        // Check the API for the emote
        var emoteByIdUrl = $"{_config.OnlineEmoteRepositoryAPI}/v1/emotes/{emoteId}";
        var emoteByIdResponse = await _httpService.GetAsync(emoteByIdUrl);
        if (!emoteByIdResponse.Successful)
            return null; // API response failed? Return null.

        emote = JsonConvert.DeserializeObject<Emote>(await emoteByIdResponse.ReadAsStringAsync());
        if (emote is not null) // This REALLY shouldn't happen but let's check just in case
            if (!_cachedEmotes.Any(c => c.Id == emote.Id)) // If there are no emotes with the same ID...
                _cachedEmotes.Add(emote); // Add it to the cache

        return emote;
    }

    public async Task<IReadOnlyList<Emote>> GetFavoritesAsync()
    {
        if (!_loadedFavorites)
        {
            _loadedFavorites = true;
            _ = await _offlineEmoteService.GetFavoritesAsync();
            foreach (var emote in _cachedEmotes)
            {
                if (!_config.Favorites.Contains(emote.Id) || _offlineEmoteService.FavoritesCategory.Emotes.Contains(emote))
                    continue;
                _offlineEmoteService.FavoritesCategory.Emotes.Add(emote);
            }
        }
        return _offlineEmoteService.FavoritesCategory.Emotes;
    }

    public Task RemoveAsync(Emote emote)
    {
        if (!_offlineEmoteService.FavoritesCategory.Emotes.Contains(emote))
            return Task.CompletedTask;

        _offlineEmoteService.FavoritesCategory.Emotes.Remove(emote);
        FavoritesChanged?.Invoke();
        _config.Favorites.Remove(emote.Id);
        _config.Changed();
        return Task.CompletedTask;
    }
}