using OpenMod.API.Ioc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AntiAFK.Services
{
    [Service]
    public interface IAntiAFKService
    {
        Task StartAsync();
        Task StopAsync();
    }
}
