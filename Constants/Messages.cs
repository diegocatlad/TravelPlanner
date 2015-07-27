using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TravelPlanner.Constants
{
    public static class Messages
    {
        public const string BadCredentials = "User or password incorrect";
        public const string ConfirmPasswordEmpty = "Confirm password cannot be empty. ";
        public const string DestinationEmpty = "Destination cannot be empty. ";
        public const string DuplicatedUser = "User name already exists in the database.";
        public const string ErrorOcurred = "An unexpected error has ocurred. If the problem persists, please contact your administrator.";
        public const string InvalidData = "Invalid data";
        public const string PasswordEmpty = "Password cannot be empty. ";
        public const string PasswordsDontMatch = "The password and confirmation password do not match. ";
        public const string RoleEmpty = "Role cannot be empty. ";
        public const string RoleNotFound = "Role could not be found.";
        public const string SuccesfulDelete = "Data has been deleted succeessfully";
        public const string SuccesfulSave = "Data has been saved succeessfully";
        public const string TripNotFound = "Trip could not be found";
        public const string Unauthenticated = "You need to login to perform this action";
        public const string Unauthorized = "You don't have enough permissions to perform this action.";
        public const string UserNotFound = "User could not be found";
        public const string UserWithTrips = "User cannot be removed as he has related trips.";
        public const string StartDateHigherThanEndDate = "End date must be higher than Start date. ";
        public const string UserEmpty = "User cannot be empty. ";
    }
}