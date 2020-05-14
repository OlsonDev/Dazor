IF SCHEMA_ID('Dazor') IS NULL EXEC('CREATE SCHEMA Dazor;');

IF OBJECT_ID('Dazor.LogLevel', 'U') IS NULL BEGIN
  CREATE TABLE Dazor.LogLevel (
      LogLevelID  TINYINT      NOT NULL IDENTITY(0, 1)
    , Name        VARCHAR(20)  NOT NULL
    , CONSTRAINT PK_LogLevel PRIMARY KEY ( LogLevelID )
    , CONSTRAINT UQ_LogLevel_Name UNIQUE ( Name )
  );

  -- Starting at IDENTITY 0 because these should map to Microsoft.Extensions.Logging.LogLevel.
  INSERT Dazor.LogLevel ( Name )
  VALUES
      ( 'Trace' )
    , ( 'Debug' )
    , ( 'Information' )
    , ( 'Warning' )
    , ( 'Error' )
    , ( 'Critical' )
    , ( 'None' );
END

IF OBJECT_ID('Dazor.Result', 'U') IS NULL BEGIN
  CREATE TABLE Dazor.Result (
      ResultID  TINYINT      NOT NULL IDENTITY(0, 1)
    , Name      VARCHAR(50)  NOT NULL
    , CONSTRAINT PK_Result PRIMARY KEY ( ResultID )
    , CONSTRAINT UQ_Result_Name UNIQUE ( Name )
  );

  -- Starting at IDENTITY 0 because these should map to the Dazor.Result enum (CLI exit codes).
  INSERT Dazor.Result ( Name )
  VALUES
      ( 'Success' )
    , ( 'Failure' );
END

IF OBJECT_ID('Dazor.MigrationType', 'U') IS NULL BEGIN
  CREATE TABLE Dazor.MigrationType (
      MigrationTypeID  TINYINT      NOT NULL IDENTITY(0, 1)
    , Name             VARCHAR(50)  NOT NULL
    , CONSTRAINT PK_MigrationType PRIMARY KEY ( MigrationTypeID )
    , CONSTRAINT UQ_MigrationType_Name UNIQUE ( Name )
  );

  -- Starting at IDENTITY 0 because these should map to the Dazor.MigrationType enum.
  INSERT Dazor.MigrationType ( Name )
  VALUES
      ( 'Version' )
    , ( 'UndoVersion' )
    , ( 'Repeatable' );
END

IF OBJECT_ID('Dazor.Execution', 'U') IS NULL BEGIN
  CREATE TABLE Dazor.Execution (
      ExecutionID        BIGINT         NOT NULL IDENTITY
    , DateTimeUtc        DATETIME2(7)   NOT NULL
    , Args               NVARCHAR(MAX)  NOT NULL
    , ResultID           TINYINT            NULL
    , ExecutionTimeInMs  INT                NULL
    , CONSTRAINT PK_Execution PRIMARY KEY ( ExecutionID )
    , CONSTRAINT FK_Execution_Result FOREIGN KEY (ResultID) REFERENCES Dazor.Result (ResultID)
  )
END

IF OBJECT_ID('Dazor.Log', 'U') IS NULL BEGIN
  CREATE TABLE Dazor.Log (
      LogID        BIGINT         NOT NULL IDENTITY
    , DateTimeUtc  DATETIME2(7)   NOT NULL
    , ExecutionID  BIGINT         NOT NULL
    , LogLevelID   TINYINT        NOT NULL
    , Message      NVARCHAR(MAX)  NOT NULL
    , CONSTRAINT PK_Log PRIMARY KEY ( LogID )
    , CONSTRAINT FK_Log_Execution FOREIGN KEY (ExecutionID) REFERENCES Dazor.Execution (ExecutionID)
    , CONSTRAINT FK_Log_LogLevel  FOREIGN KEY (LogLevelID)  REFERENCES Dazor.LogLevel (LogLevelID)
  )
END

IF OBJECT_ID('Dazor.Migration', 'U') IS NULL BEGIN
  CREATE TABLE Dazor.Migration (
      MigrationID        BIGINT         NOT NULL IDENTITY
    , DateTimeUtc        DATETIME2(7)   NOT NULL
    , MigrationTypeID    TINYINT        NOT NULL
    , Version            SMALLINT       NOT NULL
    , SizeInBytes        BIGINT         NOT NULL
    , Blake3Hash         BINARY(512)    NOT NULL
    , Path               NVARCHAR(MAX)  NOT NULL
    , ExecutionTimeInMs  INT            NOT NULL
    , CONSTRAINT PK_Migration PRIMARY KEY ( MigrationID )
    , CONSTRAINT FK_Migration_MigrationType FOREIGN KEY (MigrationTypeID) REFERENCES Dazor.MigrationType (MigrationTypeID)
    , CONSTRAINT UQ_Migration_MigrationType_Version UNIQUE (MigrationTypeID, Version)
  )
END

-- Common TVP types to be used in generated queries...
-- NOT NULL:
IF TYPE_ID('Dazor.BitList')                    IS NULL CREATE TYPE Dazor.BitList                    AS TABLE ( Value        BIT          NOT NULL );
IF TYPE_ID('Dazor.TinyIntList')                IS NULL CREATE TYPE Dazor.TinyIntList                AS TABLE ( Value    TINYINT          NOT NULL );
IF TYPE_ID('Dazor.SmallIntList')               IS NULL CREATE TYPE Dazor.SmallIntList               AS TABLE ( Value   SMALLINT          NOT NULL );
IF TYPE_ID('Dazor.IntList')                    IS NULL CREATE TYPE Dazor.IntList                    AS TABLE ( Value        INT          NOT NULL );
IF TYPE_ID('Dazor.BigIntList')                 IS NULL CREATE TYPE Dazor.BigIntList                 AS TABLE ( Value     BIGINT          NOT NULL );
IF TYPE_ID('Dazor.GuidList')                   IS NULL CREATE TYPE Dazor.GuidList                   AS TABLE ( Value  UNIQUEIDENTIFIER   NOT NULL );
IF TYPE_ID('Dazor.FloatList')                  IS NULL CREATE TYPE Dazor.FloatList                  AS TABLE ( Value  FLOAT(24)          NOT NULL );
IF TYPE_ID('Dazor.DoubleList')                 IS NULL CREATE TYPE Dazor.DoubleList                 AS TABLE ( Value  FLOAT(53)          NOT NULL );
IF TYPE_ID('Dazor.DateList')                   IS NULL CREATE TYPE Dazor.DateList                   AS TABLE ( Value       DATE          NOT NULL );
IF TYPE_ID('Dazor.TimeList')                   IS NULL CREATE TYPE Dazor.TimeList                   AS TABLE ( Value           TIME      NOT NULL );
IF TYPE_ID('Dazor.DateTime2List')              IS NULL CREATE TYPE Dazor.DateTime2List              AS TABLE ( Value       DATETIME2(7)  NOT NULL );
IF TYPE_ID('Dazor.SmallDateTimeList')          IS NULL CREATE TYPE Dazor.SmallDateTimeList          AS TABLE ( Value  SMALLDATETIME      NOT NULL );
IF TYPE_ID('Dazor.ShortVarCharList')           IS NULL CREATE TYPE Dazor.ShortVarCharList           AS TABLE ( Value   VARCHAR(128)      NOT NULL );
IF TYPE_ID('Dazor.LongVarCharList')            IS NULL CREATE TYPE Dazor.LongVarCharList            AS TABLE ( Value   VARCHAR(512)      NOT NULL );
IF TYPE_ID('Dazor.MaxVarCharList')             IS NULL CREATE TYPE Dazor.MaxVarCharList             AS TABLE ( Value   VARCHAR(MAX)      NOT NULL );
IF TYPE_ID('Dazor.ShortNVarCharList')          IS NULL CREATE TYPE Dazor.ShortNVarCharList          AS TABLE ( Value  NVARCHAR(128)      NOT NULL );
IF TYPE_ID('Dazor.LongNVarCharList')           IS NULL CREATE TYPE Dazor.LongNVarCharList           AS TABLE ( Value  NVARCHAR(512)      NOT NULL );
IF TYPE_ID('Dazor.MaxNVarCharList')            IS NULL CREATE TYPE Dazor.MaxNVarCharList            AS TABLE ( Value  NVARCHAR(MAX)      NOT NULL );
-- NULL:
IF TYPE_ID('Dazor.NullableBitList')            IS NULL CREATE TYPE Dazor.NullableBitList            AS TABLE ( Value        BIT              NULL );
IF TYPE_ID('Dazor.NullableTinyIntList')        IS NULL CREATE TYPE Dazor.NullableTinyIntList        AS TABLE ( Value    TINYINT              NULL );
IF TYPE_ID('Dazor.NullableSmallIntList')       IS NULL CREATE TYPE Dazor.NullableSmallIntList       AS TABLE ( Value   SMALLINT              NULL );
IF TYPE_ID('Dazor.NullableIntList')            IS NULL CREATE TYPE Dazor.NullableIntList            AS TABLE ( Value        INT              NULL );
IF TYPE_ID('Dazor.NullableBigIntList')         IS NULL CREATE TYPE Dazor.NullableBigIntList         AS TABLE ( Value     BIGINT              NULL );
IF TYPE_ID('Dazor.NullableGuidList')           IS NULL CREATE TYPE Dazor.NullableGuidList           AS TABLE ( Value  UNIQUEIDENTIFIER       NULL );
IF TYPE_ID('Dazor.NullableFloatList')          IS NULL CREATE TYPE Dazor.NullableFloatList          AS TABLE ( Value  FLOAT(24)              NULL );
IF TYPE_ID('Dazor.NullableDoubleList')         IS NULL CREATE TYPE Dazor.NullableDoubleList         AS TABLE ( Value  FLOAT(53)              NULL );
IF TYPE_ID('Dazor.NullableDateList')           IS NULL CREATE TYPE Dazor.NullableDateList           AS TABLE ( Value       DATE              NULL );
IF TYPE_ID('Dazor.NullableTimeList')           IS NULL CREATE TYPE Dazor.NullableTimeList           AS TABLE ( Value           TIME          NULL );
IF TYPE_ID('Dazor.NullableDateTime2List')      IS NULL CREATE TYPE Dazor.NullableDateTime2List      AS TABLE ( Value       DATETIME2(7)      NULL );
IF TYPE_ID('Dazor.NullableSmallDateTimeList')  IS NULL CREATE TYPE Dazor.NullableSmallDateTimeList  AS TABLE ( Value  SMALLDATETIME          NULL );
IF TYPE_ID('Dazor.NullableShortVarCharList')   IS NULL CREATE TYPE Dazor.NullableShortVarCharList   AS TABLE ( Value   VARCHAR(128)          NULL );
IF TYPE_ID('Dazor.NullableLongVarCharList')    IS NULL CREATE TYPE Dazor.NullableLongVarCharList    AS TABLE ( Value   VARCHAR(512)          NULL );
IF TYPE_ID('Dazor.NullableMaxVarCharList')     IS NULL CREATE TYPE Dazor.NullableMaxVarCharList     AS TABLE ( Value   VARCHAR(MAX)          NULL );
IF TYPE_ID('Dazor.NullableShortNVarCharList')  IS NULL CREATE TYPE Dazor.NullableShortNVarCharList  AS TABLE ( Value  NVARCHAR(128)          NULL );
IF TYPE_ID('Dazor.NullableLongNVarCharList')   IS NULL CREATE TYPE Dazor.NullableLongNVarCharList   AS TABLE ( Value  NVARCHAR(512)          NULL );
IF TYPE_ID('Dazor.NullableMaxNVarCharList')    IS NULL CREATE TYPE Dazor.NullableMaxNVarCharList    AS TABLE ( Value  NVARCHAR(MAX)          NULL );

/*
SELECT * FROM Dazor.LogLevel;
SELECT * FROM Dazor.Result;
SELECT * FROM Dazor.MigrationType
SELECT * FROM Dazor.Execution;
SELECT * FROM Dazor.Log;
SELECT * FROM Dazor.Migration;
*/