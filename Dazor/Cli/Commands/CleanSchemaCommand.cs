using System;
using System.Threading.Tasks;
using Dapper;
using Dazor.Cli.Options;
using Dazor.Config;
using Microsoft.Data.SqlClient;

namespace Dazor.Cli.Commands {
  internal class CleanSchemaCommand : ICommand {
    private readonly CleanSchemaOptions _options;
    private readonly BoundConfig _config;

    private static readonly string BuildDynamicCleanCommand = @"
        DECLARE @ExcludedSchemaIDs TABLE (SchemaID INT NOT NULL PRIMARY KEY);
        INSERT @ExcludedSchemaIDs
        SELECT S.Schema_ID
        FROM (SELECT _ = NULL) AS _
        JOIN sys.Schemas       AS S  ON S.Name IN ('Dazor', 'sys', 'guest', 'INFORMATION_SCHEMA') OR Schema_ID >= 16384;


        DECLARE @SQL NVARCHAR(MAX) = N'';


        SET @SQL += '-- Procedures';
        SELECT @SQL += @NewLine + 'DROP PROCEDURE [' + SCHEMA_NAME(P.Schema_ID) + '].[' + P.Name + '];'
        FROM sys.Procedures AS P
        WHERE P.Schema_ID NOT IN (SELECT SchemaID FROM @ExcludedSchemaIDs);


        SET @SQL += @NewLine + '-- Check constraints';
        SELECT @SQL += @NewLine + 'ALTER TABLE [' + SCHEMA_NAME(CC.Schema_ID) + '].[' + OBJECT_NAME(CC.Parent_Object_ID) + ']' + @NewLine + '  DROP CONSTRAINT ' + CC.Name + '];'
        FROM sys.Check_Constraints AS CC
        WHERE CC.Schema_ID NOT IN (SELECT SchemaID FROM @ExcludedSchemaIDs);


        SET @SQL += @NewLine + '-- Functions';
        SELECT @SQL += @NewLine + 'DROP FUNCTION [' + SCHEMA_NAME(F.Schema_ID) + '].[' + F.Name + '];'
        FROM sys.Objects AS F
        WHERE F.Schema_ID NOT IN (SELECT SchemaID FROM @ExcludedSchemaIDs)
          AND F.Type IN ('FN', 'IF', 'TF');


        SET @SQL += @NewLine + '-- Views';
        SELECT @SQL += @NewLine + 'DROP VIEW [' + SCHEMA_NAME(V.Schema_ID) + '].[' + V.Name + '];'
        FROM sys.Views AS V
        WHERE V.Schema_ID NOT IN (SELECT SchemaID FROM @ExcludedSchemaIDs);

        SET @SQL += @NewLine + '-- Foreign keys';
        SELECT @SQL += @NewLine + 'ALTER TABLE [' + SCHEMA_NAME(FK.Schema_ID) + '].[' + OBJECT_NAME(FK.Parent_Object_ID ) + ']' + @NewLine + '  DROP CONSTRAINT [' + FK.Name + '];'
        FROM sys.Foreign_Keys AS FK
        WHERE FK.Schema_ID NOT IN (SELECT SchemaID FROM @ExcludedSchemaIDs);


        SET @SQL += @NewLine + '-- Temporal (system-versioned) tables';
        SELECT @SQL += @NewLine + 'ALTER TABLE [' + SCHEMA_NAME(T.Schema_ID) + '].[' + T.Name + ']' + @NewLine + '  SET (SYSTEM_VERSIONING = OFF);'
        FROM sys.Tables AS T
        WHERE T.Schema_ID NOT IN (SELECT SchemaID FROM @ExcludedSchemaIDs)
          AND T.Temporal_Type = 2;


        SET @SQL += @NewLine + '-- Tables';
        SELECT @SQL += @NewLine + 'DROP TABLE [' + SCHEMA_NAME(T.Schema_ID) + '].[' + T.Name + '];'
        FROM sys.Tables AS T
        WHERE T.Schema_ID NOT IN (SELECT SchemaID FROM @ExcludedSchemaIDs);


        SET @SQL += @NewLine + '-- User Defined Types (UDTs)';
        SELECT @SQL += @NewLine + 'DROP TYPE [' + SCHEMA_NAME(T.Schema_ID) + '].[' + T.Name + '];'
        FROM sys.Types AS T
        WHERE T.Schema_ID NOT IN (SELECT SchemaID FROM @ExcludedSchemaIDs)
          AND T.Is_User_Defined = 1;";

    private static readonly string GetCleanCommand = BuildDynamicCleanCommand + Environment.NewLine + Environment.NewLine + "SELECT @SQL;";
    private static readonly string ExecuteCleanCommand = BuildDynamicCleanCommand + Environment.NewLine + Environment.NewLine + "EXEC SP_ExecuteSQL @SQL;";


    public CleanSchemaCommand(CleanSchemaOptions options, BoundConfig config) {
      _options = options;
      _config = config;
    }

    public async Task<Result> ExecuteAsync() {
      using var connection = new SqlConnection(_config.ConnectionString);
      var parameters = new DynamicParameters();
      parameters.Add("NewLine", Environment.NewLine);
      await connection.OpenAsync();
      if (_options.DryRun) {
        var cmd = await connection.QueryFirstAsync<string>(GetCleanCommand, parameters);
        Console.WriteLine("-- If this wasn't a dry run, would have executed this:");
        Console.WriteLine();
        Console.WriteLine(cmd);
      } else {
        await connection.ExecuteAsync(ExecuteCleanCommand, parameters);
      }
      await connection.CloseAsync();
      return Result.Success;
    }
  }
}