using Emoter.Utilities;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using SiraUtil.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Version = Hive.Versioning.Version;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace Emoter;

internal class Config
{
    [NonNullable, UseConverter(typeof(VersionConverter))]
    public virtual Version Version { get; set; } = null!;

    public virtual float Duration { get; set; } = 4f;
    public virtual float Distance { get; set; } = 2.5f;
    public virtual float MaximumEmoteRatePerPlayer { get; set; } = 0.5f;

    [UseConverter(typeof(ListConverter<Guid, GuidConverter>))]
    public virtual List<Guid> Favorites { get; set; } = new();

    public virtual void Changed() { }
}