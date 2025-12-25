using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

using Tyuiu.SivolapEM.Sprint7.Project.V5.Lib;

namespace Tyuiu.SivolapEM.Sprint7.Project.V5.Test
{
    [TestClass]
    public class DataServiceTest
    {
        [TestMethod]
        public void ValidCalcProfit()
        {
            DataService ds = new DataService();
            List<ItemModel> testData = new List<ItemModel>
            {
                new ItemModel
                {
                    Price = 200,
                    PurchasePrice = 150,
                    Quantity = 10
                },
                
                new ItemModel
                {
                    Price = 100,
                    PurchasePrice = 80,
                    Quantity = 5
                }
            };

            decimal res = ds.GetPotentialProfit(testData);

            Assert.AreEqual(600, res);
        }

        [TestMethod]
        public void ValidFilterByCategory()
        {
            DataService ds = new DataService();
            List<ItemModel> testData = new List<ItemModel>
            {
                new ItemModel { Name = "Дрель", Category = "Инструменты" },
                new ItemModel { Name = "Кирпич", Category = "Строительство" },
                new ItemModel { Name = "Молоток", Category = "Инструменты" }
            };

            var res = ds.FilterByCategory(testData, "Инструменты");

            Assert.AreEqual(2, res.Count);
            Assert.AreEqual("Дрель", res[0].Name);
        }

        [TestMethod]
        public void ValidSearch()
        {
            DataService ds = new DataService();
            List<ItemModel> testData = new List<ItemModel>
            {
                new ItemModel { Name = "Цемент М500", Code = "A001" },
                new ItemModel { Name = "Краска", Code = "B002" }
            };

            var res = ds.Search(testData, "цемент");

            Assert.AreEqual(1, res.Count);
            Assert.AreEqual("A001", res[0].Code);
        }
    }
}