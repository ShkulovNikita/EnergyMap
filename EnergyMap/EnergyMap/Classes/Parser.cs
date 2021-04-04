using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using EnergyMap.Classes;

namespace EnergyMap.Classes
{
    static public class Parser
    {
        //создание файла js с данными GeoJSON
        static public void CreateGEOData(string mapPath, string dataPath)
        {
            string text = "var bounds = ";
            //прочитать GeoJSON
            try
            {
                using (StreamReader sr = new StreamReader(mapPath))
                {
                    text = text + sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }

            //сохранить файл
            try
            {
                using (StreamWriter sw = new StreamWriter(dataPath, false, System.Text.Encoding.UTF8))
                {
                    sw.WriteLine(text);
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }
        }

        //добавить названия регионов в файл данных CSV
        static public void AddCountries(string ruNamesPath, string databasePath)
        {
            //список добавляемых названий регионов
            List<string> regionNames = FilesHandler.GetRuRegionNames(ruNamesPath);

            //добавить названия регионов в CSV-базу
            try
            {
                using (StreamWriter sw = new StreamWriter(databasePath, false, System.Text.Encoding.UTF8))
                {
                    sw.WriteLine("region");
                    for (int i = 0; i < regionNames.Count(); i++)
                        sw.WriteLine(regionNames[i] + "");
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }
        }
    }
}