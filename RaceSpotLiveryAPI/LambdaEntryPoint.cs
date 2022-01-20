using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RaceSpotLiveryAPI
{
    /// <summary>
    /// This class extends from APIGatewayProxyFunction which contains the method FunctionHandlerAsync which is the 
    /// actual Lambda function entry point. The Lambda handler field should be set to
    /// 
    /// RaceSpotLiveryAPI::RaceSpotLiveryAPI.LambdaEntryPoint::FunctionHandlerAsync
    /// </summary>
    public class LambdaEntryPoint :

        // The base class must be set to match the AWS service invoking the Lambda function. If not Amazon.Lambda.AspNetCoreServer
        // will fail to convert the incoming request correctly into a valid ASP.NET Core request.
        //
        // API Gateway REST API                         -> Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
        // API Gateway HTTP API payload version 1.0     -> Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
        // API Gateway HTTP API payload version 2.0     -> Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction
        // Application Load Balancer                    -> Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction
        // 
        // Note: When using the AWS::Serverless::Function resource with an event type of "HttpApi" then payload version 2.0
        // will be the default and you must make Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction the base class.

        Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    {
        /// <summary>
        /// The builder has configuration, logging and Amazon API Gateway already configured. The startup class
        /// needs to be configured in this method using the UseStartup<>() method.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((c, b) => b.AddSystemsManager("/LiveryAPI"));
            builder.ConfigureLogging(logging =>
            {
                logging.AddAWSProvider();
                logging.SetMinimumLevel(LogLevel.Debug);
            });
            builder
                .UseStartup<Startup>();
        }

        /// <summary>
        /// Use this override to customize the services registered with the IHostBuilder. 
        /// 
        /// It is recommended not to call ConfigureWebHostDefaults to configure the IWebHostBuilder inside this method.
        /// Instead customize the IWebHostBuilder in the Init(IWebHostBuilder) overload.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IHostBuilder builder)
        {
        }
        
        public override async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request,
            ILambdaContext lambdaContext)
        {
            if (request.Resource == "WarmingLambda")
            {
                int.TryParse(request.Body, out var concurrencyCount);

                if (concurrencyCount > 1)
                {
                    Console.WriteLine($"Warming instance {concurrencyCount}.");
                    var client = new AmazonLambdaClient();
                    await client.InvokeAsync(new Amazon.Lambda.Model.InvokeRequest
                    {
                        FunctionName = lambdaContext.FunctionName,
                        InvocationType = InvocationType.RequestResponse,
                        Payload = JsonConvert.SerializeObject(new APIGatewayProxyRequest
                        {
                            Resource = request.Resource,
                            Body = (concurrencyCount - 1).ToString()
                        })
                    });
                }

                return new APIGatewayProxyResponse { };
            }

            return await base.FunctionHandlerAsync(request, lambdaContext);
        }
    }
}
