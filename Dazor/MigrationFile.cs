namespace Dazor {
  internal class MigrationFile {
    public string Path { get; set; }
    public byte[] Hash { get; set; }

    public MigrationFile(string path, byte[] hash) {
      Path = path;
      Hash = hash;
    }
  }
}