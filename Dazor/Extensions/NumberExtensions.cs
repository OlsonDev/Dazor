namespace Dazor.Extensions {
  public static class NumberExtensions {
    public static bool IsInRangeInclusive(this short value, short min, short max)
      => value >= min && value <= max;
  }
}