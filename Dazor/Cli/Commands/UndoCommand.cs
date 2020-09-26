using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dazor.Cli.Options;
using Dazor.Config;
using Dazor.Extensions;
using Microsoft.Data.SqlClient;

namespace Dazor.Cli.Commands {
  internal class UndoCommand : VersioningCommandBase {
    private readonly UndoOptions _options;

    public UndoCommand(UndoOptions options, BoundConfig config, string[] commandLineArgs)
      : base(config, commandLineArgs)
      => _options = options;

    protected override async Task<Result> ExecuteAsync(int executionId, SqlConnection connection) {
      var validationContext = await ValidateAsync();
      if (validationContext.ShouldReportFailure) return Result.Failure;
      var maxDatabasedMigrationVersion = validationContext.MaxDatabasedMigrationVersion;
      var toVersionRaw = ParseToVersionFromWhat(maxDatabasedMigrationVersion);
      if (UndoToVersionIsImpossible(toVersionRaw, validationContext)) {
        return Result.Failure;
      }

      await UndoAsync(toVersionRaw!.Value);

      return Result.Success;
    }

    private Task UndoAsync(int toVersion) {
      throw new System.NotImplementedException();
    }

    private bool UndoToVersionIsImpossible(short? toVersionRaw,  ValidationContext validationContext) {
      if (toVersionRaw is null) {
        LogWhatNotParsed("undo", _options.What);
        return true;
      }
      var maxDatabasedMigrationVersion = validationContext.MaxDatabasedMigrationVersion;
      var toVersion = toVersionRaw.Value;
      if (toVersion > maxDatabasedMigrationVersion) {
        LogOnVersionBeforeRequested(toVersion, maxDatabasedMigrationVersion, "undo");
        return true;
      } else if (toVersion == maxDatabasedMigrationVersion) {
        LogAlreadyOnVersionRequested(maxDatabasedMigrationVersion, "undo");
        return true;
      } else if (toVersion <= 0) {
        LogCantDowngradeToZeroOrLess(toVersion, "undo");
      }

      var undoMigrations = validationContext.MigrationFiles
        .Where(mf => mf.MigrationType == MigrationType.UndoVersion && mf.Version!.Value.IsInRangeInclusive((short)(toVersion + 1), maxDatabasedMigrationVersion))
        .OrderByDescending(mf => mf.Version!.Value);

      var canBeAtVersion = maxDatabasedMigrationVersion;
      foreach (var undoMigration in undoMigrations) {
        if (canBeAtVersion != undoMigration.Version!.Value) break;
        canBeAtVersion--;
      }
      if (canBeAtVersion != toVersion) {
        LogCanOnlyDowngradeTo(toVersion, canBeAtVersion, "undo");
        return true;
      }

      return false;
    }

    private short? ParseToVersionFromWhat(short maxDatabasedMigrationVersion) {
      // _options.What formats:
      // "" (empty string, same as "last")
      // "last"
      // "18+"
      // "to 17"
      if (string.IsNullOrWhiteSpace(_options.What)) return (short)(maxDatabasedMigrationVersion - 1);
      var match = Regex.Match(_options.What, @"^(?:(?:to (?<toDigits>-?\d+))|(?<last>last)|(?<digitsPlus>[1-9]|[1-9]\d+)\+)$", RegexOptions.Compiled);
      if (!match.Success) return null;
      var toDigits = match.Groups["toDigits"];
      if (toDigits.Success) return short.Parse(toDigits.Value);
      var last = match.Groups["last"];
      if (last.Success) return (short)(maxDatabasedMigrationVersion - 1);
      var digitsPlus = match.Groups["digitsPlus"];
      // No need to check `if (digitsPlus.Success)`.
      return (short)(short.Parse(digitsPlus.Value) - 1);
    }
  }
}