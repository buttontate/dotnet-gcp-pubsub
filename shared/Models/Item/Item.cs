using System;

namespace shared.Models.Item
{
    public class Item
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public string Upc { get; set; }
        public string Description { get; set; }
    }
}