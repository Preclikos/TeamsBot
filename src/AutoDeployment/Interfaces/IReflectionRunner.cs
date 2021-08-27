using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoDeployment.Interfaces
{
    public interface IReflectionRunner
    {
        Task RunCommand(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string command, string subCommand, string uniqueMessageId = null, string[] textCommandAttributes = null);
        Task RunCommand(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string command, string subCommand, string uniqueMessageId = null, JObject textCommandAttributes = null);
    }
}
