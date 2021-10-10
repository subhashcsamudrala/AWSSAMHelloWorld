using System;
using System.Collections.Generic;
using System.Text;

namespace StockChecker
{
    public class ApplicationService
    {
        public string userId { get; set; }
        public BookTable bookTable { get; set; }
        public int Total { get; set; }
        public int price { get; set; }
        public bool redeem { get; set; }
    }
}
