using System;

namespace Dazor.Dto {
  internal class Log {
    public long LogId { get; set; }
    public DateTime DateTimeUtc { get; set; }
    public long ExecutionId { get; set; }
    public byte LogLevelId { get; set; }
    public string Message { get; set; }
    public Log(
      long logId,
      DateTime dateTimeUtc,
      long executionId,
      byte logLevelId,
      string message) {
      LogId = logId;
      DateTimeUtc = dateTimeUtc;
      ExecutionId = executionId;
      LogLevelId = logLevelId;
      Message = message;
    }
  }
}