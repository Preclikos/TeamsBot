using AutoDeployment.BotServices;
using AutoDeployment.Interfaces;
using AutoDeployment.JobServices;
using AutoDeployment.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace AutoDeployment
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<GitLabOptions>(Configuration.GetSection("GitLabOptions"));
            services.Configure<ProactiveBotOptions>(Configuration.GetSection("ProactiveBotOptions"));

            services.AddMvc(options => options.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddLogging(configure => configure.AddConsole()).Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

            services.AddSingleton<ICommandService, CommandService>();
            services.AddSingleton<IObjectStorage, ObjectStorage>();

            services.AddHttpContextAccessor();
            services.AddScoped<IMessageInformationService, MessageInformationService>();
            services.AddScoped<IUserObjectStorage, ConversationObjectStorage>();
            services.AddScoped<ITokenStore, TokenStore>();

            services.AddSingleton<IFinanceGitLabService, FinanceGitLabService>();
            services.AddTransient<IFinanceBotGitLabService, FinanceBotGitLabService>();

            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, BotMainService>();

            services.AddSingleton<IProactiveBotService, ProactiveBotService>();

            //ReflectionRunner for CommandServices
            services.AddTransient<IReflectionRunner, ReflectionRunner>();

            //BotCommandService
            services.AddScoped<BotReleaseService>();
            services.AddScoped<BotTokenService>();
            services.AddScoped<BotMergeChecker>();
            services.AddScoped<BotChannelInfo>();

            //CronJobs
            services.AddCronJob<PipelineTracker>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = @"*/5 * * * * *";
            });

            services.AddCronJob<TagJobFinder>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = @"*/15 * * * * *";
            });

            services.AddCronJob<DailyFinisRelease>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = @"0 30 7 * * 1,3";
            });

            services.AddCronJob<FinisReleaseRandomUserSelect>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = @"0 30 8 * * 1,3";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            app.UseMvc();
        }
    }
}
