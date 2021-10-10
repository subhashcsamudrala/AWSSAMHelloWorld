using StockChecker;
using System;
using System.Collections.Generic;
using System.Text;

namespace StockSeller
{
    public class CustomSQS
    {
        public ApplicationService Input { get; set; }
        public string TaskToken { get; set; }
    }
}
