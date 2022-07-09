using Emoter.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emoter.Services;

internal interface IEmoteService
{
    Task<IReadOnlyList<EmoteCategory>> GetCategoriesAsync();
    Task<EmoteCategory?> GetCategoryAsync(Guid categoryId);
    Task<Emote?> GetEmoteAsync(Guid emoteId);
}