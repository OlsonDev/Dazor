using System;

namespace Dazor.Dto {
  internal class Migration {
    public long MigrationId { get; set; }
    public DateTime DateTimeUtc { get; set; }
    public byte MigrationTypeId { get; set; }
    public short Version { get; set; }
    public long SizeInBytes { get; set; }
    public byte[] HashValue { get; set; }
    public string HashFunction { get; set; }
    public string Path { get; set; }
    public int ExecutionTimeInMs { get; set; }
    public Migration(
      long migrationId,
      DateTime dateTimeUtc,
      byte migrationTypeId,
      short version,
      long sizeInBytes,
      byte[] hashValue,
      string hashFunction,
      string path,
      int executionTimeInMs) {
      MigrationId = migrationId;
      DateTimeUtc = dateTimeUtc;
      MigrationTypeId = migrationTypeId;
      Version = version;
      SizeInBytes = sizeInBytes;
      HashValue = hashValue;
      HashFunction = hashFunction;
      Path = path;
      ExecutionTimeInMs = executionTimeInMs;
    }
  }
}