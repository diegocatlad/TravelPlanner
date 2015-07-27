using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json;
using TravelPlanner.Utilities;

namespace TravelPlanner.Controllers
{
    public abstract class BaseController : ApiController
    {
        private IUserHelper _userHelper = new UserHelper();

        public IUserHelper UserHelper
        {
            get
            {
                return _userHelper;
            }
            set
            {
                _userHelper = value;
            }
        }

        internal string GetModelStateErrors(System.Web.Http.ModelBinding.ModelStateDictionary modelState)
        {
            var message = string.Empty;
            foreach (var value in modelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    message += error.ErrorMessage + "<br />";
                }
            }
            return message;
        }
    }
}