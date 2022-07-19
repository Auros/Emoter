using Emoter.Models;
using Newtonsoft.Json;
using SiraUtil.Web;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Emoter.Services.Online;

internal class OnlineEmotePlayerValidator : IPlayerValidator
{
    private readonly Config _config;
    private readonly IHttpService _httpService;

    private readonly Dictionary<string, bool> _permissionCache = new();

    public OnlineEmotePlayerValidator(Config config, IHttpService httpService)
    {
        _config = config;
        _httpService = httpService;
    }

    public async Task<bool> ValidateAsync(Emote emote, string platformId)
    {
        var permissionUrl = _config.OnlineEmoteRepositoryAPI + $"/v1/emotes/{emote.Id}/perms/{platformId}";
        if (_permissionCache.TryGetValue(permissionUrl, out var permission))
            return permission;
        
        var permissionResponse = await _httpService.GetAsync(permissionUrl);
        if (!permissionResponse.Successful)
            return true;

        var validations = JsonConvert.DeserializeObject<Validation[]>(await permissionResponse.ReadAsStringAsync());
        foreach (var validation in validations)
            if (validation.Id == platformId && validation.Permission)
                permission = true;

        _permissionCache.Add(permissionUrl, permission);
        return permission;
    }

    private class Validation
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty("permission")]
        public bool Permission { get; set; }
    }
}