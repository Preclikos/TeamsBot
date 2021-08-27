using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoDeployment.Models
{
    public class ProactiveChannelModel
    {
        public ChannelAccount BotId { get; set; }
        public TeamsChannelData TeamsChannel { get; set; }
        public string ServiceUrl { get; set; }
        public string TenantId { get; set; }
    }
}
