using Hello.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hello.Entityframework.Core.EntityFrameworkCore
{
    public class HelloDbContext : IdentityDbContext<User>
    {
        public HelloDbContext(DbContextOptions<HelloDbContext> options) : base(options) { }


    }
}


