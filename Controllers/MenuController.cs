using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ResAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class MenuController : ControllerBase
    {
        MongoClient db;
        IMongoCollection<MenuModel> CollectionMenu;
        IMongoCollection<ModelOrder> CollectionOrder;
        public MenuController()
        {
            db = new MongoClient("mongodb://admin:12341234@cluster0-shard-00-00.p8pm8.mongodb.net:27017,cluster0-shard-00-01.p8pm8.mongodb.net:27017,cluster0-shard-00-02.p8pm8.mongodb.net:27017/myFirstDatabase?ssl=true&replicaSet=atlas-1084r9-shard-0&authSource=admin&retryWrites=true&w=majority");
            var Data = db.GetDatabase("ResDB");
            CollectionMenu = Data.GetCollection<MenuModel>("Menu");
            CollectionOrder = Data.GetCollection<ModelOrder>("Order");
        }

        [HttpGet]
        public IEnumerable<MenuModel> GetMenus()
        {
            var data = CollectionMenu.Find(x => true).ToList();
            return data;
        }

        [HttpGet]
        public IEnumerable<MenuModel> GetMenuscategory(string category)
        {
            var data = CollectionMenu.Find(x => x.Catagory == category).ToList();
            return data;
        }

        [HttpGet]
        public IEnumerable<MenuModel> GetMenusByCatagory(string Catagory)
        {
            var data = CollectionMenu.Find(x => x.Catagory == Catagory).ToList();
            return data;
        }

        [HttpGet("{id}")]
        public MenuModel GetMenus(string id)
        {
            var data = CollectionMenu.Find(x => x.Id == id).FirstOrDefault();
            return data;
        }

        [HttpPost]
        public bool CreateMenu([FromBody] MenuModel require)
        {
            require.Id = Guid.NewGuid().ToString();
            CollectionMenu.InsertOne(require);
            return true;
        }

        [HttpPost]
        public bool EditeMenu([FromBody] MenuModel require)
        {
            var update = Builders<MenuModel>.Update
                .Set(x => x.Image, require.Image)
                .Set(x => x.Name, require.Name)
                .Set(x => x.Price, require.Price)
                .Set(x => x.Catagory, require.Catagory);
            CollectionMenu.UpdateOne(x => x.Id == require.Id, update);
            return true;
        }

        [HttpDelete]
        public bool DeleteMenu(string id)
        {
            CollectionMenu.DeleteOne(it => it.Id == id);
            return true;
        }

        [HttpDelete]
        public bool DeleteMenu2(string id)
        {
            Builders<MenuModel>.Update
                .Set(x => x.DeletetionDate, DateTime.UtcNow.ToString());
            CollectionMenu.UpdateOne(it => it.Id == id, null);
            return true;
        }

        [HttpPost]
        public bool AddMenuToCart([FromBody] ModelListmanu require, string table)
        {
            var str = CollectionOrder.Find(it => it.NameTable == table && it.paid == false && it.status == "").FirstOrDefault();
            var c1 = str != null ? str.listmanu.Count() + 1 : 0 + 1;
            if (str == null)
            {
                var list = new List<ModelListmanu>();
                require.NameManu.Id = c1.ToString();
                list.Add(require);

                var pp = require.tatol * require.NameManu.Price;
                var order = new ModelOrder
                {
                    Id = Guid.NewGuid().ToString(),
                    date = DateTime.UtcNow,
                    listmanu = list,
                    NameTable = table,
                    paid = false,
                    price = pp,
                    status = ""
                };
                CollectionOrder.InsertOne(order);
            }
            else
            {
                var pp = require.tatol * require.NameManu.Price;
                var pd = str.price + pp;
                var usd = str.listmanu;
                require.NameManu.Id = c1.ToString();
                usd.Add(require);
                var updateorder = Builders<ModelOrder>.Update
                    .Set(x => x.listmanu, usd)
                    .Set(it => it.price, pd);
                CollectionOrder.UpdateOne(it => it.Id == str.Id, updateorder);
            }
            return true;
        }

        [HttpPost]
        public bool ConfirmOrder(string id)
        {
            var str = CollectionOrder.Find(it => it.Id == id && it.paid == false).FirstOrDefault();
            if (str != null)
            {
                var status = Builders<ModelOrder>.Update
                                    .Set(it => it.status, "รอรับออเดอร์");
                CollectionOrder.UpdateOne(it => it.Id == id, status);
            }
            return true;
        }

        [HttpDelete("{id}/{menuid}")]
        public bool Deletemanuorder(string id, string menuid)
        {
            var pp = 0.0;
            var str = CollectionOrder.Find(it => it.Id == id && it.paid == false && it.status == "").FirstOrDefault();
            var lst = str.listmanu;
            var data = lst.FirstOrDefault(it => it.NameManu.Id == menuid);
            pp = (str.price - (data.tatol * data.NameManu.Price));
            var ind = lst.FindIndex(it => it.NameManu.Id == menuid);
            lst.RemoveAt(ind);

            var update = Builders<ModelOrder>.Update
                                   .Set(it => it.listmanu, lst)
                                   .Set(it => it.price, pp);
            CollectionOrder.UpdateOne(it => it.Id == str.Id, update);
            return true;
        }

        [HttpGet("{id}")]
        public ModelListmanu GetMenusOrder(string id, string idmenu)
        {
            var data = CollectionOrder.Find(x => x.Id == id).FirstOrDefault();
            var menudata = data.listmanu.Find(x => x.NameManu.Id == idmenu);
            return menudata;
        }

        [HttpPost]
        public bool ChangeTotalOrder([FromBody] ModelListmanu require, string Id)
        {
            var str = CollectionOrder.Find(it => it.Id == Id && it.paid == false).FirstOrDefault();
            var lst = str.listmanu;
            var ind = lst.FindIndex(it => it.NameManu.Id == require.NameManu.Id);
            lst.RemoveAt(ind);
            lst.Add(require);
            var updateorder = Builders<ModelOrder>.Update
                                    .Set(it => it.listmanu, lst);
            CollectionOrder.UpdateOne(it => it.Id == str.Id, updateorder);
            return true;
        }

        [HttpGet]
        public IEnumerable<ModelOrder> Getorderappuve()
        {
            var data = CollectionOrder.Find(x => x.status == "รอรับออเดอร์").ToList();
            var list = data.OrderByDescending(x => x.date).ToArray();
            Array.Reverse(list);
            return list;
        }

        [HttpGet("{id}")]
        public ModelOrder GetMenusOrderappuve(string id)
        {
            var data = CollectionOrder.Find(x => x.Id == id).FirstOrDefault();
            return data;
        }

        [HttpPost]
        public bool UpdateStatus(string id)
        {
            var str = CollectionOrder.Find(it => it.Id == id && it.paid == false).FirstOrDefault();
            if (str != null)
            {
                var status = Builders<ModelOrder>.Update
                                    .Set(it => it.status, "รับออเดอร์แล้ว");
                CollectionOrder.UpdateOne(it => it.Id == str.Id, status);
            }
            return true;
        }

        [HttpPost]
        public bool UpdateStatusEnd(string id)
        {
            var str = CollectionOrder.Find(it => it.Id == id && it.paid == false).FirstOrDefault();
            if (str != null)
            {
                var status = Builders<ModelOrder>.Update
                                               .Set(it => it.status, "ออเดอร์พร้อมเสิร์ฟแล้ว");
                CollectionOrder.UpdateOne(it => it.Id == str.Id, status);
            }
            return true;
        }

        [HttpPost]
        public bool UpdateStatusCancel(string id)
        {
            var str = CollectionOrder.Find(it => it.Id == id && it.paid == false).FirstOrDefault();
            if (str != null)
            {
                var status = Builders<ModelOrder>.Update
                                               .Set(it => it.status, "ออเดอร์ถูกยกเลิก");
                CollectionOrder.UpdateOne(it => it.Id == str.Id, status);
            }
            return true;
        }

        [HttpPost]
        public bool UpdateStatusPayment(string table)
        {
            var str = CollectionOrder.Find(it => it.NameTable == table && it.paid == false).ToList();
            foreach (var item in str)
            {
                var status = Builders<ModelOrder>.Update
                     .Set(it => it.status, "ชำระเงินสำเร็จ")
                     .Set(it => it.paid, true);
                CollectionOrder.UpdateOne(it => it.Id == item.Id, status);
            }

            return true;
        }

        [HttpGet]
        public ModelOrder Getorderbasket(string tabel)
        {
            var data = CollectionOrder.Find(x => x.NameTable == tabel && x.paid == false && x.status == "").FirstOrDefault();
            return data;
        }

        [HttpGet]
        public ModelOrder Getordertable(string table)
        {
            var order = CollectionOrder.Find(x => x.NameTable == table && x.paid == false).ToList();
            var allManu = new List<ModelListmanu>();
            var price = 0.0;
            var status = "";
            foreach (var lMenu in order)
            {
                status = lMenu.status;
                foreach (var menu in lMenu.listmanu)
                {
                    allManu.Add(menu);
                    price += ((double)menu.tatol * menu.NameManu.Price);
                }
            }
            var newOrder = new ModelOrder
            {
                Id = "",
                date = DateTime.UtcNow,
                listmanu = allManu,
                NameTable = table,
                paid = false,
                status = status,
                price = price
            };
            return newOrder;
        }

        [HttpGet("{id}")]
        public ModelOrder GetMenusOrderbyid(string id)
        {
            var data = CollectionOrder.Find(x => x.Id == id).FirstOrDefault();
            return data;
        }

        [HttpGet]
        public List<respon> Getorderhistory()
        {
            var result = new respon();
            var data = CollectionOrder.Find(x => x.paid == false && x.status == "เสิร์ฟแล้ว").ToList();
            var tt = data.GroupBy(it => it.NameTable).Select(g => new respon
            {
                nameTable = g.Key
            }).ToList();
            return tt;
        }

        public class respon
        {
            public string nameTable { get; set; }
        }

        [HttpGet]
        public IEnumerable<ModelOrder> GetOrdersReady()
        {
            var data = CollectionOrder.Find(x => x.status == "ออเดอร์พร้อมเสิร์ฟแล้ว").ToList();
            var list = data.OrderByDescending(x => x.date).ToArray();
            Array.Reverse(list);
            return list;
        }

        [HttpPost]
        public bool UpdateStatusOrderReady(string id)
        {
            var str = CollectionOrder.Find(it => it.Id == id && it.paid == false).FirstOrDefault();
            if (str != null)
            {
                var status = Builders<ModelOrder>.Update
                                    .Set(it => it.status, "เสิร์ฟแล้ว");
                CollectionOrder.UpdateOne(it => it.Id == str.Id, status);
            }
            return true;
        }

        [HttpGet]
        public ModelReprot GetdataReprot(DateTime date)
        {
            var price = 0.0;
            var allManu = new List<ModelListmanu>();

            var data = CollectionOrder.Find(it => it.paid == true).ToList();
            var newdata = data.Where(it => it.date.Date == date.Date).ToList();
            foreach (var item in newdata)
            {
                foreach (var datas in item.listmanu)
                {
                    allManu.Add(datas);
                    price += ((double)datas.tatol * datas.NameManu.Price);
                }
            }

            var groupdata = allManu.GroupBy(it => it.NameManu.Name).Select(g => new ModelData
            {
                namemenu = g.Key,
                price = g.Sum(x => x.NameManu.Price * x.tatol),
                tatol = g.Sum(x => x.tatol)
            }).ToList();

            var DataReprot = new ModelReprot
            {
                listdata = groupdata,
                result = price
            };

            return DataReprot;
        }

        public class ModelReprot
        {
            public List<ModelData> listdata { get; set; }
            public double result { get; set; }
        }

        public class ModelData
        {
            public string namemenu { get; set; }
            public double price { get; set; }
            public int tatol { get; set; }
        }
    }
}
