using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Newtonsoft.Json;
using System.Web.Http;
using System.Web.Routing;
using TravelPlanner.DAL;
using TravelPlanner.Utilities;
using TravelPlanner.Enums;
using TravelPlanner.Constants;
using System.Data.Entity;
using TravelPlanner.Models;
using System.Text;
using System.Globalization;

namespace TravelPlanner.Controllers
{
    public class UserController : BaseController
    {
        public UserController()
        {
        }

        public UserController(IUserHelper userHelper)
        {
            UserHelper = userHelper;
        }

        // POST api/User/RegisterUser
        [System.Web.Http.HttpPost]
        public TokenResponse RegisterUser([FromUri]User user)
        {
            using (var context = new TravelPlannerEntities())
            {
                var validationMessage = ValidateUserData(user);
                if (!string.IsNullOrEmpty(validationMessage))
                {
                    throw new WebException(validationMessage);
                }

                if (context.User.Any(x => x.Username == user.Username))
                {
                    throw new WebException(Messages.DuplicatedUser);
                }

                var role = context.Role.FirstOrDefault(x => x.Id == user.RoleId);
                if (role == null)
                {
                    throw new WebException(Messages.RoleNotFound);
                }

                context.User.Add(user);
                context.SaveChanges();

                return CreateToken(user, context);
            }
        }

        // POST api/User/Login
        [System.Web.Http.HttpPost]
        public TokenResponse Login(string username, string password)
        {
            using (var context = new TravelPlannerEntities())
            {
                var user = context.User.FirstOrDefault(x => x.Username == username);
                if (user == null)
                {
                    throw new WebException(Messages.UserNotFound);
                }

                if (user.Password != password)
                {
                    throw new WebException(Messages.BadCredentials);
                }

                return CreateToken(user, context);
            }
        }

        // POST api/User/Logout
        [System.Web.Http.HttpPost]
        public bool Logout()
        {
            using (var context = new TravelPlannerEntities())
            {
                var loggedUserId = UserHelper.GetLoggedUser(Request).Id;
                context.User.First(x => x.Id == loggedUserId).Token = string.Empty;
                context.SaveChanges();
            }
            return true;
        }

        // GET api/User/GetAllUsers
        public IEnumerable<User> GetAllUsers()
        {
            var users = new List<User>();
            using (var context = new TravelPlannerEntities())
            {
                var userId = UserHelper.GetLoggedUser(Request).Id;
                var role = context.Role.FirstOrDefault(x => x.User.Any(y => y.Id == userId));

                if (role == null || role.Name == RolesEnum.User.ToString())
                {
                    throw new WebException(Messages.Unauthorized);
                }

                users = context.User.Where(x => x.Id != userId).ToList();
            }
            return users.Select(x => new User
            {
                Id = x.Id,
                Username = x.Username,
                RoleId = x.RoleId
            });
        }

        // GET api/User/GetAllRoles
        public IEnumerable<Role> GetAllRoles()
        {
            var roles = new List<Role>();
            using (var context = new TravelPlannerEntities())
            {
                var userId = UserHelper.GetLoggedUser(Request).Id;
                var role = context.Role.FirstOrDefault(x => x.User.Any(y => y.Id == userId));

                if (role != null && (role.Name == RolesEnum.Manager.ToString() || role.Name == RolesEnum.Administrator.ToString()))
                {
                    roles = context.Role.ToList();
                }
            }
            return roles.Select(x => new Role
            {
                Id = x.Id,
                Name = x.Name
            }).Distinct();
        }

        // POST api/User/CreateOrUpdateUser
        [System.Web.Http.HttpPost]
        public int CreateOrUpdateUser([FromUri]User user)
        {
            using (var context = new TravelPlannerEntities())
            {
                var loggedUser = UserHelper.GetLoggedUser(Request);

                var loggedRole = context.Role.First(x => x.Id == loggedUser.RoleId);

                if (loggedRole.Name == RolesEnum.User.ToString())
                {
                    throw new WebException(Messages.Unauthorized);
                }

                var validationMessage = ValidateUserData(user);
                if (!string.IsNullOrEmpty(validationMessage))
                {
                    throw new WebException(validationMessage);
                }

                if (user.Id == 0)
                {
                    context.User.Add(user);
                }
                else
                {
                    context.Entry(user).State = EntityState.Modified;
                }
                context.SaveChanges();
                return user.Id;
            }
        }

        // GET api/User/GetUser
        public User GetUser(int userId)
        {
            using (var context = new TravelPlannerEntities())
            {
                var user = GetUserIfAuthorized(userId, context);
                return new User
                {
                    Id = user.Id,
                    Username = user.Username,
                    RoleId = user.RoleId
                };
            }
        }

        // POST api/User/DeleteUser
        [System.Web.Http.HttpPost]
        public bool DeleteUser(int userId)
        {
            using (var context = new TravelPlannerEntities())
            {
                var user = GetUserIfAuthorized(userId, context);

                if (context.Trip.Any(x => x.UserId == userId))
                {
                    throw new WebException(Messages.UserWithTrips);
                }

                context.User.Remove(user);
                context.SaveChanges();
            }
            return true;
        }

        private static string ValidateUserData(User user)
        {
            var validationMessage = string.Empty;
            //DateTime date;
            if (string.IsNullOrEmpty(user.Username))
            {
                validationMessage += Messages.UserEmpty;
            }
            if (user.RoleId == 0)
            {
                validationMessage += Messages.RoleEmpty;
            }
            if (string.IsNullOrEmpty(user.Password))
            {
                validationMessage += Messages.PasswordEmpty;
            }
            if (string.IsNullOrEmpty(user.ConfirmPassword))
            {
                validationMessage += Messages.ConfirmPasswordEmpty;
            }
            if (user.ConfirmPassword != user.Password)
            {
                validationMessage += Messages.PasswordsDontMatch;
            }
            return validationMessage;
        }

        private TokenResponse CreateToken(User user, TravelPlannerEntities context)
        {
            var dateTime = DateTime.UtcNow;
            var time = BitConverter.GetBytes(dateTime.ToBinary());
            var key = Guid.NewGuid().ToByteArray();
            var token = Convert.ToBase64String(time.Concat(key).ToArray());

            user.Token = token;
            context.Entry(user).State = EntityState.Modified;
            context.SaveChanges();
            return new TokenResponse
            {
                Token = token,
                ExpirationDate = dateTime.AddHours(24).ToString("MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture),
                Role = user.Role.Name
            };
        }

        private User GetUserIfAuthorized(int userId, TravelPlannerEntities context)
        {
            var loggedUser = UserHelper.GetLoggedUser(Request);

            var role = context.Role.FirstOrDefault(x => x.Id == loggedUser.RoleId);

            if (role.Name == RolesEnum.User.ToString())
            {
                throw new WebException(Messages.Unauthorized);
            }

            var user = context.User.FirstOrDefault(x => x.Id == userId);

            if (user == null)
            {
                throw new WebException(Messages.UserNotFound);
            }
            return user;
        }
    }
}
