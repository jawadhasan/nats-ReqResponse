using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using NATS.Client;
using NatsSampleApp.Common;
using NatsSampleApp.Web.Dto;
using ModbusPOC.CommandMessages;

namespace NatsSampleApp.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {

        [HttpPost("Execute")]
        public IActionResult Execute([FromBody] JobRequest jobRequest)
        {

            try
            {
                //TODO: Model Validation
                // var job = new Job(jobRequest.Command);
                var job = new EmptyCommand("","","");
                var operationResult = PublishMessageService(job);
                return Ok(operationResult);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e.Message);
            }

        }


        [HttpPost("ExecuteJob")]
        public IActionResult ExecuteJob([FromBody] PrinterCommand printerCommand)
        {

            try
            {
                //TODO: Model Validation              
                var command = CommandBuilder.BuildCommand(printerCommand);
                var operationResult = PublishMessageService(command);
                return Ok(operationResult);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e.Message);
            }

        }

        private PrinterOperationResult PublishMessageService(ICommand command)
        {
            var opts = ConnectionFactory.GetDefaultOptions();
            opts.Url = CommonConfigs.MessageBrokerUrl;         

            using (IConnection c = new ConnectionFactory().CreateConnection(opts))
            {
                //Setup Reply Subscription
                string replySubject = $"_INBOX.{Guid.NewGuid().ToString("N")}";
                var subscription = c.SubscribeSync(replySubject);
                subscription.AutoUnsubscribe(1);

                // client also has a convenience-method to do this in line:
                //string replySubject = conn.NewInbox();

             
                // send with reply subject
                var data = JsonHelpers.Serialize(command);
                c.Publish(CommonConfigs.JobWorkerSubject, replySubject, data);

                // wait for response in reply subject
                var response = subscription.NextMessage(10000);

                //var responseData = Encoding.UTF8.GetString(response.Data);

                var jobResponse = JsonHelpers.Deserialize<PrinterOperationResult>(response.Data);


                return jobResponse;
            }



        }
    }


}

