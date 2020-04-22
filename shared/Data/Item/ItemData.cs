using System;
using System.Collections.Generic;
using Dapper;
using shared.Factories;

namespace shared.Data.Item
{
    public interface IItemData
    {
        public IEnumerable<shared.Models.Item.Item> GetRecentlyUpdateItems();
        public void UpdateItem(int itemId);
    }

    public class ItemData : IItemData
    {
        private readonly IConnectionFactory _connectionFactory;

        public ItemData(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        public IEnumerable<shared.Models.Item.Item> GetRecentlyUpdateItems()
        {
            using var connection = _connectionFactory.GetConnection();

            var sql = "select * from item where updated is null";
            return connection.Query<Models.Item.Item>(sql);
        }

        public void UpdateItem(int itemId)
        {
            using var connection = _connectionFactory.GetConnection();

            var sql = "update item set updated = @Time where id = @Id";
            connection.Execute(sql, new {Time = DateTime.UtcNow,Id = itemId });
        }
    }
    
}