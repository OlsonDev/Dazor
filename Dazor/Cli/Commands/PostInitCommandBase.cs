using System.Diagnostics;
using System.Threading.Tasks;
using Dazor.Config;
using Microsoft.Data.SqlClient;
using Dapper;
using System;
using Microsoft.Extensions.Logging;
using Dazor.Extensions;

namespace Dazor.Cli.Commands {
  internal abstract class PostInitCommandBase : ICommand {
    protected BoundConfig Config { get; }

    protected string[] CommandLineArgs { get; }

    protected PostInitCommandBase(BoundConfig config, string[] commandLineArgs) {
      Config = config;
      CommandLineArgs = commandLineArgs;
    }

    public async Task<Result> ExecuteAsync() {
      // TODO: Timer should start before parsing...
      var stopwatch = new Stopwatch();
      stopwatch.Start();
      var connection = new SqlConnection(Config.ConnectionString);
      await connection.OpenAsync();
      var insertCmd = @"
        INSERT Dazor.Execution ( DateTimeUtc , Args )
        VALUES ( SYSUTCDATETIME(), @Args );
        SELECT SCOPE_IDENTITY();
      ";

      var executionId = await connection.ExecuteScalarAsync<int>(insertCmd, new { args = string.Join(" ", CommandLineArgs) });

      var result = Result.InternalError;
      try {
        result = await ExecuteAsync(executionId, connection);
      } catch (Exception ex) {
        stopwatch.Stop();
        Console.Error.WriteLine(ex);
        await connection.ExecuteAsync(@"
          INSERT Dazor.Log ( DateTimeUtc, ExecutionID, LogLevelID, Message )
          VALUES ( SYSUTCDATETIME(), @ExecutionID, @LogLevelID, @Message );
        ", new { executionId, logLevelId = (int)LogLevel.Critical, message = ex.PrettyPrint() });
      }

      stopwatch.Stop();

      var updateCmd = @"
        UPDATE Dazor.Execution SET
            ResultID           = @Result
          , ExecutionTimeInMs  = @ExecutionTimeInMs
        WHERE ExecutionID      = @ExecutionID;
      ";

      await connection.ExecuteAsync(updateCmd, new { result, executionId, executionTimeInMs = stopwatch.ElapsedMilliseconds });
      await connection.CloseAsync();
      return result;
    }

    protected abstract Task<Result> ExecuteAsync(int executionId, SqlConnection connection);
  }
}