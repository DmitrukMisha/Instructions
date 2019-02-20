
﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Instructions.Models
{
    public class User : IdentityUser
    {
        public string Language { get; set; }
        public bool Color { get; set; }
        public bool RoleISAdmin { get; set; }
        public bool Status { get; set; }
    }
}
