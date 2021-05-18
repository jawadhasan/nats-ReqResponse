using System;
using System.Collections.Generic;
using System.Text;

namespace NatsSampleApp.Common
{
    public class Job
    {
        public Job(string command)
        {
            Id = Guid.NewGuid().ToString("N");
            CreatedAt = DateTime.Now;

            Command = command;
        }
        public string Id { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string Command { get; private set; }

    }
}
