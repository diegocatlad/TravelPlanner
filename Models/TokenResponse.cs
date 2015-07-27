using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TravelPlanner.Controllers
{
    public class TokenResponse
    {
        public string Token { get; set; }

        public string ExpirationDate { get; set; }

        public string Role { get; set; }

        public string Error { get; set; }
    }
}
