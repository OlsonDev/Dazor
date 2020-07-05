namespace Dazor.Dto {
  internal class MigrationType {
    public byte MigrationTypeId { get; set; }
    public string Name { get; set; }
    public MigrationType(byte migrationTypeId, string name) {
      MigrationTypeId = migrationTypeId;
      Name = name;
    }
  }
}