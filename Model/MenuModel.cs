using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResAPI.Model
{
    public class MenuModel
    {
        [BsonId]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public double Price { get; set; }
        public string Catagory { get; set; }
        public string DeletetionDate { get; set; }
    }
}
