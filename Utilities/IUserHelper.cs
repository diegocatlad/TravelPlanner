using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using TravelPlanner.DAL;

namespace TravelPlanner.Utilities
{
    public interface IUserHelper
    {
        User GetLoggedUser(HttpRequestMessage request);

        Role GetLoggedUserRole(HttpRequestMessage request);

    }
}