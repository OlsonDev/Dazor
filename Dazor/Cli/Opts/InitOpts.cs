namespace Dazor.Cli.Opts {
  public static class InitOpts {
    public static readonly string[] ConnectionString = { "-cs", "--connection-string" };
    public static readonly string[] RootDirectory = { "-p", "-d", "--path", "--directory" };
    public static readonly string[] DataSource = { "-ds", "-s", "--datasource", "--server" };
    public static readonly string[] Database = { "-db", "-ic", "--database", "--initial-catalog" };
    public static readonly string[] IntegratedSecurity = { "-is", "--integrated-security" };
    public static readonly string[] User = { "-u", "--user" };
    public static readonly string[] Password = { "-pw", "--password" };
    public static readonly string[] AutoFromClause = { "--auto-from" };
    public static readonly string[] AutoJoinClause = { "--auto-join" };
    public static readonly string[] AutoParameterNameSuffix = { "--auto-param-suffix" };
    public static readonly string[] GitHook = { "--git-hook" };
    public static readonly string[] DefaultSeed = { "--default-seed" };
  }
}