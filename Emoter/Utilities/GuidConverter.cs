using IPA.Config.Data;
using IPA.Config.Stores;
using System;

namespace Emoter.Utilities;

internal class GuidConverter : ValueConverter<Guid>
{
    public override Guid FromValue(Value? value, object parent)
    {
        return value is Text t
            ? new Guid(t.Value)
            : throw new ArgumentException("Value cannot be parsed into a Guid", nameof(value));
    }

    public override Value? ToValue(Guid obj, object parent)
    {
        return Value.Text(obj.ToString());
    }
}