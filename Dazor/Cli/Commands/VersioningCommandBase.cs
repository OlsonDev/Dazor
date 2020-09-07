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
        .OrderBy(name => name)
        .Select(path => new MigrationFile(path))
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
      // TODO: Are all checksums the same as those in the database?

      foreach (var migrationFile in migrationFiles) {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(migrationFile.HashValue.AsHexString());
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  {migrationFile.FileName}");
      }
      Console.ForegroundColor = foregroundColor;

      return new ValidationContext(migrationFiles, migrations);
    }

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

      // TODO: Log migration...
      await transaction.CommitAsync();
    }
  }
}