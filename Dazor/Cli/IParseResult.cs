using System.Threading.Tasks;

namespace Dazor.Cli {
  internal interface IParseResult {
     Task<Result> RunAsync();
  }
}