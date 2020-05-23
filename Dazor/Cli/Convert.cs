using System;
using System.Collections.Generic;
using System.Linq;
using Dazor.Extensions;

namespace Dazor.Cli {
  internal static class Convert {
    internal static T To<T>(string value) {
      var type = typeof(T);
      if (type == typeof(string)) return (T)(object)value;
      if (type == typeof(bool)) return (T)(object)ConvertToBool(value);
      if (type == typeof(OffOrOn)) return (T)(object)ConvertToOffOrOn(value);
      throw new NotImplementedException($"{nameof(Convert)}.{nameof(To)}<{type.GetFriendlyName()}>(value) not implemented.");
    }

    internal static T To<T>(IEnumerable<string> values) {
      var type = typeof(T);
      if (type == typeof(string)) return (T)(object)string.Join(" ", values);
      if (type == typeof(bool)) return (T)(object)ConvertToBool(values.Single());
      if (type == typeof(OffOrOn)) return (T)(object)ConvertToOffOrOn(values.Single());
      throw new NotImplementedException($"{nameof(Convert)}.{nameof(To)}<{type.GetFriendlyName()}>(values) not implemented.");
    }

    internal static bool ConvertToBool(string value)
      => value.ToLowerInvariant() switch {
        "y" => true,
        "yes" => true,
        "1" => true,
        "true" => true,
        "n" => false,
        "no" => false,
        "0" => false,
        "false" => false,
        _ => throw new ParseException($"Please enter y/yes/1/true/n/no/0/false."),
      };

    internal static OffOrOn ConvertToOffOrOn(string value)
      => value.ToLowerInvariant() switch {
        "on" => OffOrOn.On,
        "off" => OffOrOn.On,
        _ => throw new ParseException($"Please enter on or off."),
      };
  }
}