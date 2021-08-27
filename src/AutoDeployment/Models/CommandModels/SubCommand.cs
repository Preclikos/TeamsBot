using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AutoDeployment.Models.CommandModels
{
    public class SubCommand
    {
        public SubCommand(MethodInfo subCommandMethod,string name, string description, bool internalOnly)
        {
            Name = name;
            SubCommandMethod = subCommandMethod;
            Description = description;
            InternalOnly = internalOnly;
        }
        public string Name { get; set; }
        public MethodInfo SubCommandMethod { get; set; }
        public string Description { get; set; }
        public bool InternalOnly { get; set; }
    }
}
