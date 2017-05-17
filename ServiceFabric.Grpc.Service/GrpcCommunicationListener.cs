using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Helloworld;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace ServiceFabric.Grpc.Service
{
  internal class GrpcCommunicationListener : ICommunicationListener
  {
    private readonly ServiceEventSource _eventSource;
    private readonly IEnumerable<ServerServiceDefinition> _services;
    private readonly ServiceContext _serviceContext;
    private readonly string _endpointName;

    private Server _server;

    public GrpcCommunicationListener(
      IEnumerable<ServerServiceDefinition> services,
      ServiceContext serviceContext,
      ServiceEventSource eventSource,
      string endpointName)
    {
      if (services == null)
      {
        throw new ArgumentNullException(nameof(services));
      }

      if (serviceContext == null)
      {
        throw new ArgumentNullException(nameof(serviceContext));
      }

      if (endpointName == null)
      {
        throw new ArgumentNullException(nameof(endpointName));
      }

      if (eventSource == null)
      {
        throw new ArgumentNullException(nameof(eventSource));
      }

      _services = services;
      _serviceContext = serviceContext;
      _endpointName = endpointName;
      _eventSource = eventSource;
    }

    public async Task<string> OpenAsync(CancellationToken cancellationToken)
    {
      var serviceEndpoint = _serviceContext.CodePackageActivationContext.GetEndpoint(_endpointName);
      var port = serviceEndpoint.Port;
      var host = FabricRuntime.GetNodeContext().IPAddressOrFQDN;

      try
      {
        _eventSource.ServiceMessage(_serviceContext, $"Starting gRPC server on http://{host}:{port}");

        _server = new Server
        {
          Ports = {new ServerPort(host, port, ServerCredentials.Insecure)}
        };
        foreach (var service in _services)
        {
          _server.Services.Add(service);
        }

        _server.Start();

        _eventSource.ServiceMessage(_serviceContext, $"Listening on http://{host}:{port}");

        return $"http://{host}:{port}";
      }
      catch (Exception ex)
      {
        _eventSource.ServiceMessage(_serviceContext, "gRPC server failed to open. " + ex);

        await StopServerAsync();

        throw;
      }
    }

    public Task CloseAsync(CancellationToken cancellationToken)
    {
      _eventSource.ServiceMessage(_serviceContext, "Closing gRPC server");

      return StopServerAsync();
    }

    public void Abort()
    {
      _eventSource.ServiceMessage(_serviceContext, "Aborting gRPC server");

      StopServerAsync().Wait();
    }

    private async Task StopServerAsync()
    {
      if (_server != null)
      {
        try
        {
          await _server.ShutdownAsync();
        }
        catch (Exception)
        {
          // no-op
        }
      }
    }
  }
}