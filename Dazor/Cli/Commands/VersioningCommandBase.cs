using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dazor.Config;
using Microsoft.Data.SqlClient;
using Dapper;
using Dazor.Dto;
using System.Diagnostics;

namespace Dazor.Cli.Commands {
  internal abstract class VersioningCommandBase : PostInitCommandBase {
    public VersioningCommandBase(BoundConfig config, string[] commandLineArgs)
      : base(config, commandLineArgs) { }

    protected async Task<ValidationContext> ValidateAsync() {
      var relativeDirectory = Path.Combine(Config.RootDirectory, "Migrations");
      var absoluteDirectory = Path.GetFullPath(relativeDirectory);

      var migrationFiles = Directory
        .GetFiles(relativeDirectory, "*.sql", SearchOption.AllDirectories)
        .Select(path => new MigrationFile(path))
        .OrderBy(mf => mf.MigrationType != MigrationType.Invalid)
        .ThenBy(mf => mf.MigrationType == MigrationType.Repeatable)
        .ThenBy(mf => mf.Version)
        .ThenBy(mf => mf.MigrationType) // Version before UndoVersion
        .ToArray();

      var cmd = @"
        SELECT
            M.MigrationID
          , M.DateTimeUtc
          , M.MigrationTypeID
          , M.Version
          , M.SizeInBytes
          , M.HashValue
          , M.HashFunction
          , M.Path
          , M.ExecutionTimeInMs
        FROM Dazor.Migration AS M
        ORDER BY M.DateTimeUtc;
      ";

      using var connection = new SqlConnection(Config.ConnectionString);
      await connection.OpenAsync();
      var migrations = await connection.QueryAsync<Migration>(cmd);

      var foregroundColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.Write(migrationFiles.Length);
      Console.ForegroundColor = ConsoleColor.White;
      Console.Write(migrationFiles.Length == 1 ? " migration in " : " migrations in ");
      Console.ForegroundColor = ConsoleColor.DarkGray;
      Console.Write(absoluteDirectory[..Environment.CurrentDirectory.Length]);
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine(absoluteDirectory[Environment.CurrentDirectory.Length..]);

      // TODO: Are all file versions >= 0001V?
      // TODO: Are there any versions present in the database, but not in the filesystem?
      //       Keep in mind it could've been undone and its previous incarnation would've been valid.
      // TODO: Are all checksums the same as those in the database?

      var invalidMigrationNameCount = 0;
      foreach (var migrationFile in migrationFiles) {
        if (invalidMigrationNameCount > 0 && migrationFile.MigrationType != MigrationType.Invalid) {
          break;
        }
        if (migrationFile.MigrationType == MigrationType.Invalid) invalidMigrationNameCount++;

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(migrationFile.HashValue.AsHexString());
        Console.ForegroundColor = ConsoleColorForMigrationType(migrationFile.MigrationType);
        Console.WriteLine($"  {migrationFile.FileName}");
      }

      if (invalidMigrationNameCount > 0) {
        Console.ForegroundColor = ConsoleColor.Red;
        if (invalidMigrationNameCount == 1) {
          Console.Error.WriteLine("This migration has an invalid name; migrations must be prefixed 'R ' or be '0001(V|U) ' or higher.");
        } else {
          Console.Error.Write("These ");
          Console.ForegroundColor = ConsoleColor.Magenta;
          Console.Error.Write(invalidMigrationNameCount);
          Console.ForegroundColor = ConsoleColor.Red;
          Console.Error.WriteLine(" migrations have invalid names; migrations must be prefixed 'R ' or be '0001(V|U) ' or higher.");
        }
      }

      Console.ForegroundColor = foregroundColor;

      return new ValidationContext(migrationFiles, migrations, invalidMigrationNameCount);
    }

    private static ConsoleColor ConsoleColorForMigrationType(MigrationType migrationType)
      => migrationType switch
      {
        MigrationType.Version => ConsoleColor.Cyan,
        MigrationType.UndoVersion => ConsoleColor.Blue,
        MigrationType.Repeatable => ConsoleColor.DarkGreen,
        MigrationType.Invalid => ConsoleColor.Red,
        _ => throw new NotImplementedException(),
      };

    protected static async Task ApplyMigrationAsync(MigrationFile migration, int executionId, SqlConnection connection) {
      var stopwatch = new Stopwatch();
      stopwatch.Start();
      using var transaction = await connection.BeginTransactionAsync();
      var cmd = await File.ReadAllTextAsync(migration.Path, System.Text.Encoding.UTF8);
      await connection.ExecuteAsync(cmd, transaction: transaction);
      stopwatch.Stop();

      var insertCmd = @"
        INSERT Dazor.Migration (
            DateTimeUtc
          , ExecutionID
          , MigrationTypeID
          , Version
          , SizeInBytes
          , HashValue
          , HashFunction
          , Path
          , ExecutionTimeInMs
        ) VALUES (
            SYSUTCDATETIME()
          , @ExecutionID
          , @MigrationType
          , @Version
          , @SizeInBytes
          , @HashValue
          , 'xxHash-64'
          , @Path
          , @ExecutionTimeInMs
        );
      ";

      var parameters = new {
        executionId
        , migration.MigrationType
        , migration.Version
        , migration.SizeInBytes
        , hashValue = migration.HashValue.Hash
        , migration.Path
        , executionTimeInMs = stopwatch.ElapsedMilliseconds
      };
      await connection.ExecuteAsync(insertCmd, parameters, transaction: transaction);
      await transaction.CommitAsync();
    }

    protected static void LogOnVersionAfterRequested(short toVersion, short maxDatabasedMigrationVersion) {
      var foregroundColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write("Already at version ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Error.Write(maxDatabasedMigrationVersion);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write("; can't upgrade to ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Error.Write(toVersion);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.WriteLine(".");
      Console.ForegroundColor = foregroundColor;
    }

    protected static void LogOnVersionBeforeRequested(short toVersion, short maxDatabasedMigrationVersion, string commandName) {
      var foregroundColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write("Already at version ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Error.Write(maxDatabasedMigrationVersion);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write("; can't ");
      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.Error.Write(commandName);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write(" to ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Error.Write(toVersion);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.WriteLine(".");
      Console.ForegroundColor = foregroundColor;
    }

    protected static void LogCantDowngradeToZeroOrLess(short toVersion, string commandName) {
      var foregroundColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write("Minimum version ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Error.Write("1");
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write($"; can't ");
      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.Error.Write(commandName);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write(" to ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Error.Write(toVersion);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.WriteLine(".");
      Console.ForegroundColor = foregroundColor;
    }

    protected static void LogCanOnlyDowngradeTo(short toVersion, short canOnlyBeAtVersion, string commandName) {
      var foregroundColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write("Cannot ");
      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.Error.Write(commandName);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write($" to version ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Error.Write(toVersion);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write("; undo versions present only allow going to version ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Error.Write(canOnlyBeAtVersion);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.WriteLine(".");
      Console.ForegroundColor = foregroundColor;
    }

    protected static void LogWhatNotParsed(string commandName, string what) {
      var foregroundColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write("Cannot ");
      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.Error.Write(commandName);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write($"; cannot parse `");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Error.Write(what);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.WriteLine("`.");
      Console.ForegroundColor = foregroundColor;
    }

    protected static void LogAlreadyOnVersionRequested(short maxDatabasedMigrationVersion, string commandName) {
      var foregroundColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write("Already at version ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.Error.Write(maxDatabasedMigrationVersion);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write("; no need to ");
      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.Error.Write(commandName);
      Console.ForegroundColor = ConsoleColor.Red;
      Console.Error.Write(".");
      Console.ForegroundColor = foregroundColor;
    }

    protected static void LogVersionRequestedDoesNotExist(short toVersion, short maxFileMigrationVersion) {
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

    protected static void LogUpgradingFromXToY(short toVersion, short maxMigrationVersion) {
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

    protected static void LogUpgradingSingle(short version, string description) {
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