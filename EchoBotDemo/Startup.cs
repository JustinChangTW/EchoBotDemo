﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.11.1

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using EchoBotDemo.Bots;
using Microsoft.Bot.Schema;
using System.Collections.Concurrent;
using Microsoft.Bot.Builder.Azure;

namespace EchoBotDemo
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
            //儲存

            // Create the storage we'll be using for User and Conversation state.
            // (Memory is great for testing purposes - examples of implementing storage with
            // Azure Blob Storage or Cosmos DB are below).
            //var storage = new MemoryStorage();
            var storage = new CosmosDbPartitionedStorage(
                    new CosmosDbPartitionedStorageOptions
                    {
                        CosmosDbEndpoint = Configuration.GetValue<string>("CosmosDbEndpoint"),
                        AuthKey = Configuration.GetValue<string>("CosmosDbAuthKey"),
                        DatabaseId = Configuration.GetValue<string>("CosmosDbDatabaseId"),
                        ContainerId = Configuration.GetValue<string>("CosmosDbContainerId"),
                        CompatibilityMode = false,
                    });

            // Create the User state passing in the storage layer.
            var userState = new UserState(storage);
            services.AddSingleton(userState);

            // Create the Conversation state passing in the storage layer.
            var conversationState = new ConversationState(storage);
            services.AddSingleton(conversationState);

            services.AddSingleton<IStorage>(storage);

            services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();


            services.AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, EchoBot>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
