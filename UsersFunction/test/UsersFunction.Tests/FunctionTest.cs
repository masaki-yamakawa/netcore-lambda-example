using Xunit;

using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;   

using UsersFunction;

namespace UsersFunction.Tests
{
    public class FunctionTest
    {
        public FunctionTest()
        {
        }

        [Fact]
        public void TestGetMethod()
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            Functions functions = new Functions();

            request = new APIGatewayProxyRequest();
            context = new TestLambdaContext();
            response = functions.Get(request, context);
            Assert.Equal(200, response.StatusCode);

            JArray jsonArray = JArray.Parse(response.Body);
            Assert.Equal(3, jsonArray.Count);

            dynamic jsonObject1 = JObject.Parse(jsonArray[0].ToString());
            Assert.Equal("1234567", jsonObject1.userId.ToString());
            Assert.Equal("Yamada", jsonObject1.lastName.ToString());
            Assert.Equal("Taro", jsonObject1.firstName.ToString());

            dynamic jsonObject2 = JObject.Parse(jsonArray[1].ToString());
            Assert.Equal("7654321", jsonObject2.userId.ToString());
            Assert.Equal("Tanaka", jsonObject2.lastName.ToString());
            Assert.Equal("Jiro", jsonObject2.firstName.ToString());

            dynamic jsonObject3 = JObject.Parse(jsonArray[2].ToString());
            Assert.Equal("8888888", jsonObject3.userId.ToString());
            Assert.Equal("Kimura", jsonObject3.lastName.ToString());
            Assert.Equal("Hanako", jsonObject3.firstName.ToString());
        }
    }
}
