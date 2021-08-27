using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoDeployment.Interfaces
{
    public interface IJobService
    {
        bool Running { get; set; }
        DateTime NextRun { get; set; }
        Task DoWork();
    }
}
