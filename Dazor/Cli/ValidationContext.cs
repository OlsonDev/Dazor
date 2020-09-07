using System.Collections.Generic;
using System.Linq;
using Dazor.Dto;

namespace Dazor.Cli {
  internal class ValidationContext {
    private short? _maxDatabasedMigrationVersion;
    private short? _maxFileMigrationVersion;

    internal IEnumerable<MigrationFile> MigrationFiles { get; set; }
    internal IEnumerable<Migration> DatabasedMigrations { get; set; }
    internal int InvalidMigrationNameCount { get; set; }
    internal bool ShouldReportFailure => InvalidMigrationNameCount > 0;

    internal short MaxFileMigrationVersion
      => _maxFileMigrationVersion ??= MigrationFiles
        .Where(mf => mf.MigrationType == MigrationType.Version)
        .DefaultIfEmpty()
        .Max(m => m?.Version ?? 0);

    internal short MaxDatabasedMigrationVersion
      => _maxDatabasedMigrationVersion ??= DatabasedMigrations
        .DefaultIfEmpty()
        .Max(m => m?.Version ?? 0);

    public ValidationContext(
      IEnumerable<MigrationFile> migrationFiles,
      IEnumerable<Migration> databasedMigrations,
      int invalidMigrationNameCount) {
      MigrationFiles = migrationFiles;
      DatabasedMigrations = databasedMigrations;
      InvalidMigrationNameCount = invalidMigrationNameCount;
    }
  }
}