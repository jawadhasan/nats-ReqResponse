using System;
using System.Collections.Generic;
using System.Text;

namespace NatsSampleApp.Common
{
    //Can use Envelop Class Instead
    public class JobResponse
    {
        public JobResponse()
        {
        }
        public string Id { get; set; }
        public string Result { get; set; }
        public bool IsSuccess { get; set; }
    }
}
