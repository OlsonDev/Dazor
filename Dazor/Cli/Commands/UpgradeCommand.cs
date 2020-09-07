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
      if (validationContext.ShouldReportFailure) return Result.Failure;
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
  }
}