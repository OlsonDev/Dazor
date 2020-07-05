using System;

namespace Dazor.Dto {
  internal class Execution {
    internal long ExecutionId { get; set; }
    public DateTime DateTimeUtc { get; set; }
    public string Args { get; set; }
    public byte ResultId { get; set; }
    public int ExecutionTimeInMs { get; set; }

    public Execution(
      long executionId,
      DateTime dateTimeUtc,
      string args,
      byte resultId,
      int executionTimeInMs) {
      ExecutionId = executionId;
      DateTimeUtc = dateTimeUtc;
      Args = args;
      ResultId = resultId;
      ExecutionTimeInMs = executionTimeInMs;
    }
  }
}