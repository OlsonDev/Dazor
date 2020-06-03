using System;
using System.Threading.Tasks;
using Dapper;
using Dazor.Cli.Options;
using Dazor.Config;
using Microsoft.Data.SqlClient;

namespace Dazor.Cli.Commands {
  internal class CleanDataCommand : ICommand {
    private readonly CleanDataOptions _options;
    private readonly BoundConfig _config;

    private static readonly string BuildDynamicCleanCommand = @"
        DECLARE @ExcludedSchemaIDs TABLE (SchemaID INT NOT NULL PRIMARY KEY);
        INSERT @ExcludedSchemaIDs
        SELECT S.Schema_ID
        FROM (SELECT _ = NULL) AS _
        JOIN sys.Schemas       AS S  ON S.Name IN ('Dazor', 'sys', 'guest', 'INFORMATION_SCHEMA') OR Schema_ID >= 16384;


        DECLARE @SQL NVARCHAR(MAX) = N'SET NOCOUNT ON;';


        SET @SQL += @NewLine + '-- Disable triggers';
        WITH DistinctTablesWithTriggers AS (
          SELECT DISTINCT
              SchemaName = SCHEMA_NAME(O.Schema_ID)
            , TableName = O.Name
          FROM (SELECT _ = NULL) AS _
          JOIN sys.Triggers AS T ON T.Parent_Class = 1 -- Object or Column
          JOIN sys.Objects  AS O ON O.Object_ID = T.Parent_ID
          WHERE O.Schema_ID NOT IN (SELECT SchemaID FROM @ExcludedSchemaIDs)
        )
        SELECT @SQL += @NewLine + 'DISABLE TRIGGER ALL ON [' + T.SchemaName + '].[' + T.TableName + '];'
        FROM DistinctTablesWithTriggers AS T
        ORDER BY T.SchemaName, T.TableName;

        SET @SQL += @NewLine + '-- Disable constraints';
        WITH DistinctTablesWithConstraints AS (
          SELECT DISTINCT
              SchemaName = SCHEMA_NAME(O.Schema_ID)
            , TableName = O.Name
          FROM sys.Foreign_Keys AS FK
          JOIN sys.Objects      AS O ON O.Object_ID = FK.Parent_Object_ID
          WHERE O.Schema_ID NOT IN (SELECT SchemaID FROM @ExcludedSchemaIDs)
        )
        SELECT @SQL += @NewLine + 'ALTER TABLE [' + T.SchemaName + '].[' + T.TableName + '] NOCHECK CONSTRAINT ALL;'
        FROM DistinctTablesWithConstraints AS T
        ORDER BY T.SchemaName, T.TableName;

        SET @SQL += @NewLine + '-- Delete data';
        SELECT @SQL += @NewLine + 'DELETE FROM [' + SCHEMA_NAME(T.Schema_ID) + '].[' + T.Name + '];'
        FROM sys.Tables AS T
        WHERE T.Schema_ID NOT IN (SELECT SchemaID FROM @ExcludedSchemaIDs)
        ORDER BY SCHEMA_NAME(T.Schema_ID), T.Name;


        SET @SQL += @NewLine + '-- Reenable constraints';
        WITH DistinctTablesWithConstraints AS (
          SELECT DISTINCT
              SchemaName = SCHEMA_NAME(O.Schema_ID)
            , TableName = O.Name
          FROM sys.Foreign_Keys AS FK
          JOIN sys.Objects      AS O ON O.Object_ID = FK.Parent_Object_ID
          WHERE O.Schema_ID NOT IN (SELECT SchemaID FROM @ExcludedSchemaIDs)
        )
        SELECT @SQL += @NewLine + 'ALTER TABLE [' + T.SchemaName + '].[' + T.TableName + '] CHECK CONSTRAINT ALL;'
        FROM DistinctTablesWithConstraints AS T
        ORDER BY T.SchemaName, T.TableName;


        SET @SQL += @NewLine + '-- Reenable triggers';
        WITH DistinctTablesWithTriggers AS (
          SELECT DISTINCT
              SchemaName = SCHEMA_NAME(O.Schema_ID)
            , TableName = O.Name
          FROM (SELECT _ = NULL) AS _
          JOIN sys.Triggers AS T ON T.Parent_Class = 1 -- Object or Column
          JOIN sys.Objects  AS O ON O.Object_ID = T.Parent_ID
          WHERE O.Schema_ID NOT IN (SELECT SchemaID FROM @ExcludedSchemaIDs)
        )
        SELECT @SQL += @NewLine + 'ENABLE TRIGGER ALL ON [' + T.SchemaName + '].[' + T.TableName + '];'
        FROM DistinctTablesWithTriggers AS T
        ORDER BY T.SchemaName, T.TableName;


        SET @SQL += @NewLine + '-- Reseed identities which have had values before; avoid reseeding identities of tables which have never had records (since truncation)';
        SELECT @SQL += @NewLine + 'DBCC CHECKIDENT (''[' + SCHEMA_NAME(O.Schema_ID) + '].[' + O.Name + ']'', RESEED, ' + CONVERT(NVARCHAR(20), CONVERT(BIGINT, IC.Seed_Value) - 1) + ') WITH NO_INFOMSGS;'
        FROM sys.Identity_Columns AS IC
        JOIN sys.Objects          AS O   ON IC.Object_ID = O.Object_ID AND O.Schema_ID NOT IN (SELECT SchemaID FROM @ExcludedSchemaIDs)
        ORDER BY SCHEMA_NAME(O.Schema_ID), O.Name;";

    private static readonly string GetCleanCommand = BuildDynamicCleanCommand + Environment.NewLine + Environment.NewLine + "SELECT @SQL;";
    private static readonly string ExecuteCleanCommand = BuildDynamicCleanCommand + Environment.NewLine + Environment.NewLine + "EXEC SP_ExecuteSQL @SQL;";


    public CleanDataCommand(CleanDataOptions options, BoundConfig config) {
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