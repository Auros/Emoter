using Newtonsoft.Json;
using System;

namespace Emoter.Models;
internal class Emote
{
    public static Emote Empty = new() { Name = "Empty", Source = "Emoter.<null>" };

    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("source")]
    public string Source { get; set; } = string.Empty;

    public override string ToString()
    {
        return Name;
    }
}