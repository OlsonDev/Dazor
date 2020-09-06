using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dazor.Cli.Opts;
using Dazor.Extensions;
using Xunit;

namespace Dazor.Test {
  public class OptsTests {
    [Theory]
    [InlineData(typeof(CleanDataOpts))]
    [InlineData(typeof(CleanSchemaOpts))]
    [InlineData(typeof(DowngradeOpts))]
    [InlineData(typeof(InitOpts))]
    [InlineData(typeof(MigrateOpts))]
    [InlineData(typeof(RerunOpts))]
    [InlineData(typeof(UndoOpts))]
    [InlineData(typeof(UpgradeOpts))]
    public void OptsAreUnique(Type optsType) {
      var flags = BindingFlags.Public | BindingFlags.Static;
      Assert.True(optsType.IsAbstract && optsType.IsSealed, $"All `Dazor.Opts` classes should be `static`; {optsType.GetFriendlyName()} is not.");
      var propertyNames = optsType.GetProperties(flags).Select(p => p.Name).ToList();
      Assert.True(!propertyNames.Any(), $"All `Dazor.Opts` classes shouldn't have properties; {optsType.GetFriendlyName()} has {propertyNames.ToFriendlyList("property", "properties")}.");
      var fields = optsType.GetFields(flags);
      var opts = new HashSet<string>();
      foreach (var field in fields) {
        Assert.True(field.IsInitOnly, $"All fields in `Dazor.Opts` classes should be `readonly`; {optsType.GetFriendlyName()}.{field.Name} is not.");
        var value = field.GetValue(null);
        Assert.True(value is not null, $"All fields in `Dazor.Opts` classes shouldn't be `null`; {optsType.GetFriendlyName()}.{field.Name} is.");
        Assert.True(value is string[], $"All fields in `Dazor.Opts` classes should be of type {typeof(string[]).GetFriendlyName()}; {optsType.GetFriendlyName()}.{field.Name} is not.");
        var values = (string[])value!;
        Assert.True(values.Any(), $"All fields in `Dazor.Opts` classes should have values; {optsType.GetFriendlyName()}.{field.Name} is empty.");
        foreach (var opt in values) {
          Assert.True(opts.Add(opt), $"All opts in `Dazor.Opts` classes should be unique. Duplicate `{opt}` found in {optsType.GetFriendlyName()}.{field.Name}");
        }
      }
    }
  }
}