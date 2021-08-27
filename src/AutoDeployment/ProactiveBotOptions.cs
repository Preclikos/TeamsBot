using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoDeployment
{
    public class ProactiveBotOptions
    {
        public string BotId { get; set; }
        public string BotName { get; set; }
        public string TeamId { get; set; }
        public string ServiceUrl { get; set; }
        public string TenantId { get; set; }
    }
}
