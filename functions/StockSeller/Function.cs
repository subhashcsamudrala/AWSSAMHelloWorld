using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StockChecker;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace StockSeller
{
    public class StockEvent
    {
        public int stockPrice;
    }

    public class TransactionResult
    {
        public string id;
        public string price;
        public string type;
        public string qty;
        public string timestamp;
    }

    public class Function
    {

        private static readonly Random rand = new Random((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        private readonly IAmazonDynamoDB _dynamoDB;
        private readonly IAmazonStepFunctions _amazonStepFunctions;
        public Function()
        {
            var services = new ServiceCollection();
            Bootstrap.Initialize(services);
            var serviceProvider = services.BuildServiceProvider();
            this._dynamoDB = serviceProvider.GetService<IAmazonDynamoDB>();
            this._amazonStepFunctions = serviceProvider.GetService<IAmazonStepFunctions>();
        }
        public TransactionResult FunctionHandler(StockEvent stockEvent, ILambdaContext context)
        {
            // Sample Lambda function which mocks the operation of selling a random number
            // of shares for a stock.

            // For demonstration purposes, this Lambda function does not actually perform any
            // actual transactions. It simply returns a mocked result.

            // Parameters
            // ----------
            // stockEvent: StockEvent, required
            //     Input event to the Lambda function

            // context: ILambdaContext
            //     Lambda Context runtime methods and attributes

            // Returns
            // ------
            //     TransactionResult: Object containing details of the stock selling transaction

            return new TransactionResult
            {
                id = rand.Next().ToString(),
                type = "Sell",
                price = stockEvent.stockPrice.ToString(),
                qty = (rand.Next() % 10 + 1).ToString(),
                timestamp = DateTime.Now.ToString("yyyyMMddHHmmssffff")
            };
        }

        public int CalculateTotalFunction(ApplicationService applicationService, ILambdaContext context)
        {
            var total = Convert.ToInt32(applicationService.bookTable.quantity) * Convert.ToInt32(applicationService.bookTable.price);
            return total;
        }

        public async Task<int> RedeemPointsFunction(ApplicationService applicationService, ILambdaContext context)
        {
            var orderTotal = applicationService.Total;
            try
            {
                context.Logger.LogLine($"applicationService.userId: {applicationService.userId}");
                string table = "userTable";
                var request = new QueryRequest
                {
                    TableName = table,
                    KeyConditionExpression = "userId = :v_Id",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":v_Id", new AttributeValue { S =  applicationService.userId }}}
                };

                if (_dynamoDB == null)
                    throw new ArgumentNullException(nameof(_dynamoDB));
                else
                {
                    context.Logger.LogLine($"one");
                    var response = await _dynamoDB.QueryAsync(request);
                    context.Logger.LogLine($"two");
                    if (response.Items == null || response.Items.Count < 1)
                    {
                        throw new Exception();
                    }
                    var userPoints = Convert.ToInt32(response.Items[0]["points"].S);
                    if (orderTotal > userPoints)
                    {
                        //await DeductPoints(applicationService.userId);
                        orderTotal = orderTotal - userPoints;
                    }
                    else
                    {
                        throw new Exception("Order total is less than redeem points");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.StackTrace);
            }
            return orderTotal;
        }

        private async Task<UpdateItemResponse> DeductPoints(string userId)
        {
            var request = new UpdateItemRequest
            {
                Key = new Dictionary<string, AttributeValue>() { { "userId", new AttributeValue { N = userId } } },

                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#P", "points"}
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                    {
                        {":newpoints",new AttributeValue {N = "0"}}
                    },
                UpdateExpression = "SET #P = :newprice",
                TableName = "userTable",
                ReturnValues = "ALL_NEW" // Return all the attributes of the updated item.
            };

            if (_dynamoDB == null)
                throw new ArgumentNullException(nameof(_dynamoDB));

            var response = await _dynamoDB.UpdateItemAsync(request);
            return response;

        }


        public string BillCustomerFunction(ApplicationService applicationService, ILambdaContext context)
        {
            return "Billing successful!";
        }

        public async Task<SendTaskSuccessResponse> SQSWorkerFunction(SQSEvent sqsevent, ILambdaContext context)
        {
            context.Logger.LogLine($"sqsevent: {JsonConvert.SerializeObject(sqsevent.Records.Count)}");
            context.Logger.LogLine($"sqsevent: {JsonConvert.SerializeObject(sqsevent.Records[0])}");
            var record = sqsevent.Records[0];
            var body = JsonConvert.DeserializeObject<CustomSQS>(record.Body);
            var courierEmail = "test@test.com";
            context.Logger.LogLine($"sqs applicationservice: {JsonConvert.SerializeObject(body.Input)}");
            context.Logger.LogLine($"sqs tasktoken: {JsonConvert.SerializeObject(body.TaskToken)}");
            //await updateBookQuantity(body.bookTable.bookId, body.bookTable.quantity);
            var sendTaskSuccessRequest = new SendTaskSuccessRequest
            {
                TaskToken = body.TaskToken,
                Output = JsonConvert.SerializeObject(courierEmail)
            };
            return await _amazonStepFunctions.SendTaskSuccessAsync(sendTaskSuccessRequest);
        }
    }
}
