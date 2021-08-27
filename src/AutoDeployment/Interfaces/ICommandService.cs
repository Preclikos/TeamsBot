using AutoDeployment.Models.CommandModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoDeployment.Interfaces
{
    public interface ICommandService
    {
        List<Command> Commands { get; set; }
        bool CheckCommandExist(string commandName);
        IEnumerable<SubCommand> GetSubCommands(string commandName);
    }
}