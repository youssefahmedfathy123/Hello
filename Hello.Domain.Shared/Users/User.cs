using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hello.Domain.Users
{
    public class User : IdentityUser
    {
        public string Name { get; set; }

    }
}


