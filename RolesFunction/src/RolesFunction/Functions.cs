using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace RolesFunction
{
    public class Functions
    {
        // This const is the name of the environment variable that the env file will use to set
        // the name of the DynamoDB table used to store role.
        const string TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP = "DYNAMODB_ROLE_TABLE_NAME";

        public const string ROLE_NAME_QUERY_STRING_NAME = "roleName";
        IDynamoDBContext DDBContext { get; set; }

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions() : this(null, null)
        {
        }

        /// <summary>
        /// Constructor used for testing passing in a preconfigured DynamoDB client.
        /// </summary>
        /// <param name="ddbClient"></param>
        /// <param name="tableName"></param>
        public Functions(IAmazonDynamoDB ddbClient, string tableName)
        {
            string table = null;
            if (!string.IsNullOrEmpty(tableName)) {
                table = tableName;
            } else {
                // Check to see if a table name was passed in through environment variables and if so 
                // add the table mapping.
                table = System.Environment.GetEnvironmentVariable(TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);
            }

            if (!string.IsNullOrEmpty(table)) {
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(Role)] = new Amazon.Util.TypeMapping(typeof(Role), table);
            }

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            this.DDBContext = new DynamoDBContext(ddbClient == null ? new AmazonDynamoDBClient() : ddbClient, config);
        }

        /// <summary>
        /// A Lambda function that returns back all roles.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The list of roles</returns>
        public async Task<APIGatewayProxyResponse> GetRolesAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Getting roles");
            var search = this.DDBContext.ScanAsync<Role>(null);
            var page = await search.GetNextSetAsync();
            context.Logger.LogLine($"Found {page.Count} roles");

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(page),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };

            return response;
        }

        /// <summary>
        /// A Lambda function that returns the role identified by role name.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> GetRolesByNameAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string roleName = null;
            if (request.PathParameters != null && request.PathParameters.ContainsKey(ROLE_NAME_QUERY_STRING_NAME)) {
                roleName = request.PathParameters[ROLE_NAME_QUERY_STRING_NAME];
            } else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(ROLE_NAME_QUERY_STRING_NAME)) {
                roleName = request.QueryStringParameters[ROLE_NAME_QUERY_STRING_NAME];
            }

            if (string.IsNullOrEmpty(roleName))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {ROLE_NAME_QUERY_STRING_NAME}"
                };
            }

            context.Logger.LogLine($"Getting role {roleName}");
            var roles = await DDBContext.QueryAsync<Role>(roleName).GetRemainingAsync();
            context.Logger.LogLine($"Found role: {roles != null}");

            if (roles == null)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.NotFound
                };
            }

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(roles),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
            return response;
        }

        /// <summary>
        /// A Lambda function that adds a role.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<APIGatewayProxyResponse> PostRoleAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var role = JsonConvert.DeserializeObject<Role>(request?.Body);
            role.CreatedTimestamp = DateTime.Now;
            role.UpdatedTimestamp = DateTime.Now;

            context.Logger.LogLine($"Saving role: Role name={role.Name}");
            await DDBContext.SaveAsync<Role>(role);

            string json = $"{{ Name: \"{role.Name}\" }}";
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Body = json,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
            return response;
        }

        /// <summary>
        /// A Lambda function that deletes a role from the DynamoDB table.
        /// </summary>
        /// <param name="request"></param>
        public async Task<APIGatewayProxyResponse> DeleteRoleAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string roleName = null;
            if (request.PathParameters != null && request.PathParameters.ContainsKey(ROLE_NAME_QUERY_STRING_NAME)) {
                roleName = request.PathParameters[ROLE_NAME_QUERY_STRING_NAME];
            } else if (request.QueryStringParameters != null && request.QueryStringParameters.ContainsKey(ROLE_NAME_QUERY_STRING_NAME)) {
                roleName = request.QueryStringParameters[ROLE_NAME_QUERY_STRING_NAME];
            }
            var param = JsonConvert.DeserializeObject<Dictionary<string, int>>(request?.Body);
            int type = -1;
            param.TryGetValue("Type", out type);

            if (string.IsNullOrEmpty(roleName) || type == -1)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {ROLE_NAME_QUERY_STRING_NAME}, {type}"
                };
            }

            context.Logger.LogLine($"Deleting role with name {roleName}, type {type}");
            await this.DDBContext.DeleteAsync<Role>(roleName, type);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK
            };
        }
    }
}
