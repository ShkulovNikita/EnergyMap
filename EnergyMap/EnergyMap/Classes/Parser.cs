using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

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
                using (StreamWriter sw = new StreamWriter(dataPath, false, System.Text.Encoding.Default))
                {
                    sw.WriteLine(text);
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }
        }
    }
}