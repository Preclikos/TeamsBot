using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoDeployment.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BotCommand : Attribute
    {
        public string SubCommandName;
        public string Description;
        public bool InternalOnly;
        public BotCommand(string subCommandName, string description, bool internalOnly = false)
        {
            SubCommandName = subCommandName;
            Description = description;
            InternalOnly = internalOnly;
        }
    }
}
