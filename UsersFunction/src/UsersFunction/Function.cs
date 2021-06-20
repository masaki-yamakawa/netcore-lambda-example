using System;
using System.Collections.Generic;
using System.Data;
using System.Net;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using MySqlConnector;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace UsersFunction
{
    public class Functions
    {
        private static readonly string ConnString = Environment.GetEnvironmentVariable("DB_CONN_STR");

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
        }

        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Execute UserFunction.Get");
            string resultJson;
            using (var conn = new MySqlConnection(ConnString))
            {
                conn.Open();
                context.Logger.LogLine(String.Format("ConnectionString: {0}, State: {1}, DB ServerVersion: {2}", conn.ConnectionString, conn.State.ToString(), conn.ServerVersion));

                DataTable table = new DataTable();
                using(var command = conn.CreateCommand()) {
                    command.CommandText = $"SELECT * FROM User";
                    using(var reader = command.ExecuteReader()) {
                        table.Load(reader);
                    }
                }
                resultJson = JsonConvert.SerializeObject(table);
            }

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Body = resultJson,
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };

            return response;
        }
    }
}
