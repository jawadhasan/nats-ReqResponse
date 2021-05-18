using System;
using System.Collections.Generic;
using System.Text;

namespace NatsSampleApp.Common
{
    public static class CommonConfigs
    {
        public static string MessageBrokerUrl = "nats://localhost:4222";
        public static string JobWorkerSubject = "natsapp.jobworker";
    }
}
