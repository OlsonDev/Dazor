using System;
using System.Collections.Generic;

namespace Dazor.Extensions {
  internal static class TypeExtensions {
		private static readonly IDictionary<Type, string> _typeToFriendlyName = new Dictionary<Type, string> {
				{ typeof(string), "string" },
				{ typeof(object), "object" },
				{ typeof(bool), "bool" },
				{ typeof(byte), "byte" },
				{ typeof(char), "char" },
				{ typeof(decimal), "decimal" },
				{ typeof(double), "double" },
				{ typeof(short), "short" },
				{ typeof(int), "int" },
				{ typeof(long), "long" },
				{ typeof(sbyte), "sbyte" },
				{ typeof(float), "float" },
				{ typeof(ushort), "ushort" },
				{ typeof(uint), "uint" },
				{ typeof(ulong), "ulong" },
				{ typeof(void), "void" }
		};
    public static string GetFriendlyName(this Type type) {
      if (_typeToFriendlyName.TryGetValue(type, out var name)) return name;
      if (type.IsArray) return type.GetElementType().GetFriendlyName() + "[]";
      if (!type.IsGenericType) return type.Name;
      name = type.Name;
      var backtick = name.IndexOf('`');
      if (backtick > 0) name = name.Remove(backtick);
      name += "<";
      var typeParameters = type.GetGenericArguments();
      for (var i = 0; i < typeParameters.Length; i++) {
        var typeParamName = typeParameters[i].GetFriendlyName();
        name += i == 0 ? typeParamName : $", {typeParamName}";
      }
      name += ">";
      return name;
    }
  }
}