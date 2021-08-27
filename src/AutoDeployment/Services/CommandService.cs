using AutoDeployment.Attributes;
using AutoDeployment.Interfaces;
using AutoDeployment.Models.CommandModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoDeployment.Services
{
    public class CommandService : ICommandService
    {
        public List<Command> Commands { get; set; }
        public CommandService()
        {
            Commands = new List<Command>();
            var classTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.GetCustomAttributes(typeof(BotService), true)
                .Count() > 0);

            foreach (var singleBotType in classTypes)
            {
                var commandName = GetCommandName(singleBotType);
                var newCommandDefinition = new Command(singleBotType, commandName);
                if (commandName != null)
                {
                    var commandMethods = singleBotType.GetMethods()
                        .Where(w => w.GetCustomAttributes(typeof(BotCommand), true)
                        .Count() > 0);

                    foreach (var singleCommandMethod in commandMethods)
                    {
                        var subCommand = GetSubCommandInfo(singleCommandMethod);
                        newCommandDefinition.SubCommands.Add(new SubCommand(singleCommandMethod, subCommand.Name, subCommand.Desription, subCommand.InternalOnly));
                    }
                }
                Commands.Add(newCommandDefinition);
            }
        }


        public (string Name, string Desription, bool InternalOnly) GetSubCommandInfo(MethodInfo type)
        {
            var dnAttribute = (BotCommand)type.GetCustomAttributes(typeof(BotCommand), true).FirstOrDefault();
            if (dnAttribute != null)
            {
                return (Name: dnAttribute.SubCommandName, Desription: dnAttribute.Description, InternalOnly: dnAttribute.InternalOnly);
            }
            return (null, null, true);
        }

        public string GetCommandName(Type type)
        {
            var dnAttribute = (BotService)type.GetCustomAttributes(typeof(BotService), true).FirstOrDefault();
            if (dnAttribute != null)
            {
                return dnAttribute.CommandName;
            }
            return null;
        }

        public bool CheckCommandExist(string commandName)
        {
            return Commands.Any(w => w.Name == commandName);
        }
        public IEnumerable<SubCommand> GetSubCommands(string commandName)
        {
            return Commands.Single(w => w.Name == commandName).SubCommands;
        }
    }
}
