using System;
using System.Fabric;
using Grpc.Core;
using Microsoft.ServiceFabric.Services.Communication.Client;

namespace ServiceFabric.Services.Grpc.Communication.Client
{
    public class GrpcCommunicationClient<TGrpcClient> : ICommunicationClient
        where TGrpcClient : ClientBase<TGrpcClient>
    {
        internal GrpcCommunicationClient(Channel channel)
        {
            Client = (TGrpcClient) Activator.CreateInstance(typeof(TGrpcClient), channel);
            Channel = channel;
        }

        public ResolvedServicePartition ResolvedServicePartition { get; set; }

        public string ListenerName { get; set; }

        public ResolvedServiceEndpoint Endpoint { get; set; }

        public TGrpcClient Client { get; }

        internal Channel Channel { get; }

        ~GrpcCommunicationClient()
        {
            Channel.ShutdownAsync().Wait();
        }
    }
}