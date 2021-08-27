using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AutoDeployment.Services.MessageInformationService;

namespace AutoDeployment.Interfaces
{
    public interface IMessageInformationService
    {
        ConversationInfo ConversationContext { get; }
    }
}
