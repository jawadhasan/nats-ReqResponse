using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NATS.Client;
using NatsSampleApp.Common;

namespace NatsSampleApp.Worker
{
    public class Worker : BackgroundService
    {
       private readonly ILogger<Worker> _logger;
       private static IConnection _connection;
       private IAsyncSubscription _subscription;

       public Worker(ILogger<Worker> logger)
        {
            _logger = logger;

           Options opts = ConnectionFactory.GetDefaultOptions();
           opts.Url =CommonConfigs.MessageBrokerUrl;
           _connection = new ConnectionFactory().CreateConnection(opts);
        }
       protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
           
            _logger.LogInformation("Running the listener!");
            stoppingToken.ThrowIfCancellationRequested();


            //Wireup CommandHandler
            EventHandler<MsgHandlerEventArgs> eventHandler = (sender, args) =>
            {
                var job = JsonHelpers.Deserialize<Job>(args.Message.Data);
                var jobResult = ProcessJob(job);

                //Sending back the response
                var replySubject = args.Message.Reply;
                if (replySubject != null)
                {
                    var responseSerialized = JsonHelpers.Serialize(jobResult);
                    _connection.Publish(replySubject, responseSerialized);
                }
            };


            //Wireup Subscription
            _subscription = _connection.SubscribeAsync(CommonConfigs.JobWorkerSubject, eventHandler);
            Console.WriteLine("Listen to events...");

            return Task.CompletedTask;
        }
        
    
        private static JobResponse ProcessJob(Job job)
        {
            var sb = new StringBuilder();
            Console.WriteLine();
            Console.WriteLine($"Job {job.Id} Received:");
            

            //TODO command Processing
            sb.AppendLine($"Processing Job... {job.Id}");
            sb.AppendLine("Simulating Processing...");
            sb.AppendLine("ProcessingCompleted.");
            sb.AppendLine($"ACK for {job.Id}.");

            var jobResponse = new JobResponse();
            jobResponse.Id = Guid.NewGuid().ToString("N");
            jobResponse.Result = sb.ToString();
            jobResponse.IsSuccess = true;

            Console.WriteLine($"Job {job.Id} Processing Done.");
            return jobResponse;
        }



    }


  
}
