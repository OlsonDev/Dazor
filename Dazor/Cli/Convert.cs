using System;
using System.Collections.Generic;
using System.Linq;
using Dazor.Extensions;

namespace Dazor.Cli {
  internal static class Convert {
    internal static T To<T>(IEnumerable<string> values) {
      var type = typeof(T);
      if (type == typeof(string)) return (T)(object)string.Join(" ", values);
      if (type == typeof(bool)) return (T)(object)ConvertToBool(values);
      throw new NotImplementedException($"{nameof(Convert)}.{nameof(To)}<{type.GetFriendlyName()}>() not implemented.");
    }

    internal static bool ConvertToBool(IEnumerable<string> values)
      => values.Single().ToLowerInvariant() switch {
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
  }
}