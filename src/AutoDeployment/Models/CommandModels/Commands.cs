using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoDeployment.Models.CommandModels
{
    public class Command
    {
        public Command(Type commandClass, string name)
        {
            Name = name;
            CommandClass = commandClass;
            SubCommands = new List<SubCommand>();
        }
        public string Name { get; set; }
        public Type CommandClass { get; set; }
        public List<SubCommand> SubCommands { get; set; }
    }
}
