using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoDeployment.Interfaces
{
    public interface ITokenStore
    {
        void SaveToken(string token);
        bool HasToken();
        string GetToken();
        void DeleteToken();
        IEnumerable<string> GetAllOtherTokens();
    }
}
