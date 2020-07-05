namespace Dazor.Dto {
  internal class Result {
    public byte ResultId { get; set; }
    public string Name { get; set; }
    public Result(byte resultId, string name) {
      ResultId = resultId;
      Name = name;
    }
  }
}