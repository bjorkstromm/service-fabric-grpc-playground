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
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ServiceFabric.Grpc.Client
{
  /// <summary>
  /// An instance of this class is created for each service instance by the Service Fabric runtime.
  /// </summary>
  internal sealed class Client : StatelessService
  {
    public Client(StatelessServiceContext context)
        : base(context)
    { }

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
      var partition = await resolver.ResolveAsync(new Uri("fabric:/ServiceFabric.Grpc_Playground/Service"), ServicePartitionKey.Singleton, cancellationToken);
      var endpoint = JObject.Parse(partition.Endpoints.First().Address);
      var channel = new Channel(endpoint["Endpoints"].First().Values().First().ToString().Replace("http://", string.Empty), ChannelCredentials.Insecure);

      var client = new Greeter.GreeterClient(channel);

      while (!cancellationToken.IsCancellationRequested)
      {
        var reply = client.SayHello(new HelloRequest { Name = $"{++iterations}" });

        ServiceEventSource.Current.ServiceMessage(this.Context, "Client Received: {0}", reply.Message);

        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
      }

      await channel.ShutdownAsync();
    }
  }
}
