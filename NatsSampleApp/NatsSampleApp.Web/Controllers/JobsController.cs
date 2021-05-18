using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using NATS.Client;
using NatsSampleApp.Common;
using NatsSampleApp.Web.Dto;

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

                var job = new Job(jobRequest.Command);
                var operationResult = PublishMessageDBDataService(job);
                return Ok(operationResult);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e.Message);
            }

        }

        private JobResponse PublishMessageDBDataService(Job job)
        {
            var opts = ConnectionFactory.GetDefaultOptions();
            opts.Url = CommonConfigs.MessageBrokerUrl;
            var commandsSubject = CommonConfigs.JobWorkerSubject;

            using (IConnection c = new ConnectionFactory().CreateConnection(opts))
            {
                //Setup Reply Subscription
                string replySubject = $"_INBOX.{Guid.NewGuid().ToString("N")}";
                var subscription = c.SubscribeSync(replySubject);
                subscription.AutoUnsubscribe(1);

                // client also has a convenience-method to do this in line:
                //string replySubject = conn.NewInbox();

             
                // send with reply subject
                var data = JsonHelpers.Serialize(job);
                c.Publish(CommonConfigs.JobWorkerSubject, replySubject, data);

                // wait for response in reply subject
                var response = subscription.NextMessage(5000);

                //var responseData = Encoding.UTF8.GetString(response.Data);

                var jobResponse = JsonHelpers.Deserialize<JobResponse>(response.Data);


                return jobResponse;
            }



        }
    }
}

