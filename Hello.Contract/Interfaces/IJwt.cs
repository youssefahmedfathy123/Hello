using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hello.Contract.Interfaces
{
    public interface IJwt
    {
        Task<string> GenerateToken(List<string> roles, string username, string Id);

    }
}

