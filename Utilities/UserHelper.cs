using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using TravelPlanner.DAL;

namespace TravelPlanner.Utilities
{
    public class UserHelper : IUserHelper
    {
        public User GetLoggedUser(HttpRequestMessage request)
        {
            var token = GetUserToken(request);

            using (var context = new TravelPlannerEntities())
            {
                var tokenUser = context.User.FirstOrDefault(x => x.Token == token);
                if (tokenUser == null)
                {
                    throw new UnauthorizedAccessException();
                }
                return tokenUser;
            }
        }

        public Role GetLoggedUserRole(HttpRequestMessage request)
        {
            var token = GetUserToken(request);

            using (var context = new TravelPlannerEntities())
            {
                var tokenUser = context.User.FirstOrDefault(x => x.Token == token);
                if (tokenUser == null)
                {
                    throw new UnauthorizedAccessException();
                }
                return tokenUser.Role;
            }
        }

        private static string GetUserToken(HttpRequestMessage request)
        {
            if (!request.Headers.Any(x => x.Key == "Authorization"))
            {
                throw new UnauthorizedAccessException();
            }

            var token = request.Headers.GetValues("Authorization").FirstOrDefault();

            byte[] data = Convert.FromBase64String(token);
            DateTime when = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
            if (when < DateTime.UtcNow.AddHours(-24))
            {
                throw new UnauthorizedAccessException();
            }
            return token;
        }
    }
}