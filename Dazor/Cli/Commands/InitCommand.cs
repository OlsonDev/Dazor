using System;
using System.Threading.Tasks;
using Dazor.Cli.Options;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Linq;
using System.IO;

namespace Dazor.Cli.Commands {
  internal class InitCommand {
    private readonly InitOptions _options;

    internal InitCommand(InitOptions options)
      => _options = options;

    internal async Task<Result> ExecuteAsync() {
      Console.WriteLine($"Init command with options: ConnectionString = {_options.ConnectionString}");
      await InitDazorSchemaAsync();
      return Result.Success;
    }

    private async Task InitDazorSchemaAsync() {
      using var connection = new SqlConnection(_options.ConnectionString);
      await connection.OpenAsync();
      using var transaction = connection.BeginTransaction();
      // TODO: This needs to be built into the app or found relative to the deployment.
      var command = await File.ReadAllTextAsync(@"C:/dev/Dazor/Dazor/Migrate/Schema/0001V Base Dazor schema.sql");
      var user = await connection.ExecuteAsync(command, null, transaction);
      await transaction.CommitAsync();
    }
  }
}