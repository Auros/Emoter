using System;

namespace Emoter.Models;
internal class Emote
{
    public static Emote Empty = new() { Name = "Empty", Source = "Emoter.<null>" };

    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;

    public override string ToString()
    {
        return Name;
    }
}