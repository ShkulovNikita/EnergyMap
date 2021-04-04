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
            string regionEngNames = Server.MapPath("~/DataFiles/engRegions.data");
            string regionRuNames = Server.MapPath("~/DataFiles/ruRegions.data");
            string ruMapPath = Server.MapPath("~/Map/geo_ru.json");
            string databasePath = Server.MapPath("~/Database/Database.csv");

            //получение английских имен регионов из исходного GeoJSON
            FilesHandler.GetEngRegions(mapPath, regionEngNames);

            //преобразовать англоязычные имена в русские
            FilesHandler.TranslateRegNames(regionRuNames, mapPath, ruMapPath);

            //наполнить файл данных названиями регионов
            Parser.AddCountries(regionRuNames, databasePath);

            //создать data.js на основе GeoSJON
            Parser.CreateGEOData(ruMapPath, dataJSPath);

            return View();
        }
    }
}