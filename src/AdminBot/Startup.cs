﻿using System;
using System.Threading.Tasks;
using AdminBot.Bot;
using AdminBot.Bot.Commands;
using AdminBot.Services.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot.Framework;

namespace AdminBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }

        public IHostingEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            if (Environment.IsDevelopment())
            {
                services.AddSingleton<IHostedService, BotService<Bot.AdminBot>>();
            }

            var botOptions = new BotOptions<Bot.AdminBot>();
            Configuration.GetSection("AdminBot").Bind(botOptions);

            services.AddTelegramBot(botOptions)
                .AddUpdateHandler<RateLimiterHandler>()
                .AddUpdateHandler<StartCommand>()
                .Configure();

            services.Configure<RateLimiterServiceOptions>(Configuration.GetSection("RateLimiter"));
            services.AddSingleton<RateLimiterService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        return Task.CompletedTask;
                    });
                });
            }

            if (Environment.IsDevelopment())
            {
                app.UseTelegramBotLongPolling<Bot.AdminBot>();
                Console.WriteLine($"Using long polling to get updates for {nameof(Bot.AdminBot)}");
            }
            else
            {
                Console.WriteLine($"Using webhook to get updates for {nameof(Bot.AdminBot)}");
                app.UseTelegramBotWebhook<Bot.AdminBot>();
            }
        }
    }
}