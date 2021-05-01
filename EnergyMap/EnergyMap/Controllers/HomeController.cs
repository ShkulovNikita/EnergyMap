using System;
using System.Web.Mvc;
using EnergyMap.Classes;

namespace EnergyMap.Controllers
{
    public class HomeController : Controller
    {
        //загрузка данных
        public FileResult GetFile()
        {
            string filePath = Server.MapPath("~/Database/database.csv");
            string fileType = "csv";
            string fileName = "data.csv";
            return File(filePath, fileType, fileName);
        }

        //исполнение набора функций для получения и обработки данных
        //выполнять при изменении исходного набора данных
        private void GetData()
        {
            //исходный файл GeoJSON
            string mapPath = Server.MapPath("~/Map/geo.json");
            //файл GeoJSON, записанный в переменную js
            string dataJSPath = Server.MapPath("~/Scripts/data.js");
            //англоязычные названия регионов
            string regionEngNames = Server.MapPath("~/DataFiles/engRegions.data");
            //сопоставление английских и русских названий регионов
            string regionRuNames = Server.MapPath("~/DataFiles/ruRegions.data");
            //файл GeoJSON с русскими названиями регионов
            string ruMapPath = Server.MapPath("~/Map/geo_ru.json");
            //CSV-файл данных
            string databasePath = Server.MapPath("~/Database/Database.csv");
            //экселевский файл с показателями
            string xlsPath = Server.MapPath("~/DataFiles/Карта энергетики_02Апр2021.xlsx");

            //получение английских имен регионов из исходного GeoJSON
            FilesHandler.GetEngRegions(mapPath, regionEngNames);

            //преобразовать англоязычные имена в русские
            FilesHandler.TranslateRegNames(regionRuNames, mapPath, ruMapPath);

            //наполнить файл данных названиями регионов
            FilesHandler.AddRegions(regionRuNames, databasePath);

            //парсинг Excel-файла с данными
            Parser.ParseData(xlsPath, databasePath);

            //внести показатели в файл GeoJSON
            FilesHandler.EditMapJSON(databasePath, ruMapPath);

            //создать data.js на основе GeoSJON
            FilesHandler.CreateGEOData(ruMapPath, dataJSPath);
        }

        public ActionResult Index()
        {
            //GetData();
            return View();
        }
    }
}