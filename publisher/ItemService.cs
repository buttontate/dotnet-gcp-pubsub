using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using shared.Data.Item;
using shared.Models.Item;

namespace publisher
{
    public interface IItemService
    {
        void PublishRecentlyUpdatedItems();
    }

    public class ItemService : IItemService
    {
        private readonly IItemData _itemData;
        private readonly ILogger<ItemService> _logger;
        private const string GCP_PROJECT = "emulator";
        private const string GCP_TOPIC = "item-change";

        public ItemService(IItemData itemData, ILogger<ItemService> logger)
        {
            _itemData = itemData;
            _logger = logger;
        }

        public void PublishRecentlyUpdatedItems()
        {
            if (!CanConnectToPubSub()) return;
            var items = _itemData.GetRecentlyUpdateItems().ToList();
            if (items.Any())
            {
                var messages = CreateMessages(items);
                PublishMessages(messages);
            }
        }

        private List<PubsubMessage> CreateMessages(List<Item> items)
        {
            return items.Select(CreateMessage).ToList();
        }

        private PubsubMessage CreateMessage(Item item)
        {
            var message = new PubsubMessage()
            {
                Data = ByteString.CopyFromUtf8(JsonSerializer.Serialize(item))
            };
            message.Attributes.Add("itemId", item.Id.ToString());
            return message;
        }

        private void PublishMessages(List<PubsubMessage> messages)
        {
            var topicName = new TopicName(GCP_PROJECT, GCP_TOPIC);
            var publisher = CreatePublisher();
            
            var response = publisher.Publish(topicName, messages);// Get the message ids GCloud gave us
            
            foreach (string messageId in response.MessageIds)
            {
                
                _logger.LogWarning($"Published message {messageId}");
            }
        }

        private bool CanConnectToPubSub()
        {
            var publisher = CreatePublisher();

            try
            {
                var topicName = new TopicName(GCP_PROJECT, GCP_TOPIC);
                publisher.CreateTopic(topicName);
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

        private PublisherServiceApiClient CreatePublisher()
        {
            var publisherBuilder = new PublisherServiceApiClientBuilder();
            publisherBuilder.ChannelCredentials = ChannelCredentials.Insecure;
            publisherBuilder.Endpoint = Environment.GetEnvironmentVariable("PUBSUB_EMULATOR_HOST");
            var publisher = publisherBuilder.Build();
            return publisher;
        }
    }
}