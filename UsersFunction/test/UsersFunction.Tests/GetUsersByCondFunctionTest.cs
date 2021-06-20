using System.Collections.Generic;
using System.Data;

using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;   
using Xunit;

namespace UsersFunction.Tests
{
    public class GetUsersByCondFunctionTest
    {
        public GetUsersByCondFunctionTest()
        {
            // TODO Init DataBase
        }

        [Theory]
        [InlineData("1234567")]
        [InlineData("7654321")]
        [InlineData("8888888")]
        public void TestGetUsers(string userId)
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            GetUsersByCondFunction function = new GetUsersByCondFunction();

            request = new APIGatewayProxyRequest();
            request = new APIGatewayProxyRequest
            {
                PathParameters = new Dictionary<string, string> { { GetUsersByCondFunction.USER_ID_QUERY_STRING_NAME, userId } }
            };
            context = new TestLambdaContext();
            response = function.GetUsers(request, context);
            Assert.Equal(200, response.StatusCode);

            JArray jsonArray = JArray.Parse(response.Body);
            Assert.Equal(1, jsonArray.Count);

            dynamic jsonObject = JObject.Parse(jsonArray[0].ToString());
            if ("1234567".Equals(jsonObject.userId.ToString())) {
                Assert.Equal("1234567", jsonObject.userId.ToString());
                Assert.Equal("Yamada", jsonObject.lastName.ToString());
                Assert.Equal("Taro", jsonObject.firstName.ToString());
                Assert.Equal("1980/04/15 0:00:00", jsonObject.birthday.ToString());
                Assert.NotEmpty(jsonObject.createdAt.ToString());
                Assert.NotEmpty(jsonObject.updatedAt.ToString());
            } else if ("7654321".Equals(jsonObject.userId.ToString())) {
                Assert.Equal("7654321", jsonObject.userId.ToString());
                Assert.Equal("Tanaka", jsonObject.lastName.ToString());
                Assert.Equal("Jiro", jsonObject.firstName.ToString());
                Assert.Equal("2003/12/25 0:00:00", jsonObject.birthday.ToString());
                Assert.NotEmpty(jsonObject.createdAt.ToString());
                Assert.NotEmpty(jsonObject.updatedAt.ToString());
            } else if ("8888888".Equals(jsonObject.userId.ToString())) {
                Assert.Equal("8888888", jsonObject.userId.ToString());
                Assert.Equal("Kimura", jsonObject.lastName.ToString());
                Assert.Equal("Hanako", jsonObject.firstName.ToString());
                Assert.Equal("1978/03/31 0:00:00", jsonObject.birthday.ToString());
                Assert.NotEmpty(jsonObject.createdAt.ToString());
                Assert.NotEmpty(jsonObject.updatedAt.ToString());
            } else {
                Assert.True(false, "Unknown userId: " + userId);
            }
        }

        [Theory]
        [InlineData("1234567")]
        [InlineData("7654321")]
        [InlineData("8888888")]
        public void TestGetUsersDB(string userId)
        {
            GetUsersByCondFunction function = new GetUsersByCondFunction();

            DataTable table = function.GetUsersDBByCond(userId);
            Assert.Equal(1, table.Rows.Count);

            DataRow row = table.Rows[0];
            if ("1234567".Equals(row["userId"].ToString())) {
                Assert.Equal("1234567", row["userId"].ToString());
                Assert.Equal("Yamada", row["lastName"].ToString());
                Assert.Equal("Taro", row["firstName"].ToString());
                Assert.Equal("1980/04/15 0:00:00", row["birthday"].ToString());
                Assert.NotEmpty(row["createdAt"].ToString());
                Assert.NotEmpty(row["updatedAt"].ToString());
            } else if ("7654321".Equals(row["userId"].ToString())) {
                Assert.Equal("7654321", row["userId"].ToString());
                Assert.Equal("Tanaka", row["lastName"].ToString());
                Assert.Equal("Jiro", row["firstName"].ToString());
                Assert.Equal("2003/12/25 0:00:00", row["birthday"].ToString());
                Assert.NotEmpty(row["createdAt"].ToString());
                Assert.NotEmpty(row["updatedAt"].ToString());
            } else if ("8888888".Equals(row["userId"].ToString())) {
                Assert.Equal("8888888", row["userId"].ToString());
                Assert.Equal("Kimura", row["lastName"].ToString());
                Assert.Equal("Hanako", row["firstName"].ToString());
                Assert.Equal("1978/03/31 0:00:00", row["birthday"].ToString());
                Assert.NotEmpty(row["createdAt"].ToString());
                Assert.NotEmpty(row["updatedAt"].ToString());
            } else {
                Assert.True(false, "Unknown userId: " + userId);
            }
        }
    }
}
