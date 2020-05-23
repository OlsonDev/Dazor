using System;
using System.Collections.Generic;
using System.Linq;

namespace Dazor.Extensions {
  public static class EnumerableExtensiosn {
		public static IReadOnlyList<T> ToReadOnlyListFast<T>(this IEnumerable<T> enumerable)
			=> enumerable as IReadOnlyList<T> ?? enumerable?.ToList() ?? (IReadOnlyList<T>)Array.Empty<T>();

		public static string ToFriendlyList(this IEnumerable<string> strings, int maxItems = int.MaxValue - 1) {
			if (maxItems <= 0 || maxItems == int.MaxValue) throw new ArgumentOutOfRangeException(nameof(maxItems), $"Should be above zero but less than `{nameof(Int32)}.{nameof(int.MaxValue)}`");
			var list = strings.ToReadOnlyListFast();
			var count = list.Count;
			if (count == 0) return "";
			if (count == 1) return list[0];
			if (count == 2) return $"{list[0]} and {list[1]}";
			if (count > maxItems + 1) return $"{string.Join(", ", list.Take(maxItems))}, and {count - maxItems} others";
			return $"{string.Join(", ", list.Take(count - 1))}, and {list.Last()}";
		}

		public static string ToFriendlyList(this IEnumerable<string> strings, string singularPrefix, string pluralPrefix, int maxItems = int.MaxValue - 1) {
			var list = strings.ToReadOnlyListFast();
			var count = list.Count;
			if (count == 0) return $"no {pluralPrefix}";
			if (count == 1) return $"the {singularPrefix} {list[0]}";
			return $"the {pluralPrefix} {list.ToFriendlyList(maxItems)}";
		}
  }
}