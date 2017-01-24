using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Web.Models.Json
{
    public class JLiveStream
    {
        public string serviceName;
        public string streamId;
        public string streamUrl;
        public DateTime streamDate;
    }

    public class JServiceName
    {
        [Required]
        public string serviceNames { get; set; }


    }
}