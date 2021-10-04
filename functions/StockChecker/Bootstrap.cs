using System;
using System.Collections.Generic;
using System.Text;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;

namespace StockChecker
{
    public static class Bootstrap
    {
        public static void Initialize(IServiceCollection services)
        {
            services.AddTransient<IAmazonDynamoDB, AmazonDynamoDBClient>();
        }
    }
}
