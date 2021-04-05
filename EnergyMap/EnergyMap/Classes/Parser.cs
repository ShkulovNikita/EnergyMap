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
    
        //записать данные в CSV-файл
        static private void WriteRegionData(List<RegionData> data, string databasePath)
        {
            try
            {
                using(StreamWriter sw = new StreamWriter(databasePath, false, System.Text.Encoding.Default))
                {
                    sw.WriteLine("region;production_volume;");
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

        //строка с данными одного региона
        static private string GetStringForDatabase(RegionData data)
        {
            //строка данных об одном регионе
            string dataStr = data.Name + ";";

            //объем выработки
            dataStr = dataStr + CheckValue(data.ProdVolume);

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