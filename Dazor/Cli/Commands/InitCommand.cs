using System;
using System.Threading.Tasks;
using Dazor.Cli.Options;
using Microsoft.Data.SqlClient;
using Dapper;
using System.IO;
using System.Diagnostics;
using Dazor.Config;

namespace Dazor.Cli.Commands {
  internal class InitCommand : ICommand {
    private readonly InitOptions _options;

    internal InitCommand(InitOptions options)
      => _options = options;

    public async Task<Result> ExecuteAsync() {
      await WriteDazorJsonAsync();
      await WriteRootDirectoryContentAsync();
      await InitDazorSchemaAsync();
      return Result.Success;
    }

    private Task WriteDazorJsonAsync() {
      var fileConfig = new FileConfig {
        ConnectionString = _options.ConnectionString,
        RootDirectory = _options.RootDirectory,
        AutoFromClause = _options.AutoFromClause,
        AutoJoinClause = _options.AutoJoinClause,
        AutoParameterNameSuffix = _options.AutoParameterNameSuffix,
        GitHook = _options.GitHook,
        DefaultSeed = _options.DefaultSeed,
      };

      return Serializer.WriteAsync(fileConfig);
    }

    private async Task WriteRootDirectoryContentAsync() {
      var rootDirectory = Path.GetFullPath(_options.RootDirectory);
      var migrationsDirectory = Path.Combine(rootDirectory, "Migrations");
      var seedsDirectory = Path.Combine(rootDirectory, "Seeds");

      Directory.CreateDirectory(rootDirectory);
      Directory.CreateDirectory(migrationsDirectory);
      Directory.CreateDirectory(seedsDirectory);

      // TODO: These should come from a .resx file or something.
      var tasks = new Task[] {
        File.WriteAllTextAsync(Path.Combine(rootDirectory, "README.md"), "# Dazor"),
        File.WriteAllTextAsync(Path.Combine(migrationsDirectory, "README.md"), "# Dazor Migrations"),
        File.WriteAllTextAsync(Path.Combine(seedsDirectory, "README.md"), "# Dazor Seeds"),
      };

      await Task.WhenAll(tasks);
    }

    private async Task InitDazorSchemaAsync() {
      try {
        await TryInitDazorSchemaAsync(_options.ConnectionString);
      } catch (SqlException) {
        var builder = new SqlConnectionStringBuilder(_options.ConnectionString);
        var desiredDatabaseName = builder.InitialCatalog;
        builder.InitialCatalog = "master";
        var masterConnectionString = builder.ToString();
        await TryCreateDbAsync(masterConnectionString, desiredDatabaseName);
        await WaitForDatabaseReadyAsync(_options.ConnectionString);
        await TryInitDazorSchemaAsync(_options.ConnectionString);
      }
    }

    private async Task TryCreateDbAsync(string connectionString, string databaseName) {
      using var connection = new SqlConnection(connectionString);
      await connection.OpenAsync();
      await connection.ExecuteAsync($@"CREATE DATABASE [{databaseName}];");
    }

    private async Task WaitForDatabaseReadyAsync(string connectionString) {
      using var connection = new SqlConnection(connectionString);
      var stopwatch = new Stopwatch();
      var timeout = TimeSpan.FromSeconds(30);
      var delay = 100;
      while (true) {
        try {
          if (stopwatch.Elapsed >= timeout) throw new TimeoutException("Could not login to database after creating it.");
          await connection.OpenAsync();
          await connection.CloseAsync();
          break;
        } catch (SqlException ex) {
          // https://docs.microsoft.com/en-us/previous-versions/sql/sql-server-2008-r2/cc645613%28v%3dsql.105%29
          // Cannot open database "%.*ls" requested by the login. The login failed.
          if (ex.Number != 4060) throw;
          if (stopwatch.Elapsed >= timeout) throw;
          await Task.Delay(delay);
          delay = Math.Min(400, delay + 100);
        }
      }
    }

    private async Task TryInitDazorSchemaAsync(string connectionString) {
      using var connection = new SqlConnection(connectionString);
      await connection.OpenAsync();
      using var transaction = connection.BeginTransaction();
      // TODO: This needs to be built into the app or found relative to the deployment.
      var command = await File.ReadAllTextAsync(@"C:/dev/Dazor/Dazor/Migrate/Schema/0001V Base Dazor schema.sql");
      var user = await connection.ExecuteAsync(command, null, transaction);
      await transaction.CommitAsync();
    }
  }
}