namespace Dazor {
  internal enum MigrationType : byte {
    Version,
    UndoVersion,
    Repeatable,
    Invalid = 0xFF,
  }
}