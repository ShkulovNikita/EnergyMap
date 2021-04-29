using System;
using System.Collections.Generic;
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
            WriteRegionsToFile(regions, savePath);
        }

        //записать список регионов в файл
        static private void WriteRegionsToFile(List<string> regions, string savePath)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(savePath, false, System.Text.Encoding.UTF8))
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

        //получить название региона из строки
        static private string GetRegName(string line)
        {
            //найти, где начинается раздел названия региона
            int location = line.IndexOf("VARNAME_1");

            var regName = new StringBuilder();

            //в этой строке содержится название региона
            if (location > 0)
            {
                location = location + 13;

                //получаем текущий символ
                char currentSymbol = line[location];
                while (currentSymbol != '"')
                {
                    //добавляем очередную букву названия региона
                    regName.Append(currentSymbol);

                    //переход к следующей букве
                    location++;
                    currentSymbol = line[location];
                }
            }

            return regName.ToString();
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
                using (StreamWriter sw = new StreamWriter(mapPath, false, System.Text.Encoding.UTF8))
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
            string geoData = GetTranslatedGeoJSON(currentText, names);

            //запись файла с переведенными названиями регионов
            WriteToGeoJSON(newMapPath, geoData);
        }

        //получение текста GeoJSON с переведенными названиями регионов
        static private string GetTranslatedGeoJSON(string currentText, List<string> names)
        {
            string geoData = "";

            try
            {
                //разбивка по строкам
                string[] text = currentText.Split('\n');

                //замена названий
                int counter = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    //перебор строк
                    if ((i > 2) && (i < text.Length - 3))
                    {
                        //обработка строки Москвы
                        //отличается от других - использован костыль
                        if (i == 42)
                        {
                            string curr1 = text[i];
                            text[i] = text[i].Replace("\"VARNAME_1\": null", "\"VARNAME_1\": \"г. Москва и Московская область\"");
                            string curr2 = text[i];
                            counter++;
                        }
                        else
                        {
                            //получение из текущей строки английского названия региона
                            string regName = GetRegName(text[i]);

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
                geoData = null;
            }

            return geoData;
        }

        //создание файла js с данными GeoJSON
        static public void CreateGEOData(string mapPath, string dataPath)
        {
            string text = "var bounds = ";

            //прочитать GeoJSON
            try
            {
                using (StreamReader sr = new StreamReader(mapPath, Encoding.GetEncoding(1251)))
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
                using (StreamWriter sw = new StreamWriter(dataPath, false, Encoding.UTF8))
                {
                    sw.WriteLine(text);
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }
        }

        //получение строки значения показателя
        static private string GetIndicatorString(string currentLine, string indicator, double indValue)
        {
            string result = "";
            
            //если текущий показатель не был записан ранее
            if(!currentLine.Contains(indicator))
            {
                //есть известное значение показателя
                if (indValue != -1)
                    result = " \"" + indicator + "\": " + Math.Round(indValue, 2).ToString().Replace(',', '.') + ",";
                else
                    result = " \"" + indicator + "\": " + "null" + ",";
            }

            return result;
        }

        //!
        //получить значения показателей для одного региона
        static private List<string> GetRegionIndicators(string currentLine, RegionData data)
        {
            List<string> result = new List<string>();

            result.Add(GetIndicatorString(currentLine, "production_volume", data.ProdVolume));
            result.Add(GetIndicatorString(currentLine, "production_price", data.ProdPrice));
            result.Add(GetIndicatorString(currentLine, "consumption_volume", data.ConsVolume));
            result.Add(GetIndicatorString(currentLine, "production_consumption_difference", data.ProdConsDif));

            return result;
        }

        //!
        //редактировать GeoJSON
        static public void EditMapJSON(string databasePath, string mapPath)
        {
            List<RegionData> data = Parser.GetRegionData(databasePath);

            //текущий текст JSON'а
            string currentText = ReadGeoJSON(mapPath);
            //обновленный текст JSON'а
            string newText = "";

            try
            {
                //разбивка по строкам
                string[] text = currentText.Split('\n');

                int counter = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    //строка с данными о регионе
                    if ((i > 2) && (i < text.Length - 3))
                    {
                        //найти запятую, после которой можно вставить новый показатель
                        int location = text[i].IndexOf("\"ID_0\": 186,") + ("\"ID_0\": 186,").Length;

                        //Крым имеет другой идентификатор
                        if (text[i].Contains("\"ID_0\": 237,"))
                        {
                            location = text[i].IndexOf("\"ID_0\": 237,") + ("\"ID_0\": 186,").Length;
                        }

                        //такое место найдено
                        if (location - ("\"ID_0\": 186,").Length > 0)
                        {
                            //значения показателей
                            List<string> indValues = GetRegionIndicators(text[i], data[counter]);

                            //записать все полученные значения показателей
                            string insertString = "";
                            for (int j = 0; j < indValues.Count; j++)
                                insertString = insertString + indValues[j];

                            text[i] = text[i].Insert(location, insertString);

                            counter++;
                            newText = newText + text[i] + "\n";
                        }
                    }
                    else
                    {
                        newText = newText + text[i] + "\n";
                    }
                }
                newText = newText + "]\n";
                newText = newText + "}\n";
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
                newText = null;
            }

            //записать новый текст файла GeoJSON
            WriteToGeoJSON(mapPath, newText);
        }

        //!
        //добавить названия регионов в файл данных CSV
        static public void AddRegions(string ruNamesPath, string databasePath)
        {
            //список добавляемых названий регионов
            List<string> regionNames = GetRuRegionNames(ruNamesPath);

            //добавить названия регионов в CSV-базу
            try
            {
                using (StreamWriter sw = new StreamWriter(databasePath, false, System.Text.Encoding.UTF8))
                {
                    sw.WriteLine("region;;;;");
                    for (int i = 0; i < regionNames.Count; i++)
                        sw.WriteLine(regionNames[i] + ";;;;");
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }
        }
    }
}