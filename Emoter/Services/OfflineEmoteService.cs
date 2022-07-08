using Emoter.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emoter.Services;

internal class OfflineEmoteService : IEmoteService
{
    public Task<IReadOnlyList<EmoteCategory>> GetCategoriesAsync()
    {
        List<EmoteCategory> categories = new()
        {
            new EmoteCategory { Id = Plugin.FavoritesCategoryId, Name = "Favorites" }
        };
        categories.AddRange(OfflineEmoteStore.OfflineCategories);

        return Task.FromResult<IReadOnlyList<EmoteCategory>>(categories);
    }

    public Task<EmoteCategory?> GetCategoryAsync(Guid categoryId)
    {
        if (categoryId == Plugin.FavoritesCategoryId)
            _ = true;

        return Task.FromResult<EmoteCategory?>(null);
    }
}