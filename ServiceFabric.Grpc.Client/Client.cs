using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Helloworld;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceFabric.Services.Grpc.Communication.Client;

namespace ServiceFabric.Grpc.Client
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class Client : StatelessService
    {
        public Client(StatelessServiceContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            long iterations = 0;
            var resolver = ServicePartitionResolver.GetDefault();
            var serviceUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/Service");
            var communicationFactory = new GrpcCommunicationClientFactory<Greeter.GreeterClient>(null, resolver);

            var partitionClient = new ServicePartitionClient<GrpcCommunicationClient<Greeter.GreeterClient>>(communicationFactory, serviceUri, ServicePartitionKey.Singleton);

            while (!cancellationToken.IsCancellationRequested)
            {
                var reply = partitionClient.InvokeWithRetry((communicationClient) => communicationClient.Client.SayHello(new HelloRequest {Name = $"{++iterations}"}));

                ServiceEventSource.Current.ServiceMessage(this.Context, "Client Received: {0}", reply.Message);

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }
    }
}
