DECLARE @DazorSchemaID INT = SCHEMA_ID('Dazor');

IF EXISTS (SELECT * FROM sys.Tables AS T WHERE T.Schema_ID = @DazorSchemaID AND T.Name = 'Migration') BEGIN
  DROP TABLE Dazor.Migration;
END

IF EXISTS (SELECT * FROM sys.Tables AS T WHERE T.Schema_ID = @DazorSchemaID AND T.Name = 'Log') BEGIN
  DROP TABLE Dazor.Log;
END

IF EXISTS (SELECT * FROM sys.Tables AS T WHERE T.Schema_ID = @DazorSchemaID AND T.Name = 'Execution') BEGIN
  DROP TABLE Dazor.Execution;
END

IF EXISTS (SELECT * FROM sys.Tables AS T WHERE T.Schema_ID = @DazorSchemaID AND T.Name = 'MigrationType') BEGIN
  DROP TABLE Dazor.MigrationType;
END

IF EXISTS (SELECT * FROM sys.Tables AS T WHERE T.Schema_ID = @DazorSchemaID AND T.Name = 'Result') BEGIN
  DROP TABLE Dazor.Result;
END

IF EXISTS (SELECT * FROM sys.Tables AS T WHERE T.Schema_ID = @DazorSchemaID AND T.Name = 'LogLevel') BEGIN
  DROP TABLE Dazor.LogLevel;
END

-- NOT NULL:
IF TYPE_ID('Dazor.BitList')                    IS NOT NULL DROP TYPE Dazor.BitList;
IF TYPE_ID('Dazor.TinyIntList')                IS NOT NULL DROP TYPE Dazor.TinyIntList;
IF TYPE_ID('Dazor.SmallIntList')               IS NOT NULL DROP TYPE Dazor.SmallIntList;
IF TYPE_ID('Dazor.IntList')                    IS NOT NULL DROP TYPE Dazor.IntList;
IF TYPE_ID('Dazor.BigIntList')                 IS NOT NULL DROP TYPE Dazor.BigIntList;
IF TYPE_ID('Dazor.GuidList')                   IS NOT NULL DROP TYPE Dazor.GuidList;
IF TYPE_ID('Dazor.FloatList')                  IS NOT NULL DROP TYPE Dazor.FloatList;
IF TYPE_ID('Dazor.DoubleList')                 IS NOT NULL DROP TYPE Dazor.DoubleList;
IF TYPE_ID('Dazor.DateList')                   IS NOT NULL DROP TYPE Dazor.DateList;
IF TYPE_ID('Dazor.TimeList')                   IS NOT NULL DROP TYPE Dazor.TimeList;
IF TYPE_ID('Dazor.DateTime2List')              IS NOT NULL DROP TYPE Dazor.DateTime2List;
IF TYPE_ID('Dazor.SmallDateTimeList')          IS NOT NULL DROP TYPE Dazor.SmallDateTimeList;
IF TYPE_ID('Dazor.ShortVarCharList')           IS NOT NULL DROP TYPE Dazor.ShortVarCharList;
IF TYPE_ID('Dazor.LongVarCharList')            IS NOT NULL DROP TYPE Dazor.LongVarCharList;
IF TYPE_ID('Dazor.MaxVarCharList')             IS NOT NULL DROP TYPE Dazor.MaxVarCharList;
IF TYPE_ID('Dazor.ShortNVarCharList')          IS NOT NULL DROP TYPE Dazor.ShortNVarCharList;
IF TYPE_ID('Dazor.LongNVarCharList')           IS NOT NULL DROP TYPE Dazor.LongNVarCharList;
IF TYPE_ID('Dazor.MaxNVarCharList')            IS NOT NULL DROP TYPE Dazor.MaxNVarCharList;
-- NULL:
IF TYPE_ID('Dazor.NullableBitList')            IS NOT NULL DROP TYPE Dazor.NullableBitList;
IF TYPE_ID('Dazor.NullableTinyIntList')        IS NOT NULL DROP TYPE Dazor.NullableTinyIntList;
IF TYPE_ID('Dazor.NullableSmallIntList')       IS NOT NULL DROP TYPE Dazor.NullableSmallIntList;
IF TYPE_ID('Dazor.NullableIntList')            IS NOT NULL DROP TYPE Dazor.NullableIntList;
IF TYPE_ID('Dazor.NullableBigIntList')         IS NOT NULL DROP TYPE Dazor.NullableBigIntList;
IF TYPE_ID('Dazor.NullableGuidList')           IS NOT NULL DROP TYPE Dazor.NullableGuidList;
IF TYPE_ID('Dazor.NullableFloatList')          IS NOT NULL DROP TYPE Dazor.NullableFloatList;
IF TYPE_ID('Dazor.NullableDoubleList')         IS NOT NULL DROP TYPE Dazor.NullableDoubleList;
IF TYPE_ID('Dazor.NullableDateList')           IS NOT NULL DROP TYPE Dazor.NullableDateList;
IF TYPE_ID('Dazor.NullableTimeList')           IS NOT NULL DROP TYPE Dazor.NullableTimeList;
IF TYPE_ID('Dazor.NullableDateTime2List')      IS NOT NULL DROP TYPE Dazor.NullableDateTime2List;
IF TYPE_ID('Dazor.NullableSmallDateTimeList')  IS NOT NULL DROP TYPE Dazor.NullableSmallDateTimeList;
IF TYPE_ID('Dazor.NullableShortVarCharList')   IS NOT NULL DROP TYPE Dazor.NullableShortVarCharList;
IF TYPE_ID('Dazor.NullableLongVarCharList')    IS NOT NULL DROP TYPE Dazor.NullableLongVarCharList;
IF TYPE_ID('Dazor.NullableMaxVarCharList')     IS NOT NULL DROP TYPE Dazor.NullableMaxVarCharList;
IF TYPE_ID('Dazor.NullableShortNVarCharList')  IS NOT NULL DROP TYPE Dazor.NullableShortNVarCharList;
IF TYPE_ID('Dazor.NullableLongNVarCharList')   IS NOT NULL DROP TYPE Dazor.NullableLongNVarCharList;
IF TYPE_ID('Dazor.NullableMaxNVarCharList')    IS NOT NULL DROP TYPE Dazor.NullableMaxNVarCharList;