using System;
using System.Collections.Generic;
using System.Text;
using Amazon.DynamoDBv2;
using Amazon.StepFunctions;
using Microsoft.Extensions.DependencyInjection;

namespace StockSeller
{
    public static class Bootstrap
    {
        public static void Initialize(IServiceCollection services)
        {
            services.AddTransient<IAmazonDynamoDB, AmazonDynamoDBClient>();
            services.AddTransient<IAmazonStepFunctions, AmazonStepFunctionsClient>();
        }
    }
}
