using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace subscriber
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private const string GCP_PROJECT = "emulator";
        private const string GCP_SUBSCRIBER = "item-change-subscriber";
        private const string GCP_TOPIC = "item-change";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                if (CanConnectToPubSub())
                {
                    // Subscribe to the topic.
                    SubscriberServiceApiClient subscriber = CreateSubscriber();
                    var subscriptionName = new SubscriptionName(GCP_PROJECT,GCP_SUBSCRIBER);
                
                    PullResponse response = subscriber.Pull(subscriptionName, returnImmediately: true, maxMessages: 100);
                    foreach (ReceivedMessage received in response.ReceivedMessages)
                    {
                        PubsubMessage msg = received.Message;
                        _logger.LogInformation($"Received message {msg.MessageId} published at {msg.PublishTime.ToDateTime()}");
                        _logger.LogInformation($"Text: '{msg.Data.ToStringUtf8()}'");
                        
                    }

                    if (response.ReceivedMessages.Any())
                    {
                        subscriber.Acknowledge(subscriptionName, response.ReceivedMessages.Select(m => m.AckId));
                    }
                    
                }
                
                await Task.Delay(5000, stoppingToken);
            }
        }
        
        private SubscriberServiceApiClient CreateSubscriber()
        {
            var subscriberBuilder = new SubscriberServiceApiClientBuilder();
            subscriberBuilder.ChannelCredentials = ChannelCredentials.Insecure;
            subscriberBuilder.Endpoint = Environment.GetEnvironmentVariable("PUBSUB_EMULATOR_HOST");
            var subscriber = subscriberBuilder.Build();
            return subscriber;
        }
        
        private bool CanConnectToPubSub()
        {
            

            try
            {
                var subscriber = CreateSubscriber();
                SubscriptionName subscriptionName = new SubscriptionName(GCP_PROJECT, GCP_SUBSCRIBER);
                subscriber.CreateSubscription(subscriptionName,new TopicName(GCP_PROJECT, GCP_TOPIC), pushConfig: null, ackDeadlineSeconds: 60);
            }
            catch (RpcException e)
                when (e.Status.StatusCode == StatusCode.Unavailable)
            {
                _logger.LogWarning($"Unable to connect to pub sub");
                return false;
            }
            catch (RpcException e)
                when (e.Status.StatusCode == StatusCode.AlreadyExists)
            {
                return true;
            }
            
            return true;
        }
    }
}