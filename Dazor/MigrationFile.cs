using System.Data.HashFunction;
using System.Data.HashFunction.xxHash;
using System.IO;
using System.Text.RegularExpressions;

namespace Dazor {
  internal class MigrationFile {
    private static readonly IxxHashConfig HashConfig = new xxHashConfig { HashSizeInBits = 64 };
    private static readonly IxxHash Hasher = xxHashFactory.Instance.Create(HashConfig);
    private static readonly RegexOptions RegexOptions = RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant;
    private static readonly Regex VersionRegex = new Regex("^(?<version>000[1-9]|[0-9]{2}[1-9][0-9]|[0-9][1-9][0-9]{2}|[1-9][0-9]{3})(?<postfix>V|U) (?<description>.+)$", RegexOptions, Regex.InfiniteMatchTimeout);

    private string? _fileName;
    private IHashValue? _hashValue;
    private FileMetadata? _fileMetadata;
    public string Path { get; }
    public string FileName => _fileName ??= System.IO.Path.GetFileNameWithoutExtension(Path);
    public long SizeInBytes => (_fileMetadata ??= ComputeFileMetadata()).SizeInBytes;
    public MigrationType MigrationType => (_fileMetadata ??= ComputeFileMetadata()).MigrationType;
    public short? Version => (_fileMetadata ??= ComputeFileMetadata()).Version;
    public string? VersionString => (_fileMetadata ??= ComputeFileMetadata()).VersionString;
    public string Description => (_fileMetadata ??= ComputeFileMetadata()).Description;
    public IHashValue HashValue => _hashValue ??= ComputeHashValue();
    public MigrationFile(string path) => Path = path;

    private IHashValue ComputeHashValue() {
      using var stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read);
      return _hashValue = Hasher.ComputeHash(stream);
    }

    private FileMetadata ComputeFileMetadata() {
      var sizeInBytes = new FileInfo(Path).Length;
      // TODO: Enforce good practices; all types should be in specific directories by convention; repeatables shouldn't need a prefix.
      if (FileName.StartsWith("R ")) return new FileMetadata(sizeInBytes, MigrationType.Repeatable, null, FileName[2..]);
      var match = VersionRegex.Match(FileName);
      if (match.Success) {
        var version = match.Groups["version"].Value;
        var postfix = match.Groups["postfix"].Value;
        var description = match.Groups["description"].Value;
        var type = postfix == "V"
          ? MigrationType.Version
          : MigrationType.UndoVersion;
        return new FileMetadata(sizeInBytes, type, version, description);
      }
      return new FileMetadata(sizeInBytes, MigrationType.Invalid, null, FileName);
    }

    private class FileMetadata {
      public long SizeInBytes { get; set; }
      public MigrationType MigrationType { get; set; }
      public short? Version { get; set; }
      public string? VersionString { get; set; }
      public string Description { get; set; }
      public FileMetadata(
        long sizeInBytes,
        MigrationType migrationType,
        string? version,
        string description) {
        SizeInBytes = sizeInBytes;
        MigrationType = migrationType;
        VersionString = version;
        Version = short.TryParse(version, out var parsed) ? parsed : (short)0;
        Description = description;
      }
    }
  }
}