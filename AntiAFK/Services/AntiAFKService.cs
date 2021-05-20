using OpenMod.API.Ioc;
using OpenMod.API.Users;
using OpenMod.Core.Users;
using OpenMod.Extensions.Games.Abstractions.Players;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DiscordMessenger;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenMod.Core.Helpers;
using AntiAFK.Models;

namespace AntiAFK.Services
{
    [PluginServiceImplementation(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class AntiAFKService : IAntiAFKService
    {
        private bool _running = false;
        private List<PlayerData> _playersData = new List<PlayerData>();

        private readonly IUserManager _userManager;
        private readonly IConfiguration _configuration;

        public AntiAFKService(IUserManager userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public Task StartAsync()
        {
            _playersData = new List<PlayerData>();
            _running = true;
            AsyncHelper.Schedule("Check", () => Checking());
            return Task.CompletedTask;
        }
        
        private async Task Checking()
        {
            while (_running)
            {
                var getUsers = await _userManager.GetUsersAsync(KnownActorTypes.Player);

                foreach (var user in getUsers.Where(x => x.Session != null))
                {
                    var player = user as IPlayerUser;

                    var find = _playersData.FirstOrDefault(x => x.playerId == user.Id);

                    if (find != null)
                    {
                        if(find.position == player!.Player.Transform.Position)
                        {
                            Console.WriteLine(_configuration.GetSection("PunishmentConfiguration:BanPlayer").Get<bool>());

                            if (_configuration.GetSection("PunishmentConfiguration:BanPlayer").Get<bool>())
                            {
                                await _userManager.BanAsync(user, "You have been banned for been **AFK**", DateTime.Now.AddSeconds(_configuration.GetSection("PunishmentConfiguration:BanTime").Get<float>()));

                                if (_configuration.GetSection("WebHookConfiguration:UseWebHook").Get<bool>())
                                {
                                    var message = new DiscordMessage
                                    {
                                        username = "AntiAFK",
                                        embeds = new List<Embed>
                                        {
                                            new Embed
                                            {
                                                title = "AFK Player Detected",
                                                color = 332,
                                                fields = new List<Field>
                                                {
                                                    new Field
                                                    {
                                                        inline = true,
                                                        name = "Player Id",
                                                        value = user.Id
                                                    },
                                                    new Field
                                                    {
                                                        inline = true,
                                                        name = "Player Name",
                                                        value = user.DisplayName
                                                    },
                                                    new Field
                                                    {
                                                        inline = true,
                                                        name = "Player Address",
                                                        value = player!.Player.Address!.ToString()
                                                    },
                                                    new Field
                                                    {
                                                        inline = true,
                                                        name = "Punishment Type",
                                                        value = "BAN"
                                                    },
                                                    new Field
                                                    {
                                                        inline = true,
                                                        name = "Punishment Reason",
                                                        value = "You have been banned for been **AFK**"
                                                    },
                                                    new Field
                                                    {
                                                        inline = true,
                                                        name = "Punishment Expire Date",
                                                        value = DateTime.Now.AddSeconds(_configuration.GetSection("PunishmentConfiguration:BanTime").Get<float>()).ToString()
                                                    }
                                                }
                                            }
                                        }
                                    };

                                    await message.SendMessageAsync(_configuration.GetSection("WebHookConfiguration:WebHookURL").Get<string>());
                                }
                            }
                            else if(_configuration.GetSection("PunishmentConfiguration:KickPlayer").Get<bool>())
                            {
                                await _userManager.KickAsync(user, "You have been kicked for been **AFK**");

                                if (_configuration.GetSection("WebHookConfiguration:UseWebHook").Get<bool>())
                                {
                                    var message = new DiscordMessage
                                    {
                                        username = "AntiAFK",
                                        embeds = new List<Embed>
                                        {
                                            new Embed
                                            {
                                                title = "AFK Player Detected",
                                                color = 332,
                                                fields = new List<Field>
                                                {
                                                    new Field
                                                    {
                                                        inline = true,
                                                        name = "Player Id",
                                                        value = user.Id
                                                    },
                                                    new Field
                                                    {
                                                        inline = true,
                                                        name = "Player Name",
                                                        value = user.DisplayName
                                                    },
                                                    new Field
                                                    {
                                                        inline = true,
                                                        name = "Player Address",
                                                        value = player!.Player!.ToString()
                                                    },
                                                    new Field
                                                    {
                                                        inline = true,
                                                        name = "Punishment Type",
                                                        value = "Kick"
                                                    },
                                                    new Field
                                                    {
                                                        inline = true,
                                                        name = "Punishment Reason",
                                                        value ="You have been kicked for been **AFK**"
                                                    },
                                                }
                                            }
                                        }
                                    };

                                    await message.SendMessageAsync(_configuration.GetSection("WebHookConfiguration:WebHookURL").Get<string>());
                                }
                            }
                        }
                        else
                        {
                            find.position = player!.Player.Transform.Position;
                        }
                    }
                    else
                    {
                        _playersData.Add(new PlayerData
                        {
                            playerId = user.Id,
                            position = player!.Player.Transform.Position
                        });
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(_configuration.GetSection("CheckInterval").Get<double>()));
            }
        }

        public Task StopAsync()
        {
            _playersData = new List<PlayerData>();
            _running = false;
            return Task.CompletedTask;
        }
    }
}
