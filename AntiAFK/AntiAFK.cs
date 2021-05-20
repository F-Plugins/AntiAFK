using AntiAFK.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OpenMod.API.Plugins;
using OpenMod.Core.Plugins;
using System;
using System.Threading.Tasks;

[assembly: PluginMetadata("Feli.AntiAFK", DisplayName = "AntiAFK", Website = "fplugins.com", Author = "Feli")]

namespace AntiAFK
{
    public class AntiAFK : OpenModUniversalPlugin
    {
        private readonly IAntiAFKService _antiAFKService;

        public AntiAFK(
            IAntiAFKService antiAFKService,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _antiAFKService = antiAFKService;
        }

        protected override async Task OnLoadAsync()
        {    
            await _antiAFKService.StartAsync();
        }

        protected override async Task OnUnloadAsync()
        {
            await _antiAFKService.StopAsync();
        }
    }
}
