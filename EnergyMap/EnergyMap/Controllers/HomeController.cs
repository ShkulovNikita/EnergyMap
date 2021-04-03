using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnergyMap.Classes;

namespace EnergyMap.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string mapPath = Server.MapPath("~/Map/geo.json");
            string dataJSPath = Server.MapPath("~/Scripts/data.js");

            Parser.CreateGEOData(mapPath, dataJSPath);

            return View();
        }
    }
}