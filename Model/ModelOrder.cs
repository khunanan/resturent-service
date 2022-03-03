using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResAPI.Model
{
    public class ModelOrder
    {
        public string Id { get; set; }
        public string NameTable { get; set; }
        public DateTime date { get; set; }
        public double price { get; set; }
        public List<ModelListmanu> listmanu { get; set; }
        public bool paid { get; set; }
        public string status { get; set; }

    }
}

