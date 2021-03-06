﻿using System;
using System.Threading.Tasks;
using DataContract;
using Microsoft.Extensions.Hosting;
using NetRpc;
using NetRpc.Grpc;

namespace Service
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var o = new NGrpcServiceOptions();
            o.AddPort("0.0.0.0", 50001);
            var options = new MiddlewareOptions();
            options.UseCallbackThrottling(1000);
            var host = NManager.CreateHost(o, options, new ContractParam<IService, Service>());
            await host.RunAsync();
        }
    }

    internal class Service : IService
    {
        public async Task Call(Func<int, Task> cb)
        {
            for (var i = 0; i <= 20; i++)
            {
                await Task.Delay(100);
                try
                {
                    await cb.Invoke(i);
                    Console.WriteLine($"Send callback: {i}");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }
}