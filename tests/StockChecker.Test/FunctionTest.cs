using Amazon.DynamoDBv2;
using Amazon.Lambda.TestUtilities;
using Xunit;

namespace StockChecker.Tests
{
  public class FunctionTest
  {

    [Fact]
    public void TestStockCheckerFunctionHandler()
    {
            var context = new TestLambdaContext();
      
            var function = new BookInventory();
            var response = function.CheckStockFunction(new BookTable { bookId = "1", quantity = "4" }, context);

    }
  }
}