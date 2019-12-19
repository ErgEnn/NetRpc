﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Base;

namespace NetRpc.RabbitMQ
{
    public sealed class RabbitMQServiceProxy : IHostedService
    {
        private RequestHandler _requestHandler;
        private Service _service;
        private volatile int _handlingCount;
        private readonly ILogger _logger;

        public RabbitMQServiceProxy(IOptionsMonitor<RabbitMqServiceOptions> mqOptions, IServiceProvider serviceProvider, ILoggerFactory factory)
        {
            _logger = factory.CreateLogger("NetRpc");
            Reset(mqOptions.CurrentValue, serviceProvider);
            mqOptions.OnChange(i =>
            {
                Reset(i, serviceProvider);
                _service.Open();
            });
        }

        private void Reset(MQOptions opt, IServiceProvider serviceProvider)
        {
            if (_service != null)
            {
                _service.Dispose();
                _service.Received -= ServiceReceived;
            }

            _service = new Service(opt.CreateConnectionFactory(), opt.RpcQueue, opt.PrefetchCount, _logger);
            _requestHandler = new RequestHandler(serviceProvider, ChannelType.RabbitMQ);
            _service.Received += ServiceReceived;
        }

        private async void ServiceReceived(object sender, global::RabbitMQ.Base.EventArgsT<CallSession> e)
        {
            Interlocked.Increment(ref _handlingCount);
            try
            {
#if NETSTANDARD2_1 || NETCOREAPP3_1
                await
#endif
                using var connection = new RabbitMQServiceConnection(e.Value);
                await _requestHandler.HandleAsync(connection);
            }
            finally
            {
                Interlocked.Decrement(ref _handlingCount);
            }
        }

        private bool IsHanding => _handlingCount > 0;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _service.Open();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _service?.Dispose();

            return Task.Run(() =>
            {
                while (IsHanding)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    Thread.Sleep(1000);
                }
            }, cancellationToken);
        }
    }
}