using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace EnergyMap.Classes
{
    static public class Parser
    {
        //получить данные по объему выработки
        static private Dictionary<string, double> GetProdVolume(XSSFWorkbook book)
        {
            Dictionary<string, double> data = new Dictionary<string, double>();

            try
            {
                var sheet = book.GetSheet("Выработка_области");

                int rowCount = sheet.LastRowNum;
                for (int i = 1; i < rowCount; i++)
                {
                    //получение очередной строки из таблицы
                    IRow currentRow = sheet.GetRow(i);

                    if (currentRow.GetCell(1).StringCellValue.Trim() != "Россия")
                    {
                        var cellRegion = currentRow.GetCell(1).StringCellValue.Trim();
                        var volumeValue = currentRow.GetCell(4).NumericCellValue;
                        data.Add(cellRegion, volumeValue);
                    }
                    else break;
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
                data = null;
            }

            return data;
        }

        //получить данные по себестоимости выработки
        static private Dictionary<string, double> GetProdPrice(XSSFWorkbook book)
        {
            Dictionary<string, double> data = new Dictionary<string, double>();
            try
            {
                var sheet = book.GetSheet("Выработка_области");

                int rowCount = sheet.LastRowNum;
                for (int i = 1; i < rowCount; i++)
                {
                    //получение очередной строки из таблицы
                    IRow currentRow = sheet.GetRow(i);
                    if (currentRow.GetCell(1).StringCellValue.Trim() != "Россия")
                    {
                        var cellRegion = currentRow.GetCell(1).StringCellValue.Trim();
                        var volumeValue = currentRow.GetCell(5).NumericCellValue;
                        data.Add(cellRegion, volumeValue);
                    }
                    else break;
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
                data = null;
            }

            return data;
        }

        //получить данные по объему потребления
        static private Dictionary<string, double> GetConsVolume(XSSFWorkbook book)
        {
            Dictionary<string, double> data = new Dictionary<string, double>();

            try
            {
                var sheet = book.GetSheet("Потребление_области");

                int rowCount = sheet.LastRowNum;
                for (int i = 1; i < rowCount; i++)
                {
                    //получение очередной строки из таблицы
                    IRow currentRow = sheet.GetRow(i);
                    if (currentRow.GetCell(1).StringCellValue.Trim() != "Россия")
                    {
                        var cellRegion = currentRow.GetCell(1).StringCellValue.Trim();
                        var volumeValue = currentRow.GetCell(4).NumericCellValue;
                        data.Add(cellRegion, volumeValue);
                    }
                    else break;
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
                data = null;
            }

            return data;
        }

        //получить данные по разнице потребления и выработки
        static private Dictionary<string, double> GetDifVolume(List<RegionData> regionData)
        {
            Dictionary<string, double> data = new Dictionary<string, double>();

            for (int i = 0; i < regionData.Count(); i++)
            {
                if (!data.ContainsKey(regionData[i].Name))
                {
                    if ((regionData[i].ProdVolume != -1) && (regionData[i].ConsVolume != -1))
                        data.Add(regionData[i].Name, regionData[i].ProdVolume - regionData[i].ConsVolume);
                    else
                        data.Add(regionData[i].Name, -1);
                }
            }

            return data;
        }

        //парсинг данных по объему выработки регионов
        static public void ParseProductionVolume(string dataPath, string databasePath)
        {
            //открыть Excel файл на чтение
            XSSFWorkbook book = OpenExcelBook(dataPath);

            //получить значения объема выработки
            Dictionary<string, double> data = GetProdVolume(book);

            //получить список регионов с их данными
            List<RegionData> regionData = GetRegionData(databasePath);

            //обновить данные по объему выработки
            regionData = AddProdVolume(data, regionData);

            //записать в CSV-файл данных
            WriteRegionData(regionData, databasePath);
        }

        //парсинг данных по себестоимости выработки регионов
        static public void ParseProductionPrice(string dataPath, string databasePath)
        {
            //открыть Excel файл на чтение
            XSSFWorkbook book = OpenExcelBook(dataPath);

            //получить значения себестоимости выработки
            Dictionary<string, double> data = GetProdPrice(book);

            //получить список регионов с их данными
            List<RegionData> regionData = GetRegionData(databasePath);

            //обновить данные по себестоимости выработки
            regionData = AddProdPrice(data, regionData);

            //записать в CSV-файл данных
            WriteRegionData(regionData, databasePath);
        }

        //парсинг данных по объему потребления регионов
        static public void ParseConsumptionVolume(string dataPath, string databasePath)
        {
            //открыть Excel файл на чтение
            XSSFWorkbook book = OpenExcelBook(dataPath);

            //получить значения объема потребления
            Dictionary<string, double> data = GetConsVolume(book);

            //получить список регионов с их данными
            List<RegionData> regionData = GetRegionData(databasePath);

            //обновить данные по объему потребления
            regionData = AddConsVolume(data, regionData);

            //записать в CSV-файл данных
            WriteRegionData(regionData, databasePath);
        }

        //получение данных по разнице выработки и потребления
        static public void ParseProdConsDifference(string dataPath, string databasePath)
        {
            //получить список регионов с их данными
            List<RegionData> regionData = GetRegionData(databasePath);

            //получить значения разницы потребления и выработки
            Dictionary<string, double> data = GetDifVolume(regionData);

            //обновить данные по разнице выработки и потребления
            regionData = AddDifVolume(data, regionData);

            //записать в CSV-файл данных
            WriteRegionData(regionData, databasePath);
        }

        //открыть файл данных Excel
        static private XSSFWorkbook OpenExcelBook(string path)
        {
            XSSFWorkbook book;

            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                book = new XSSFWorkbook(fs);
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
                book = null;
            }

            return book;
        }

        //обновить данные по объему выработки
        static private List<RegionData> AddProdVolume(Dictionary<string, double> newInfo, List<RegionData> regionData)
        {
            //перебор регионов из списка
            for (int i = 0; i < regionData.Count(); i++)
            {
                //если текущий регион упоминается в словаре, то информация переносится
                if (newInfo.ContainsKey(regionData[i].Name))
                {
                    regionData[i].ProdVolume = newInfo[regionData[i].Name];
                }
            }

            return regionData;
        }

        //обновить данные по себестоимости выработки
        static private List<RegionData> AddProdPrice(Dictionary<string, double> newInfo, List<RegionData> regionData)
        {
            //перебор регионов из списка
            for (int i = 0; i < regionData.Count(); i++)
            {
                //если текущий регион упоминается в словаре, то информация переносится
                if (newInfo.ContainsKey(regionData[i].Name))
                {
                    regionData[i].ProdPrice = newInfo[regionData[i].Name];
                }
            }

            return regionData;
        }

        //обновить данные по объему потребления
        static private List<RegionData> AddConsVolume(Dictionary<string, double> newInfo, List<RegionData> regionData)
        {
            //перебор регионов из списка
            for (int i = 0; i < regionData.Count(); i++)
            {
                //если текущий регион упоминается в словаре, то информация переносится
                if (newInfo.ContainsKey(regionData[i].Name))
                {
                    regionData[i].ConsVolume = newInfo[regionData[i].Name];
                }
            }

            return regionData;
        }

        //обновить данные по разнице выработки и потребления
        static private List<RegionData> AddDifVolume(Dictionary<string, double> newInfo, List<RegionData> regionData)
        {
            //перебор регионов из списка
            for (int i = 0; i < regionData.Count(); i++)
            {
                //если текущий регион упоминается в словаре, то информация переносится
                if (newInfo.ContainsKey(regionData[i].Name))
                {
                    regionData[i].ProdConsDif = newInfo[regionData[i].Name];
                }
            }

            return regionData;
        }

        //!
        //получить информацию о регионах из database.csv
        static public List<RegionData> GetRegionData(string databasePath)
        {
            List<RegionData> data = new List<RegionData>();

            //прочитать всю информацию из файла данных
            try
            {
                using (StreamReader sr = new StreamReader(databasePath, System.Text.Encoding.GetEncoding(1251)))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        //разбить очередную строку
                        string[] dataParts = line.Split(';');

                        //не заголовок
                        if (dataParts[0] != "region")
                        {
                            RegionData regionData = new RegionData();

                            //получение названия региона
                            regionData.Name = dataParts[0];

                            //проверка, были ли уже внесены изменения

                            //объем выработки
                            if ((dataParts[1] != "NULL") && (dataParts[1] != ""))
                                regionData.ProdVolume = Convert.ToDouble(dataParts[1].Replace('.', ','));
                            else
                                regionData.ProdVolume = -1;

                            //себестоимость выработки
                            if ((dataParts[2] != "NULL") && (dataParts[2] != ""))
                                regionData.ProdPrice = Convert.ToDouble(dataParts[2].Replace('.', ','));
                            else
                                regionData.ProdPrice = -1;

                            //объем потребления
                            if ((dataParts[3] != "NULL") && (dataParts[3] != ""))
                                regionData.ConsVolume = Convert.ToDouble(dataParts[3].Replace('.', ','));
                            else
                                regionData.ConsVolume = -1;

                            //разница потребления и выработки
                            if ((dataParts[4] != "NULL") && (dataParts[4] != ""))
                                regionData.ProdConsDif = Convert.ToDouble(dataParts[4].Replace('.', ','));
                            else
                                regionData.ProdConsDif = -1;

                            data.Add(regionData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
                data = null;
            }

            return data;
        }
    
        //!
        //записать данные в CSV-файл
        static private void WriteRegionData(List<RegionData> data, string databasePath)
        {
            try
            {
                using(StreamWriter sw = new StreamWriter(databasePath, false, System.Text.Encoding.UTF8))
                {
                    sw.WriteLine("region;production_volume;production_price;consumption_volume;production_consumption_difference;");
                    for (int i = 0; i < data.Count(); i++)
                    {
                        string dataStr = GetStringForDatabase(data[i]);
                        sw.WriteLine(dataStr);
                    }
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }
        }

        //!
        //строка с данными одного региона
        static private string GetStringForDatabase(RegionData data)
        {
            //строка данных об одном регионе
            string dataStr = data.Name + ";";

            //объем выработки
            dataStr = dataStr + CheckValue(data.ProdVolume);

            //себестоимость выработки
            dataStr = dataStr + CheckValue(data.ProdPrice);

            //объем потребления
            dataStr = dataStr + CheckValue(data.ConsVolume);

            //разница выработки и потребления
            dataStr = dataStr + CheckValue(data.ProdConsDif);

            return dataStr;
        }

        static private string CheckValue(double value)
        {
            string result = "";

            if (value != -1)
                result = result + value.ToString().Replace(',', '.') + ";";
            else
                result = result + "NULL;";

            return result;
        }
    }
}