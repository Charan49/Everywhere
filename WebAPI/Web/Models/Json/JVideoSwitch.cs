using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Web.Models.Json
{
    public class JVideoSwitch
    {
        [Required]
        public int id;
        [Required]
        public string url;
   
      //  public Guid guid;
    }
}