﻿using System;
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

        //редактировать GeoJSON
        static public void EditMapJSON(string databasePath, string mapPath)
        {
            List<RegionData> data = Parser.GetRegionData(databasePath);

            //текущий текст JSON'а
            string currentText = FilesHandler.ReadGeoJSON(mapPath);
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

                        //такое место найдено
                        if (location - ("\"ID_0\": 186,").Length > 0)
                        {
                            //значения показателей
                            string prodVolume = "";

                            //если этот показатель не был записан ранее
                            if (!text[i].Contains("production_volume"))
                            {
                                if (data[counter].ProdVolume != -1)
                                    prodVolume = " \"production_volume\": " + data[counter].ProdVolume.ToString().Replace(',', '.') + ",";
                                else
                                    prodVolume = " \"production_volume\": " + "null" + ",";

                                text[i] = text[i].Insert(location, prodVolume);

                                counter++;
                                newText = newText + text[i] + "\n";
                            }
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
            FilesHandler.WriteToGeoJSON(mapPath, newText);
        }

        //добавить названия регионов в файл данных CSV
        static public void AddRegions(string ruNamesPath, string databasePath)
        {
            //список добавляемых названий регионов
            List<string> regionNames = GetRuRegionNames(ruNamesPath);

            //добавить названия регионов в CSV-базу
            try
            {
                using (StreamWriter sw = new StreamWriter(databasePath, false, System.Text.Encoding.Default))
                {
                    sw.WriteLine("region;");
                    for (int i = 0; i < regionNames.Count; i++)
                        sw.WriteLine(regionNames[i] + ";");
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }
        }
    }
}