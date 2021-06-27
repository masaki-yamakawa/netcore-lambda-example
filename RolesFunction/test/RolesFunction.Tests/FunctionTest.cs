using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

using Newtonsoft.Json;

using Xunit;

namespace RolesFunction.Tests
{
    public class FunctionTest : IDisposable
    { 
        string TableName { get; }
        IAmazonDynamoDB DDBClient { get; }
        DynamoDBContext DDBContext { get; }
        IDictionary<string, Role> initRoles = new Dictionary<string, Role>();
        
        public FunctionTest()
        {
            this.TableName = "Role";
            var clientConfig = new AmazonDynamoDBConfig { ServiceURL = "http://localhost:8000" };
            this.DDBClient = new AmazonDynamoDBClient(clientConfig);
            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            this.DDBContext = new DynamoDBContext(DDBClient, config);

            SetupInitDataAsync().Wait();
        }

        [Theory]
        [InlineData("User1", 10)]
        [InlineData("User2", 11)]
        [InlineData("User3", 99)]
        public async Task TestPostRoleAsync(string roleName, int roleType)
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            Functions functions = new Functions(this.DDBClient, this.TableName);

            Role role = new Role();
            role.Name = roleName;
            role.Type = roleType;
            role.PrivilegeTypes = new List<string>(){"SELECT","INSERT","UPDATE","DELETE","TRUNCATE","REFERENCES","TRIGGER"};

            request = new APIGatewayProxyRequest
            {
                Body = JsonConvert.SerializeObject(role)
            };
            context = new TestLambdaContext();
            response = await functions.PostRoleAsync(request, context);
            Assert.Equal(200, response.StatusCode);

            Assert.Equal($"{{ Name: \"{role.Name}\" }}", response.Body);

            var retrieved = await DDBContext.LoadAsync<Role>(role.Name, role.Type);
            Assert.Equal(role.PrivilegeTypes, retrieved.PrivilegeTypes);
            Assert.Equal(retrieved.CreatedTimestamp, retrieved.UpdatedTimestamp);
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("Writer")]
        [InlineData("Reader")]
        public async Task TestGetRolesByNameAsync(string roleName)
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            Functions functions = new Functions(this.DDBClient, this.TableName);

            request = new APIGatewayProxyRequest
            {
                PathParameters = new Dictionary<string, string> { { Functions.ROLE_NAME_QUERY_STRING_NAME, roleName } }
            };
            context = new TestLambdaContext();
            response = await functions.GetRolesByNameAsync(request, context);
            Assert.Equal(200, response.StatusCode);

            List<Role> roles = JsonConvert.DeserializeObject<List<Role>>(response.Body);
            if ("Admin".Equals(roleName)) {
                Assert.Equal(2, roles.Count);
                Assert.Equal(initRoles["Admin-1"], roles[0], new RoleComparer());
                Assert.Equal(initRoles["Admin-2"], roles[1], new RoleComparer());
            } else if ("Writer".Equals(roleName)) {
                Assert.Single(roles);
                Assert.Equal(initRoles["Writer-3"], roles[0], new RoleComparer());
            } else if ("Reader".Equals(roleName)) {
                Assert.Single(roles);
                Assert.Equal(initRoles["Reader-4"], roles[0], new RoleComparer());
            }
        }

        [Fact]
        public async Task TestGetRolesAsync()
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            Functions functions = new Functions(this.DDBClient, this.TableName);

            request = new APIGatewayProxyRequest
            {
            };
            context = new TestLambdaContext();
            response = await functions.GetRolesAsync(request, context);
            Assert.Equal(200, response.StatusCode);

            List<Role> roles = JsonConvert.DeserializeObject<List<Role>>(response.Body);
			Assert.Equal(4, roles.Count);
            Assert.Contains(initRoles["Admin-1"], roles, new RoleComparer());
            Assert.Contains(initRoles["Admin-2"], roles, new RoleComparer());
            Assert.Contains(initRoles["Writer-3"], roles, new RoleComparer());
            Assert.Contains(initRoles["Reader-4"], roles, new RoleComparer());
        }

        [Theory]
        [InlineData("Admin", 1)]
        [InlineData("Admin", 2)]
        [InlineData("Writer", 3)]
        [InlineData("Reader", 4)]
        public async Task TestDeleteRoleAsync(string roleName, int roleType)
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            Functions functions = new Functions(this.DDBClient, this.TableName);

            IDictionary<string, int> param = new Dictionary<string, int>();
            param["Type"] = roleType;
            request = new APIGatewayProxyRequest
            {
                PathParameters = new Dictionary<string, string> { { Functions.ROLE_NAME_QUERY_STRING_NAME, roleName } },
                Body = JsonConvert.SerializeObject(param)
            };
            context = new TestLambdaContext();
            response = await functions.DeleteRoleAsync(request, context);
            Assert.Equal(200, response.StatusCode);


            var roles = await DDBContext.ScanAsync<Role>(null).GetRemainingAsync();
            Assert.Equal(3, roles.Count);
            Assert.DoesNotContain(initRoles[$"{roleName}-{roleType}"], roles, new RoleComparer());
        }

        /// <summary>
        /// Create the DynamoDB table for testing. This table is deleted as part of the object dispose method.
        /// </summary>
        /// <returns></returns>
        private async Task SetupInitDataAsync()
        {
            Role roleAdmin1 = new Role();
            roleAdmin1.Name = "Admin";
            roleAdmin1.Type = 1;
            roleAdmin1.PrivilegeTypes = new List<string>(){"SELECT","INSERT","UPDATE","DELETE","TRUNCATE","REFERENCES","TRIGGER"};
            initRoles[$"{roleAdmin1.Name}-{roleAdmin1.Type}"] = roleAdmin1;

            Role roleAdmin2 = new Role();
            roleAdmin2.Name = "Admin";
            roleAdmin2.Type = 2;
            roleAdmin2.PrivilegeTypes = new List<string>(){"SELECT","INSERT","UPDATE","DELETE"};
            initRoles[$"{roleAdmin2.Name}-{roleAdmin2.Type}"] = roleAdmin2;

            Role roleWriter1 = new Role();
            roleWriter1.Name = "Writer";
            roleWriter1.Type = 3;
            roleWriter1.PrivilegeTypes = new List<string>(){"INSERT","UPDATE","DELETE"};
            initRoles[$"{roleWriter1.Name}-{roleWriter1.Type}"] = roleWriter1;

            Role roleReader1 = new Role();
            roleReader1.Name = "Reader";
            roleReader1.Type = 4;
            roleReader1.PrivilegeTypes = new List<string>(){"SELECT"};
            initRoles[$"{roleReader1.Name}-{roleReader1.Type}"] = roleReader1;

            foreach (var role in initRoles.Values) {
                await DDBContext.SaveAsync<Role>(role);
            }

            var roles = await DDBContext.ScanAsync<Role>(null).GetRemainingAsync();
            Assert.Equal(initRoles.Count, roles.Count);
            foreach (var role in roles) {
                initRoles[$"{role.Name}-{role.Type}"] = role;
            }
            Assert.Equal(initRoles.Count, roles.Count);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    var roles = DDBContext.ScanAsync<Role>(null).GetRemainingAsync();
                    roles.Wait();
                    foreach (var role in roles.Result) {
                        DDBContext.DeleteAsync<Role>(role.Name, role.Type);
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    public class RoleComparer : IEqualityComparer<Role>
    {
        public bool Equals(Role x, Role y)
        {
            return x.Name.Equals(y.Name) &&
                x.Type.Equals(y.Type) &&
                x.PrivilegeTypes.SequenceEqual(y.PrivilegeTypes) &&
                x.CreatedTimestamp.Equals(y.CreatedTimestamp) &&
                x.UpdatedTimestamp.Equals(y.UpdatedTimestamp);
        }

        public int GetHashCode(Role role)
        {
            return role.Name.GetHashCode() ^ role.Type.GetHashCode() ^ role.PrivilegeTypes.GetHashCode() ^ role.CreatedTimestamp.GetHashCode() ^ role.UpdatedTimestamp.GetHashCode();
        }
    }
}
