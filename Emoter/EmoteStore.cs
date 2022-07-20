using Emoter.Models;
using System;
using System.Collections.Generic;

namespace Emoter;

/// <summary>
/// For offline emotes, there GUIDs can be random but need to be unique (for both the category ids and emote ids)
/// </summary>
internal class OfflineEmoteStore
{
    public static EmoteCategory Unicode = new()
    {
        Id = Guid.Parse("ad2eecb9-6701-4307-85a3-b59e0c106cfb"),
        Name = nameof(Unicode),
        Emotes = new()
        {
            BuildEmoteFromResource(Guid.Parse("08e19260-eeff-464b-9797-4ffdcac5a939"), nameof(Unicode), "Grinning"),
            BuildEmoteFromResource(Guid.Parse("b2d95242-db77-3325-b9f5-91e0d99bb3e3"), nameof(Unicode), "Split82"),
            BuildEmoteFromResource(Guid.Parse("484da46d-53b7-4f12-a4e8-9bcca20752be"), nameof(Unicode), "Frowning"),
            BuildEmoteFromResource(Guid.Parse("59bfbc23-ab00-41eb-9b8e-157826e5e465"), nameof(Unicode), "Joy"),
            BuildEmoteFromResource(Guid.Parse("b9bf5556-84da-4fd6-9b69-756b9f1fc68f"), nameof(Unicode), "Face With Hearts"),
            BuildEmoteFromResource(Guid.Parse("578adb42-4e5e-4de7-8a1b-cc7b040de142"), nameof(Unicode), "Heart Eyes"),
            BuildEmoteFromResource(Guid.Parse("6731e61a-8b77-426a-91cf-599f2496abec"), nameof(Unicode), "Neutral"),
            BuildEmoteFromResource(Guid.Parse("148fd268-115d-470e-bcb5-3317853bacfc"), nameof(Unicode), "Face With Raised Eyebrow"),
            BuildEmoteFromResource(Guid.Parse("d2295057-cd12-49b0-bcd6-41bee1634145"), nameof(Unicode), "Cowboy"),
            BuildEmoteFromResource(Guid.Parse("754a5d6f-531a-4c7f-9c35-1cbd026d70d4"), nameof(Unicode), "Angry"),
            BuildEmoteFromResource(Guid.Parse("5b703d4b-595e-4616-84da-0c703b23a9f0"), nameof(Unicode), "Skull"),
            BuildEmoteFromResource(Guid.Parse("e92ab98c-5a47-40aa-abe7-6e5716df3051"), nameof(Unicode), "Circle"),
            BuildEmoteFromResource(Guid.Parse("570ec09e-b8b1-4163-8f71-692001bbb036"), nameof(Unicode), "Cross"),
            BuildEmoteFromResource(Guid.Parse("cd02de89-3714-4c27-882c-7bfbb2e19c84"), nameof(Unicode), "Heart"),
            BuildEmoteFromResource(Guid.Parse("88a61a76-1ee1-4f18-9883-bfbd6dac87f2"), nameof(Unicode), "Thumbs Up"),
            BuildEmoteFromResource(Guid.Parse("d2651a77-5256-46a4-a246-280cb2013781"), nameof(Unicode), "Thumbs Down"),
        }
    };

    public static Emote BuildEmoteFromResource(Guid id, string category, string name, string extension = ".png")
    {
        return new Emote
        {
            Id = id,
            Name = name,
            Source = $"Emoter.Resources.Emotes.{category}.{name.Replace(" ", string.Empty)}{extension}"
        };
    }

    /// <summary>
    /// When adding a new offline category, do it here.
    /// </summary>
    public static IReadOnlyList<EmoteCategory> OfflineCategories = new List<EmoteCategory>()
    {
        Unicode
    };
}