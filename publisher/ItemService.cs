using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using shared.Data.Item;

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
            var items = _itemData.GetRecentlyUpdateItems();
            foreach (var item in items)
            {
                _logger.LogInformation(JsonSerializer.Serialize(item));
            }
        }

        private bool CanConnectToPubSub()
        {
            var publisherBuilder = new PublisherServiceApiClientBuilder();
            publisherBuilder.ChannelCredentials = ChannelCredentials.Insecure;
            publisherBuilder.Endpoint = Environment.GetEnvironmentVariable("PUBSUB_EMULATOR_HOST");
            var publisher = publisherBuilder.Build();

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
    }
}