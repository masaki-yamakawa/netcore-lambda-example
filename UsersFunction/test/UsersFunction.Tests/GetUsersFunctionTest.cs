using System.Data;

using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;   
using Xunit;

namespace UsersFunction.Tests
{
    public class GetUsersFunctionTest
    {
        public GetUsersFunctionTest()
        {
            // TODO Init DataBase
        }

        [Fact]
        public void TestGetUsers()
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            GetUsersFunction function = new GetUsersFunction();

            request = new APIGatewayProxyRequest();
            context = new TestLambdaContext();
            response = function.GetUsers(request, context);
            Assert.Equal(200, response.StatusCode);

            JArray jsonArray = JArray.Parse(response.Body);
            Assert.Equal(3, jsonArray.Count);

            dynamic jsonObject1 = JObject.Parse(jsonArray[0].ToString());
            Assert.Equal("1234567", jsonObject1.userId.ToString());
            Assert.Equal("Yamada", jsonObject1.lastName.ToString());
            Assert.Equal("Taro", jsonObject1.firstName.ToString());
            Assert.Equal("1980/04/15 0:00:00", jsonObject1.birthday.ToString());
            Assert.NotEmpty(jsonObject1.createdAt.ToString());
            Assert.NotEmpty(jsonObject1.updatedAt.ToString());

            dynamic jsonObject2 = JObject.Parse(jsonArray[1].ToString());
            Assert.Equal("7654321", jsonObject2.userId.ToString());
            Assert.Equal("Tanaka", jsonObject2.lastName.ToString());
            Assert.Equal("Jiro", jsonObject2.firstName.ToString());
            Assert.Equal("2003/12/25 0:00:00", jsonObject2.birthday.ToString());
            Assert.NotEmpty(jsonObject2.createdAt.ToString());
            Assert.NotEmpty(jsonObject2.updatedAt.ToString());

            dynamic jsonObject3 = JObject.Parse(jsonArray[2].ToString());
            Assert.Equal("8888888", jsonObject3.userId.ToString());
            Assert.Equal("Kimura", jsonObject3.lastName.ToString());
            Assert.Equal("Hanako", jsonObject3.firstName.ToString());
            Assert.Equal("1978/03/31 0:00:00", jsonObject3.birthday.ToString());
            Assert.NotEmpty(jsonObject3.createdAt.ToString());
            Assert.NotEmpty(jsonObject3.updatedAt.ToString());
        }

        [Fact]
        public void TestGetUsersDB()
        {
            GetUsersFunction function = new GetUsersFunction();

            DataTable table = function.GetUsersDB();
            Assert.Equal(3, table.Rows.Count);

            DataRow row1 = table.Rows[0];
            Assert.Equal("1234567", row1["userId"].ToString());
            Assert.Equal("Yamada", row1["lastName"].ToString());
            Assert.Equal("Taro", row1["firstName"].ToString());
            Assert.Equal("1980/04/15 0:00:00", row1["birthday"].ToString());
            Assert.NotEmpty(row1["createdAt"].ToString());
            Assert.NotEmpty(row1["updatedAt"].ToString());

            DataRow row2 = table.Rows[1];
            Assert.Equal("7654321", row2["userId"].ToString());
            Assert.Equal("Tanaka", row2["lastName"].ToString());
            Assert.Equal("Jiro", row2["firstName"].ToString());
            Assert.Equal("2003/12/25 0:00:00", row2["birthday"].ToString());
            Assert.NotEmpty(row2["createdAt"].ToString());
            Assert.NotEmpty(row2["updatedAt"].ToString());

            DataRow row3 = table.Rows[2];
            Assert.Equal("8888888", row3["userId"].ToString());
            Assert.Equal("Kimura", row3["lastName"].ToString());
            Assert.Equal("Hanako", row3["firstName"].ToString());
            Assert.Equal("1978/03/31 0:00:00", row3["birthday"].ToString());
            Assert.NotEmpty(row3["createdAt"].ToString());
            Assert.NotEmpty(row3["updatedAt"].ToString());
        }
    }
}
