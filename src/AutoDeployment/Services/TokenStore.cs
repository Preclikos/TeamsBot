using AutoDeployment.Interfaces;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoDeployment.Services
{
    public class TokenStore : ITokenStore
    {

        private string UserId { get; set; }
        public TokenStore(IMessageInformationService messageInformation)
        {
            UserId = messageInformation.ConversationContext.From.Id;
        }

        public void SaveToken(string token)
        {
            using (StreamWriter w = File.AppendText("Tokens.txt"))
            {
                w.WriteLine(UserId + "-:-" + token);
            }
        }
        public IEnumerable<string> GetAllOtherTokens()
        {
            string line;
            List<string> otherTokens = new List<string>();
            try
            {
                StreamReader file = new StreamReader("Tokens.txt");
                while ((line = file.ReadLine()) != null)
                {
                    if (!String.IsNullOrEmpty(line))
                    {
                        var userAndTokens = line.Split("-:-", StringSplitOptions.RemoveEmptyEntries);
                        if (userAndTokens.Length == 2)
                        {
                            if (userAndTokens[0] != UserId)
                            {
                                otherTokens.Add(userAndTokens[1]);
                            }
                        }
                    }
                }
                file.Close();
            }
            catch
            {
            }
            return otherTokens;
        }

        public bool HasToken()
        {
            string line;
            try
            {
                StreamReader file = new StreamReader("Tokens.txt");
                while ((line = file.ReadLine()) != null)
                {
                    if (!String.IsNullOrEmpty(line))
                    {
                        var userAndTokens = line.Split("-:-", StringSplitOptions.RemoveEmptyEntries);
                        if (userAndTokens.Length == 2)
                        {
                            if (userAndTokens[0] == UserId)
                            {
                                return true;
                            }
                        }
                    }
                }
                file.Close();
                return false;
            }
            catch
            {
                return false;
            }

        }

        public string GetToken()
        {
            try
            {
                string line;
                StreamReader file = new StreamReader("Tokens.txt");
                while ((line = file.ReadLine()) != null)
                {
                    if (!String.IsNullOrEmpty(line))
                    {
                        var userAndTokens = line.Split("-:-", StringSplitOptions.RemoveEmptyEntries);
                        if (userAndTokens.Length == 2)
                        {
                            if (userAndTokens[0] == UserId)
                            {
                                return userAndTokens[1];
                            }
                        }
                    }
                }
                file.Close();
                return "";
            }
            catch
            {
                return "";
            }
        }

        public void DeleteToken()
        {
            var lines = File.ReadAllLines("Tokens.txt").Where(line => line.Split("-:-", StringSplitOptions.RemoveEmptyEntries)[0] != UserId).ToArray();
            File.WriteAllLines("Tokens.txt", lines);

        }
    }
}
