namespace Dazor.Cli {
  public static class Opts {
    public static string[] ConnectionString = { "-cs", "--connection-string" };
    public static string[] DataSource = { "-ds", "-s", "--datasource", "--server" };
    public static string[] Database = { "-db", "-ic", "--database", "--initial-catalog" };
    public static string[] IntegratedSecurity = { "-is", "--integrated-security" };
    public static string[] User = { "-u", "--user" };
    public static string[] Password = { "-pw", "--password" };
  }
}