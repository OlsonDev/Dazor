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

      if (toVersion < maxDatabasedMigrationVersion) {
        LogOnVersionAfterRequested(toVersion, maxDatabasedMigrationVersion);
        return Result.Failure;
      } else if (toVersion == maxDatabasedMigrationVersion) {
        LogAlreadyOnVersionRequested(maxDatabasedMigrationVersion);
        return Result.Success;
      } else if (validationContext.MaxFileMigrationVersion < toVersion) {
        LogVersionRequestedDoesNotExist(toVersion, validationContext.MaxFileMigrationVersion);
        return Result.Failure;
      }

      LogUpgradingFromXToY(toVersion, maxDatabasedMigrationVersion);
      await UpgradeAsync(toVersion, validationContext, executionId, connection);
      return Result.Success;
    }

    private static async Task UpgradeAsync(short toVersion, ValidationContext validationContext, int executionId, SqlConnection connection) {
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

    private static void LogOnVersionAfterRequested(short toVersion, short maxMigrationVersion) {
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

    private static void LogAlreadyOnVersionRequested(short maxMigrationVersion) {
      var foregroundColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Write("Already at version ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Write(maxMigrationVersion);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine("; no need to upgrade.");
      Console.ForegroundColor = foregroundColor;
    }

    private static void LogVersionRequestedDoesNotExist(short toVersion, short maxFileMigrationVersion) {
      var foregroundColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write("Max version is ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Error.Write(maxFileMigrationVersion);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write("; can't upgrade to ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Error.Write(toVersion);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.WriteLine(".");
      Console.ForegroundColor = foregroundColor;
    }

    private static void LogUpgradingFromXToY(short toVersion, short maxMigrationVersion) {
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

    private static void LogUpgradingSingle(short version, string description) {
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