using System;

namespace Emoter.Models;
internal class Emote
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;

    public override string ToString()
    {
        return Name;
    }
}