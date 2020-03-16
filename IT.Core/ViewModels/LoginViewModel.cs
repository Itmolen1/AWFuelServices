﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.Core.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }

        public string Token { get; set; }
        public string DeviceId { get; set; }
        public string Device { get; set; }
        public string Authority { get; set; }
        public int CompanyId { get; set; }


    }
}
