using System.Threading.Tasks;

namespace Dazor.Cli {
  internal interface ICommand {
    Task<Result> ExecuteAsync();
  }
}