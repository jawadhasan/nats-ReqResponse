using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NATS.Client;
using NatsSampleApp.Common;
using ModbusPOC.CommandMessages;
using System.Diagnostics;

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
            opts.Url = CommonConfigs.MessageBrokerUrl;
            _connection = new ConnectionFactory().CreateConnection(opts);
        }
        //protected override Task ExecuteAsync(CancellationToken stoppingToken)
        // {

        //     _logger.LogInformation("Running the listener!");
        //     stoppingToken.ThrowIfCancellationRequested();


        //     //Wireup CommandHandler
        //     EventHandler<MsgHandlerEventArgs> eventHandler = (sender, args) =>
        //     {
        //         var job = JsonHelpers.Deserialize<Job>(args.Message.Data);
        //         var jobResult = ProcessJob(job);

        //         //Sending back the response
        //         var replySubject = args.Message.Reply;
        //         if (replySubject != null)
        //         {
        //             var responseSerialized = JsonHelpers.Serialize(jobResult);
        //             _connection.Publish(replySubject, responseSerialized);
        //         }
        //     };


        //     //Wireup Subscription
        //     _subscription = _connection.SubscribeAsync(CommonConfigs.JobWorkerSubject, eventHandler);
        //     Console.WriteLine("Listen to events...");

        //     return Task.CompletedTask;
        // }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            try
            {
                _logger.LogInformation("Communication Worker - Running the listener!");
                stoppingToken.ThrowIfCancellationRequested();

                var commandsSubject = CommonConfigs.JobWorkerSubject;//_Options.Value.CommunicationCommandSubject;
                _subscription = _connection.SubscribeAsync(commandsSubject, HandleSubscription);

                _logger.LogInformation("Communication Worker - Listen to events...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return Task.CompletedTask;
        }

      

        private void HandleSubscription(object sender, MsgHandlerEventArgs e)
        {
            var cancelToken = new CancellationTokenSource();
            Task.Factory.StartNew(() => {

                cancelToken.Token.ThrowIfCancellationRequested();

                //RunCommand(e);
                var result = Work(e,cancelToken.Token);

            }, cancelToken.Token);




            //TODO: this stops the Task conditionally:
            Thread.Sleep(5000); //for testing/simulation
            cancelToken.Cancel(true);
        }





        private void RunCommand(MsgHandlerEventArgs e)
        {

            _logger.LogInformation("-------------------");
            var job = JsonHelpers.Deserialize<PrinterCommand>(e.Message.Data);//Message
            _logger.LogInformation($"Command {job.CommandType} ({job.CommandId}) source[{job.Source}]  Recieved. for Printer ({job.PrinterName}) IP {job.IpAddress}:{job.Port}");

            var sw = Stopwatch.StartNew();
            //var command = CommandBuilder.BuildCommand(job);

            //Pre-check: if device is set to Modbus etc.....here
            var operationResult = ProcessJob(job); //_dispatcher.Dispatch(command); //execute

            sw.Stop();
            var elapsedms = sw.ElapsedMilliseconds;           
            _logger.LogInformation($"command {job.CommandType} ({job.CommandId}) source[{job.Source}] completed! IsSuccess: {operationResult.IsSuccess}, took {elapsedms} ms.");


            //Send Response to caller
            SendReply(e.Message.Reply, operationResult);


        }


        private static void SendReply(string replySubject, object operationResult)
        {
            if (replySubject != null)
            {
                var responseSerialized = JsonHelpers.Serialize(operationResult);
                _connection.Publish(replySubject, responseSerialized);
            }
        }

        private static JobResponse ProcessJob(PrinterCommand job)
        {
            var sb = new StringBuilder();
            Console.WriteLine();
            Console.WriteLine($"Job {job.CommandId} Received:");


            //TODO command Processing
            sb.AppendLine($"Processing Job... {job.CommandId}");
            sb.AppendLine("Simulating Processing...");
            sb.AppendLine("ProcessingCompleted.");
            sb.AppendLine($"ACK for {job.CommandId}.");

            var jobResponse = new JobResponse();
            jobResponse.Id = Guid.NewGuid().ToString("N");
            jobResponse.Result = sb.ToString();
            jobResponse.IsSuccess = true;


            //do a long running process here......
            while (true)
            {               
                Console.Write($"{DateTime.UtcNow.Ticks}");
                Thread.Sleep(2);//                
            }

            Console.WriteLine($"Job {job.CommandId} Processing Done.");
            return jobResponse;
        }

        //cancelable work
        private static bool Work(MsgHandlerEventArgs e, CancellationToken cancelToken)
        {
            var count = 0;
            while (true)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    //Send Response to caller
                    SendReply(e.Message.Reply,
                        new PrinterOperationResult
                        {
                            CommandType = "Test",
                            Data = $"CancellationRequested at Count {count} ",
                            IsSuccess = true

                        });
                    return false;
                }
                else
                {
                    if(count == 1000000)
                    {
                       
                        //Send Response to caller
                        SendReply(e.Message.Reply, 
                            new PrinterOperationResult { 
                                CommandType = "Test",
                                Data = $"Count {count} ",
                                IsSuccess = true
                            
                            });
                        return true;  //completed signal
                    }
                    Console.WriteLine($"Count: {count}");
                    count++;
                }
               
            }
        }

        //        var cancelToken = new CancellationTokenSource();
        //        Task.Factory.StartNew(async () => {
        //    await Task.Delay(10000);
        //        // call web API
        //    }, cancelToken.Token);

        ////this stops the Task:
        //cancelToken.Cancel(false);
    }


    class OpResult
    {
        public string Message { get; set; }
    }



}
