using System;
using System.Collections.Generic;
using System.Data;
using System.Net;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using MySqlConnector;
using Newtonsoft.Json;

namespace UsersFunction
{
    public class GetUsersByCondFunction
    {
        public const string USER_ID_QUERY_STRING_NAME = "userId";
        private static readonly string ConnString = Environment.GetEnvironmentVariable("DB_CONN_STR");

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public GetUsersByCondFunction()
        {
        }

        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public APIGatewayProxyResponse GetUsers(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Execute GetUsersByCondFunction.GetUsers START");

            string userId = null;
            if (request.PathParameters != null && request.PathParameters.ContainsKey(USER_ID_QUERY_STRING_NAME))
                userId = request.PathParameters[USER_ID_QUERY_STRING_NAME];
            if (string.IsNullOrEmpty(userId))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.BadRequest,
                    Body = $"Missing required parameter {USER_ID_QUERY_STRING_NAME}"
                };
            }

            DataTable table = GetUsersDBByCond(userId);
            string resultJson = JsonConvert.SerializeObject(table);
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Body = resultJson,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };

            context.Logger.LogLine("Execute GetUsersByCondFunction.GetUsers END");
            return response;
        }

        internal DataTable GetUsersDBByCond(string userId)
        {
            DataTable table = new DataTable();
            using (var conn = new MySqlConnection(ConnString))
            {
                conn.Open();
                Console.WriteLine(String.Format("ConnectionString: {0}, State: {1}, DB ServerVersion: {2}", conn.ConnectionString, conn.State.ToString(), conn.ServerVersion));

                using(var command = conn.CreateCommand()) {
                    command.CommandText = string.Format("SELECT * FROM User WHERE userId='{0}' ORDER BY userId", userId);
                    using(var reader = command.ExecuteReader()) {
                        table.Load(reader);
                    }
                }
            }
            return table;
        }
    }
}
