using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dazor.Cli.Options;
using Dazor.Config;
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
      if (UndoToVersionIsImpossible(toVersionRaw, maxDatabasedMigrationVersion)) {
        return Result.Failure;
      }

      await UndoAsync(toVersionRaw!.Value);

      return Result.Success;
    }

    private Task UndoAsync(int value) {
      throw new System.NotImplementedException();
    }

    private bool UndoToVersionIsImpossible(short? toVersionRaw, short maxDatabasedMigrationVersion) {
      if (toVersionRaw is null) {
        LogWhatNotParsed("undo", _options.What);
        return true;
      }
      var toVersion = toVersionRaw.Value;
      if (toVersion > maxDatabasedMigrationVersion) {
        LogOnVersionBeforeRequested(toVersion, maxDatabasedMigrationVersion);
        return true;
      } else if (toVersion == maxDatabasedMigrationVersion) {
        LogAlreadyOnVersionRequested(maxDatabasedMigrationVersion, "undo");
        return true;
      } else if (toVersion <= 0) {
        LogCantDowngradeToZeroOrLess(toVersion, "undo");
      }

      // TODO: Make sure there are undo versions for each step...

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