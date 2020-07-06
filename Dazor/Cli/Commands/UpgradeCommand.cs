using System;
using System.Linq;
using System.Threading.Tasks;
using Dazor.Cli.Options;
using Dazor.Config;
using Microsoft.Data.SqlClient;

namespace Dazor.Cli.Commands {
  internal class UpgradeCommand : VersioningCommandBase {
    private readonly UpgradeOptions _options;

    public UpgradeCommand(UpgradeOptions options, BoundConfig config, string[] commandLineArgs)
      : base(config, commandLineArgs)
      => _options = options;

    protected override async Task<Result> ExecuteAsync(int executionId, SqlConnection connection) {
      var validationContext = await ValidateAsync();
      // TODO: This needs to account for UndoVersions, or the query does.
      var maxDatabasedMigrationVersion = validationContext.MaxDatabasedMigrationVersion;

      var toVersion = short.TryParse(_options.ToVersion, out var parsed)
        ? parsed
        : short.MaxValue;

      // TODO: Validate that toVersion is even possible..

      if (toVersion < maxDatabasedMigrationVersion) {
        LogCantUpgrade(toVersion, maxDatabasedMigrationVersion);
        return Result.Failure;
      } else if (toVersion == maxDatabasedMigrationVersion) {
        LogNoNeedToUpgrade(maxDatabasedMigrationVersion);
        return Result.Success;
      }

      LogUpgradingFromXToY(toVersion, maxDatabasedMigrationVersion);
      await UpgradeAsync(toVersion, validationContext, executionId, connection);
      return Result.Success;
    }

    private async Task UpgradeAsync(short toVersion, ValidationContext validationContext, int executionId, SqlConnection connection) {
      var startVersion = validationContext.MaxDatabasedMigrationVersion;
      var newerVersions = validationContext
        .MigrationFiles
        .Where(mf => mf.MigrationType == MigrationType.Version && mf.Version > startVersion)
        .OrderBy(mf => mf.Version);

      foreach (var migration in newerVersions) {
        if (migration.Version > toVersion) break;
        LogUpgradingSingle(migration.Version!.Value, migration.Description);
        await ApplyMigrationAsync(migration, executionId, connection);
      }
    }

    private void LogCantUpgrade(short toVersion, short maxMigrationVersion) {
      var foregroundColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write("Already at version ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Error.Write(maxMigrationVersion);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write("; can't upgrade to ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Error.Write(toVersion);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.WriteLine(".");
      Console.ForegroundColor = foregroundColor;
    }

    private void LogNoNeedToUpgrade(short maxMigrationVersion) {
      var foregroundColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Write("Already at version ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Write(maxMigrationVersion);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine("; no need to upgrade.");
      Console.ForegroundColor = foregroundColor;
    }

    private void LogUpgradingFromXToY(short toVersion, short maxMigrationVersion) {
      var foregroundColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Magenta;
      if (maxMigrationVersion == 0) {
        Console.Write("Upgrading from a clean database to version ");
      } else {
        Console.Write("Upgrading from version ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(maxMigrationVersion);
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write(" to ");
      }
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Write(toVersion);
      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.WriteLine(".");
      Console.ForegroundColor = foregroundColor;
    }

    private void LogUpgradingSingle(short version, string description) {
      var foregroundColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.Write("Upgrading to version ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Write(version);
      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.Write(" (");
      Console.ForegroundColor = ConsoleColor.DarkMagenta;
      Console.Write(description);
      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.WriteLine(").");
      Console.ForegroundColor = foregroundColor;
    }
  }
}