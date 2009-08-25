using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Resources;

namespace oAuthMVC.Controllers
{
    [OAuthorization]
    [WebApiEnabled]
    public class APIController : Controller
    {   
        public ActionResult GetName()
        {
            ViewData.Model = Global.LoggedInUser.FullName;
            return View();
        }

        public ActionResult GetAge()
        {
            ViewData.Model = Global.LoggedInUser.Age;
            return View();
        }

        public ActionResult GetFavoriteSites()
        {
            ViewData.Model = Global.LoggedInUser.FavoriteSites.Select(site => site.SiteUrl).ToArray();
            return View();
        }
    }
}
