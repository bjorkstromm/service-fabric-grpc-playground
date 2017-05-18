using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;

namespace ServiceFabric.Services.Grpc.Communication.Client
{
    public class GrpcCommunicationClientFactory<TGrpcClient> :
        CommunicationClientFactoryBase<GrpcCommunicationClient<TGrpcClient>>
        where TGrpcClient : ClientBase<TGrpcClient>
    {
        public GrpcCommunicationClientFactory(
            IEnumerable<IExceptionHandler> exceptionHandlers = null,
            IServicePartitionResolver servicePartitionResolver = null,
            string traceId = null)
            : base(servicePartitionResolver, GetExceptionHandlers(exceptionHandlers), traceId)
        {
        }

        protected virtual GrpcCommunicationClient<TGrpcClient> CreateGrpcCommunicationClient(Channel channel)
        {
            return new GrpcCommunicationClient<TGrpcClient>(channel);
        }

        protected override Task<GrpcCommunicationClient<TGrpcClient>> CreateClientAsync(
            string endpoint,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(CreateGrpcCommunicationClient(new Channel(endpoint.Replace("http://", string.Empty), ChannelCredentials.Insecure)));
        }

        protected override bool ValidateClient(GrpcCommunicationClient<TGrpcClient> client)
        {
            return (client.Channel.State == ChannelState.Ready);
        }

        protected override bool ValidateClient(string endpoint, GrpcCommunicationClient<TGrpcClient> client)
        {
            var channel = client.Channel;
            return channel.State == ChannelState.Ready && channel.Target.Equals(endpoint);
        }

        protected override void AbortClient(GrpcCommunicationClient<TGrpcClient> client)
        {
            client.Channel.ShutdownAsync().Wait();
        }

        private static IEnumerable<IExceptionHandler> GetExceptionHandlers(
            IEnumerable<IExceptionHandler> exceptionHandlers)
        {
            var handlers = new List<IExceptionHandler>();
            if (exceptionHandlers != null)
            {
                handlers.AddRange(exceptionHandlers);
            }
            // TODO: Create GrpcExceptionHandler
            return handlers;
        }
    }
}