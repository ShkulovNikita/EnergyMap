using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;

namespace EnergyMap.Classes
{
    static public class FilesHandler
    {
        //получить названия регионов на английском
        static public void GetEngRegions(string mapPath, string savePath)
        {
            List<string> regions = new List<string>();

            try
            {
                //чтение файла для выделения списка регионов
                using (StreamReader sr = new StreamReader(mapPath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        //строка содержит регион
                        if(line.Contains("VARNAME_1"))
                        {
                            string[] dataParts = line.Split(',');
                            //7 - название региона
                            regions.Add(dataParts[7].Replace("\"VARNAME_1\": ", "").Replace("\"", "") + ";");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }

            //запись списка регионов в файл
            try
            {
                using (StreamWriter sw = new StreamWriter(savePath, false, System.Text.Encoding.Default))
                {
                    for (int i = 0; i < regions.Count; i++)
                        sw.WriteLine(regions[i]);
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }
        }

        //получить названия регионов на русском
        static public List<string> GetRuRegionNames(string path)
        {
            List<string> names = new List<string>();
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] translation = line.Split(';');
                        names.Add(translation[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }

            return names;
        }

        //прочитать GeoJSON файл
        static public string ReadGeoJSON(string mapPath)
        {
            string result;

            try
            {
                using (StreamReader sr = new StreamReader(mapPath, System.Text.Encoding.GetEncoding(1251)))
                {
                    result = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
                result = null;
            }

            return result;
        }

        //записать текст в GeoJSON файл
        static public void WriteToGeoJSON(string mapPath, string text)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(mapPath, false, System.Text.Encoding.Default))
                {
                    sw.WriteLine(text);
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }
        }

        //заменить англоязычные названия на русскоязычные
        static public void TranslateRegNames(string engPath, string mapPath, string newMapPath)
        {
            //считывание данных из файла названий регионов
            List<string> names = GetRuRegionNames(engPath);

            //исходный текст файла
            string currentText = ReadGeoJSON(mapPath);

            //текст файла с переведенными названиями
            string geoData = "";

            //обработка исходного файла
            try
            {
                //разбивка по строкам
                string[] text = currentText.Split('\n');

                //замена названий
                int counter = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    //перебор строк
                    if ((i > 2)&&(i < text.Length - 3))
                    {
                        //обработка строки Москвы
                        //отличается от других - использован костыль
                        if (i == 42)
                        {
                            text[i].Replace("\"VARNAME_1\": null, \"TYPE_1\":", "\"VARNAME_1\": \"г. Москва и Московская область\", \"TYPE_1\":");
                            counter++;
                        }
                        else
                        {
                            //найти, где начинается раздел названия региона
                            int location = text[i].IndexOf("VARNAME_1");

                            var regName = new StringBuilder();

                            //в этой строке содержится название региона
                            if (location > 0)
                            {
                                location = location + 13;

                                //получаем текущий символ
                                char currentSymbol = text[i][location];
                                while (currentSymbol != '"')
                                {
                                    //добавляем очередную букву названия региона
                                    regName.Append(currentSymbol);

                                    //переход к следующей букве
                                    location++;
                                    currentSymbol = text[i][location];
                                }
                            }

                            //замена английского названия русским
                            text[i] = text[i].Replace(regName.ToString(), names[counter]);
                            counter++;
                        }

                        geoData = geoData + text[i] + "\n";
                    }
                    else
                        geoData = geoData + text[i] + "\n";
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }

            //запись файла с переведенными названиями регионов
            WriteToGeoJSON(newMapPath, geoData);
        }
    }
}