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

        //заменить англоязычные названия на русскоязычные
        static public void TranslateRegNames(string engPath, string mapPath, string newMapPath)
        {
            //считывание данных из файла названий регионов
            List<string> names = new List<string>();
            try
            {
                using (StreamReader sr = new StreamReader(engPath))
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

            //исходный текст файла
            string currentText = "";

            //текст файла с переведенными названиями
            string geoData = "";

            //считывание GeoJSON файла
            try
            {
                using (StreamReader sr = new StreamReader(mapPath))
                {
                    currentText = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }

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
            try
            {
                using (StreamWriter sw = new StreamWriter(newMapPath, false, System.Text.Encoding.Default))
                {
                    sw.WriteLine(geoData);
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }
        }
    }
}