using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Security.Principal;
using TravelPlanner.DAL;
using TravelPlanner.Utilities;
using TravelPlanner.Enums;
using TravelPlanner.Constants;

namespace TravelPlanner.Controllers
{
    public class TripController : ApiController
    {
        private readonly IUserHelper UserHelper = new UserHelper();

        public TripController()
        {
        }

        public TripController(IUserHelper userHelper)
        {
            UserHelper = userHelper;
        }

        // GET api/User/GetAllTrips
        public IEnumerable<Trip> GetAllTrips()
        {
            var trips = new List<Trip>();
            using (var context = new TravelPlannerEntities())
            {
                var userId = UserHelper.GetLoggedUser(Request).Id;
                var role = context.Role.FirstOrDefault(x => x.User.Any(y => y.Id == userId));

                if (role.Name == RolesEnum.Manager.ToString())
                {
                    throw new WebException(Messages.Unauthorized);
                }

                if (role != null)
                {
                    trips = context.Trip.Where(x =>
                        role.Name != RolesEnum.Manager.ToString() &&
                        (x.User.Id == userId || role.Name == RolesEnum.Administrator.ToString())).ToList();
                }
            }
            return trips.Select(x => new Trip
            {
                Id = x.Id,
                Destination = x.Destination,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Comment = x.Comment,
                UserId = x.UserId
            });
        }

        [System.Web.Http.HttpPost]
        // POST api/User/CreateOrUpdateTrip
        public int CreateOrUpdateTrip([FromUri]Trip trip)
        {
            using (var context = new TravelPlannerEntities())
            {
                var user = UserHelper.GetLoggedUser(Request);

                var role = context.Role.First(x => x.Id == user.RoleId);

                if (role.Name == RolesEnum.Manager.ToString())
                {
                    throw new WebException(Messages.Unauthorized);
                }

                if (trip.Id == 0)
                {
                    trip.UserId = user.Id;
                }

                var validationMessage = ValidateTripData(trip);
                if (!string.IsNullOrEmpty(validationMessage))
                {
                    throw new WebException(validationMessage);
                }

                if (trip.Id == 0)
                {
                    context.Trip.Add(trip);
                }
                else
                {
                    if (trip.UserId != user.Id && role.Name != RolesEnum.Administrator.ToString())
                    {
                        throw new WebException(Messages.Unauthorized);
                    }

                    context.Entry(trip).State = EntityState.Modified;
                }
                context.SaveChanges();
                return trip.Id;
            }
        }

        // GET api/User/GetTrip
        public Trip GetTrip(int tripId)
        {
            using (var context = new TravelPlannerEntities())
            {
                var trip = GetTripIfAuthorized(tripId, context);
                return new Trip
                {
                    Id = trip.Id,
                    Destination = trip.Destination,
                    StartDate = trip.StartDate,
                    EndDate = trip.EndDate,
                    Comment = trip.Comment,
                    UserId = trip.UserId
                };
            }
        }

        [System.Web.Http.HttpPost]
        // POST api/User/DeleteTrip
        public bool DeleteTrip(int tripId)
        {
            using (var context = new TravelPlannerEntities())
            {
                var trip = GetTripIfAuthorized(tripId, context);

                context.Trip.Remove(trip);
                context.SaveChanges();
            }
            return true;
        }

        private static string ValidateTripData(Trip trip)
        {
            var validationMessage = string.Empty;
            //DateTime date;
            if (string.IsNullOrEmpty(trip.Destination))
            {
                validationMessage += Messages.DestinationEmpty;
            }
            if (trip.UserId == 0)
            {
                validationMessage += Messages.UserEmpty;
            }
            if (!(trip.EndDate > trip.StartDate))
            {
                validationMessage += Messages.StartDateHigherThanEndDate;
            }
            return validationMessage;
        }

        private Trip GetTripIfAuthorized(int tripId, TravelPlannerEntities context)
        {
            var user = UserHelper.GetLoggedUser(Request);

            var role = context.Role.FirstOrDefault(x => x.Id == user.RoleId);

            var trip = context.Trip.FirstOrDefault(x => x.Id == tripId);

            if (trip == null)
            {
                throw new WebException(Messages.TripNotFound);
            }

            if (!context.Trip.Any(x => x.Id == tripId &&
                role.Name != RolesEnum.Manager.ToString() &&
                (x.User.Id == user.Id || role.Name == RolesEnum.Administrator.ToString())))
            {
                throw new WebException(Messages.Unauthorized);
            }
            return trip;
        }
    }
}
