using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;

namespace oAuthMVC.Controllers
{

    [HandleError]
    public class AccountController : Controller
    {

        public ActionResult LogOn()
        {

            return View();
        }
    }
}
