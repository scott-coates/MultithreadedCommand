using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MultithreadedCommand.Web.Models
{
    public class ReportEmail
    {
        [Required(ErrorMessage="Recipient Email Required.")]
        [RegularExpression((@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"),ErrorMessage="Invalid Email")]
        public string RecipientEmail { get; set; }

        [Required(ErrorMessage="From Email Required.")]
        [RegularExpression((@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"), ErrorMessage="Invalid Email")]
        public string FromEmail { get; set; }
        
        [Required(ErrorMessage="Report Name Required.")]
        public string ReportName { get; set; }
    }
}