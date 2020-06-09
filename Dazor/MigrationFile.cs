namespace Dazor {
  internal class MigrationFile {
    public string Path { get; set; }
    public byte[] Checksum { get; set; }

    public MigrationFile(string path, byte[] checksum) {
      Path = path;
      Checksum = checksum;
    }
  }
}