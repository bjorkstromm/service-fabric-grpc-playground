using System.Fabric;
using System.Threading.Tasks;
using Grpc.Core;
using Helloworld;

namespace ServiceFabric.Grpc.Service
{
  internal class GreeterService : Greeter.GreeterBase
  {
    private readonly ServiceContext _context;
    public GreeterService(ServiceContext context)
    {
      _context = context;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
      ServiceEventSource.Current.ServiceMessage(_context, "Server Received: {0}", request.Name);
      return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
    }
  }
}
