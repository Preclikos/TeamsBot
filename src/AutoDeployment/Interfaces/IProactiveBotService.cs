using AutoDeployment.Enums;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace AutoDeployment.Interfaces
{
    public interface IProactiveBotService
    {
        Task<string> SendMessage(Activity activityToSend, Channel.Name channelName, CancellationToken cancellationToken);
    }
}
