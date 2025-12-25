using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tyuiu.SivolapEM.Sprint7.Project.V5.Lib
{
    public class ItemModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal PurchasePrice { get; set; }
        public string Description { get; set; }
        public string Supplier { get; set; }
        public string DeliveryDate { get; set; }
    }

    public class DataService
    {
        public void WriteLog(string action)
        {
            try
            {
                string path = "history.log";
                string logLine = $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}] {action}";
                File.AppendAllText(path, logLine + Environment.NewLine, Encoding.UTF8);
            }
            catch { }
        }

        public List<ItemModel> LoadData(string path)
        {
            var list = new List<ItemModel>();
            if (!File.Exists(path)) return list;

            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            foreach (var line in lines)
            {
                var parts = line.Split(';');
                if (parts.Length >= 9)
                {
                    int.TryParse(parts[3], out int qty);
                    decimal.TryParse(parts[4].Replace('.', ','), out decimal price);
                    decimal.TryParse(parts[5].Replace('.', ','), out decimal purchPrice);

                    list.Add(new ItemModel
                    {
                        Code = parts[0],
                        Name = parts[1],
                        Category = parts[2],
                        Quantity = qty,
                        Price = price,
                        PurchasePrice = purchPrice,
                        Description = parts[6],
                        Supplier = parts[7],
                        DeliveryDate = parts[8]
                    });
                }
            }
            return list;
        }

        public void SaveData(string path, List<ItemModel> data)
        {
            var sb = new StringBuilder();
            foreach (var item in data)
            {
                sb.AppendLine($"{item.Code};{item.Name};{item.Category};{item.Quantity};{item.Price};{item.PurchasePrice};{item.Description};{item.Supplier};{item.DeliveryDate}");
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        public void AddItem(List<ItemModel> data, ItemModel newItem)
        {
            data.Add(newItem);
        }

        public void RemoveItem(List<ItemModel> data, ItemModel item)
        {
            data.Remove(item);
        }

        public List<ItemModel> Search(List<ItemModel> data, string query)
        {
            query = query.ToLower();
            return data.Where(x =>
                (x.Name != null && x.Name.ToLower().Contains(query)) ||
                (x.Code != null && x.Code.ToLower().Contains(query))
            ).ToList();
        }

        public List<ItemModel> FilterByCategory(List<ItemModel> data, string category)
        {
            if (category == "Все") return data;
            return data.Where(x => x.Category == category).ToList();
        }

        public int GetTotalQuantity(List<ItemModel> data) => data.Sum(x => x.Quantity);

        public decimal GetTotalStockValue(List<ItemModel> data) => data.Sum(x => x.Price * x.Quantity);

        public decimal GetAveragePrice(List<ItemModel> data) => data.Count > 0 ? data.Average(x => x.Price) : 0;

        public decimal GetMaxPrice(List<ItemModel> data) => data.Count > 0 ? data.Max(x => x.Price) : 0;

        public decimal GetMinPrice(List<ItemModel> data) => data.Count > 0 ? data.Min(x => x.Price) : 0;

        public decimal GetPotentialProfit(List<ItemModel> data)
        {
            return data.Sum(x => (x.Price - x.PurchasePrice) * x.Quantity);
        }
    }
}