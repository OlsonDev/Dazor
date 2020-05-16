using System;
using System.Collections.Generic;
using System.Linq;

namespace Dazor.Extensions {
  public static class EnumerableExtensiosn {
		public static IReadOnlyList<T> ToReadOnlyListFast<T>(this IEnumerable<T> enumerable)
			=> enumerable as IReadOnlyList<T> ?? enumerable?.ToList() ?? (IReadOnlyList<T>)Array.Empty<T>();

		public static string ToFriendlyList(this IEnumerable<string> strings, int maxItems = int.MaxValue) {
			var list = strings.ToReadOnlyListFast();
			var count = list.Count;
			if (count == 0) return "";
			if (count == 1) return list[0];
			if (count == 2) return string.Concat(list[0], " and ", list[1]);
			if (count > maxItems + 1) return $"{string.Join(", ", list.Take(maxItems))}, and {count - maxItems} others";
			return $"{string.Join(", ", list.Take(count - 1))}, and {list.Last()}";
		}
  }
}