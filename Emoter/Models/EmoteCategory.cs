using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Emoter.Models;
internal class EmoteCategory
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("emotes")]
    public List<Emote> Emotes { get; set; } = new();
    
    public override string ToString()
    {
        return Name;
    }
}