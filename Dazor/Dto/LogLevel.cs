namespace Dazor.Dto {
  internal class LogLevel {
    public int LogLevelId { get; set; }
    public string Name { get; set; }
    public LogLevel(int logLevelId, string name) {
      LogLevelId = logLevelId;
      Name = name;
    }
  }
}