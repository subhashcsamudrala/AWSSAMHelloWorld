using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace StockChecker
{
    public class StockEvent
    {
        public int stockPrice;
    }

    public class BookTable
    {
        public string bookId;
        public string quantity;
    }

    public class BookInventory
    {
        
        private static readonly Random rand = new Random((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        private readonly IAmazonDynamoDB _dynamoDB;
        //public BookInventory() : this(new AmazonDynamoDBClient()) { }
        public BookInventory()
        {
            var services = new ServiceCollection();
            Bootstrap.Initialize(services);
            var serviceProvider = services.BuildServiceProvider();
            this._dynamoDB = serviceProvider.GetService<IAmazonDynamoDB>();
        }

        public StockEvent FunctionHandler(ILambdaContext context)
        {
            // Sample Lambda function which mocks the operation of checking the current price
            // of a stock.

            // For demonstration purposes this Lambda function simply returns
            // a random integer between 0 and 100 as the stock price.

            // Parameters
            // ----------
            // context: ILambdaContext
            //     Lambda Context runtime methods and attributes

            // Returns
            // ------
            //     StockEvent: Object containing the current price of the stock

            return new StockEvent
            {
                stockPrice = rand.Next() % 100
            };
        }

        public async Task<Dictionary<string, AttributeValue>> CheckStockFunction(BookTable bookTable, ILambdaContext context)
        {
            //return new Dictionary<string, AttributeValue>{
            //    {
            //        "bookId", new AttributeValue { S = "1"}
            //    }
            //};
            try
            {
                context.Logger.LogLine($"one");
                string table = "bookTable";
                GetItemRequest getItemRequest = new GetItemRequest()
                {
                    TableName = table,
                    Key = new System.Collections.Generic.Dictionary<string, AttributeValue>
                    {
                        { "bookId", new AttributeValue { S = bookTable.bookId} }
                    }
                };

                //ScanRequest scanRequest = new ScanRequest();
                //scanRequest.TableName = table;
                //scanRequest.Select = Select.ALL_ATTRIBUTES;

                //var result = await _dynamoDB.ScanAsync(scanRequest);
                //Table books = Table.LoadTable(_dynamoDB, table);
                //var response = await books.GetItemAsync(bookTable.bookId).ConfigureAwait(false);


     

                //

                var request = new QueryRequest
                {
                    TableName = table,
                    KeyConditionExpression = "bookId = :v_Id",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":v_Id", new AttributeValue { S =  bookTable.bookId }}}
                };
                context.Logger.LogLine($"two");
                if (_dynamoDB == null)
                    throw new ArgumentNullException(nameof(_dynamoDB));
                else
                {
                    context.Logger.LogLine($"_dynamoDB: {_dynamoDB.ToString()}");
                    var response2 = await _dynamoDB.GetItemAsync(getItemRequest);
                    var item2 = response2.Item;
                    context.Logger.LogLine($"item2.Values.Count: {item2.Values.Count}");
                    Dictionary<string, AttributeValue>.ValueCollection.Enumerator items = item2.Values.GetEnumerator();
                    while (items.MoveNext())
                    {
                        // now empEnumerator.Current is the Employee instance without casting
                        var emp = items.Current;
                        string empName = emp.S;
                        context.Logger.LogLine($"empName: {empName}");
                    }
                    
                }
                var response = await _dynamoDB.QueryAsync(request);
                context.Logger.LogLine($"three");
                foreach (Dictionary<string, AttributeValue> item in response.Items)
                {
                    // Process the result.

                }
                var book = response.Items[0];
                if (IsBookAvailable(Convert.ToInt32(response.Items[0]["quantity"].S), Convert.ToInt32(bookTable.quantity)))
                {
                    context.Logger.LogLine($"four");
                    return book;
                }
                else
                {
                    context.Logger.LogLine($"five");
                    var bookOutOfStockError = new Exception("The book is out of stock");
                    bookOutOfStockError.Source = "BookOutOfStock";
                    throw bookOutOfStockError;
                }
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"Error occurred in CheckStockFunction. Error is: {ex.StackTrace}");
                if (ex.Source == "BookOutOfStock")
                {
                    throw ex;
                }
                else
                {
                    var bookNotFoundError = new Exception("BookNotFound");
                    bookNotFoundError.Source = "BookNotFound";
                    throw bookNotFoundError;
                }
            }
        }

        public bool IsBookAvailable(int availableQuantity, int requestedQuantity)
        {
            return availableQuantity - requestedQuantity > 0;
        }
    }
}
