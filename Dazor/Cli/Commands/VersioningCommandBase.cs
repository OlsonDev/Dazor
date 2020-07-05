using System;
using System.Data.HashFunction.xxHash;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dazor.Config;
using Microsoft.Data.SqlClient;
using Dapper;
using Dazor.Dto;
using System.Collections.Generic;

namespace Dazor.Cli.Commands {
  internal abstract class VersioningCommandBase : ICommand {
    protected BoundConfig Config { get; }
    public VersioningCommandBase(BoundConfig config)
      => Config = config;

    public abstract Task<Result> ExecuteAsync();

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
  }
}