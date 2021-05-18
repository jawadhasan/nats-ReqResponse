using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NatsSampleApp.Web.Dto
{
    public class JobRequest
    {
        [Required]
        public string Command { get; set; }
    }
}
