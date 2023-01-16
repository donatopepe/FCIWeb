using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace FCIWeb.Areas.FCI.Uility
{
    public class Excel
    {
        static void ConvertXlsxToCsv(string SourceXlsxName, string DestinationCsvDirectory)
        {
            try
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(SourceXlsxName, false))
                {

                    foreach (Sheet _Sheet in document.WorkbookPart.Workbook.Descendants<Sheet>())
                    {
                        WorksheetPart _WorksheetPart = (WorksheetPart)document.WorkbookPart.GetPartById(_Sheet.Id);
                        Worksheet _Worksheet = _WorksheetPart.Worksheet;

                        SharedStringTablePart _SharedStringTablePart = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
                        SharedStringItem[] _SharedStringItem = _SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();

                        if (string.IsNullOrEmpty(DestinationCsvDirectory))
                            DestinationCsvDirectory = Path.GetDirectoryName(SourceXlsxName);
                        string newFilename = string.Format("{0}_{1}.csv", Path.GetFileNameWithoutExtension(SourceXlsxName), _Sheet.Name);
                        newFilename = Path.Combine(DestinationCsvDirectory, newFilename);

                        using (var outputFile = File.CreateText(newFilename))
                        {
                            foreach (var row in _Worksheet.Descendants<Row>())
                            {
                                StringBuilder _StringBuilder = new StringBuilder();
                                foreach (Cell _Cell in row)
                                {
                                    string Value = string.Empty;
                                    if (_Cell.CellValue != null)
                                    {
                                        if (_Cell.DataType != null && _Cell.DataType.Value == CellValues.SharedString)
                                            Value = _SharedStringItem[int.Parse(_Cell.CellValue.Text)].InnerText;
                                        else
                                            Value = _Cell.CellValue.Text;
                                    }
                                    _StringBuilder.Append(string.Format("{0},", Value.Trim()));
                                }
                                outputFile.WriteLine(_StringBuilder.ToString().TrimEnd(','));
                            }
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

        static string ReadAsJSON(Stream fileExcel)
        {
            try
            {
                DataTable dtTable = new DataTable();

                //Lets open the existing excel file and read through its content . Open the excel using openxml sdk
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(fileExcel, false))
                {
                    //create the object for workbook part  
                    WorkbookPart workbookPart = doc.WorkbookPart;
                    Sheets thesheetcollection = workbookPart.Workbook.GetFirstChild<Sheets>();

                    //using for each loop to get the sheet from the sheetcollection  
                    foreach (Sheet thesheet in thesheetcollection.OfType<Sheet>())
                    {
                        //statement to get the worksheet object by using the sheet id  
                        Worksheet theWorksheet = ((WorksheetPart)workbookPart.GetPartById(thesheet.Id)).Worksheet;

                        SheetData thesheetdata = theWorksheet.GetFirstChild<SheetData>();

                        //List<Row> rows = thesheetdata.Elements<Row>().Where(row => row.Elements<Cell>().Any(cell => cell.DataType != null)).ToList();   ///codice da implementare se pulire righe vuote

                        for (int rCnt = 0; rCnt < thesheetdata.ChildElements.Count(); rCnt++)
                        {
                            List<string> rowList = new List<string>();
                            for (int rCnt1 = 0; rCnt1 < thesheetdata.ElementAt(rCnt).ChildElements.Count(); rCnt1++)
                            {

                                Cell thecurrentcell = (Cell)thesheetdata.ElementAt(rCnt).ChildElements.ElementAt(rCnt1);
                                //statement to take the integer value  
                                string currentcellvalue = string.Empty;
                                if (thecurrentcell.DataType != null)
                                {
                                    if (thecurrentcell.DataType == CellValues.SharedString)
                                    {
                                        int id;
                                        if (Int32.TryParse(thecurrentcell.InnerText, out id))
                                        {
                                            SharedStringItem item = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(id);
                                            if (item.Text != null)
                                            {
                                                //first row will provide the column name.
                                                if (rCnt == 0)
                                                {
                                                    dtTable.Columns.Add(item.Text.Text);
                                                }
                                                else
                                                {
                                                    rowList.Add(item.Text.Text);
                                                }
                                            }
                                            else if (item.InnerText != null)
                                            {
                                                currentcellvalue = item.InnerText;
                                            }
                                            else if (item.InnerXml != null)
                                            {
                                                currentcellvalue = item.InnerXml;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (rCnt != 0)//reserved for column values
                                    {
                                        rowList.Add(thecurrentcell.InnerText);
                                    }
                                }
                            }
                            if (rCnt != 0)//reserved for column values
                                dtTable.Rows.Add(rowList.ToArray());

                        }

                    }

                    return JsonConvert.SerializeObject(dtTable);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }


        //public class ExcelDocumentData
        //{
        //    public ExcelXmlStatus Status { get; set; }
        //    public IList<Sheet> Sheets { get; set; }
        //    public IList<ExcelSheetData> SheetData { get; set; }

        //    public ExcelDocumentData()
        //    {
        //        Status = new ExcelXmlStatus();
        //        Sheets = new List<Sheet>();
        //        SheetData = new List<ExcelSheetData>();
        //    }
        //}



        //public ExcelDocumentData ReadSpreadSheetDocument(SpreadsheetDocument mySpreadsheet, ExcelDocumentData data)
        //{
        //    var workbookPart = mySpreadsheet.WorkbookPart;

        //    data.Sheets = workbookPart.Workbook.Descendants<Sheet>().ToList();

        //    foreach (var sheet in data.Sheets)
        //    {
        //        var sheetData = new ExcelSheetData { SheetName = sheet.Name };
        //        var workSheet = ((WorksheetPart)workbookPart.GetPartById(sheet.Id)).Worksheet;

        //        sheetData.ColumnConfigurations = workSheet.Descendants<Columns>().FirstOrDefault();
        //        var rows = workSheet.Elements<SheetData>().First().Elements<Row>().ToList();
        //        if (rows.Count > 1)
        //        {
        //            foreach (var row in rows)
        //            {
        //                var dataRow = new List<ExcelCellWithType>();

        //                var cellEnumerator = GetExcelCellEnumerator(row);
        //                while (cellEnumerator.MoveNext())
        //                {
        //                    var cell = cellEnumerator.Current;
        //                    var cellWithType = ReadExcelCell(cell, workbookPart);
        //                    dataRow.Add(cellWithType);
        //                }

        //                sheetData.DataRows.Add(dataRow);
        //            }
        //        }
        //        data.SheetData.Add(sheetData);
        //    }

        //    return data;
        //}



        public class ExcelCellWithType
        {
            public string Value { get; set; }
            public UInt32Value ExcelCellFormat { get; set; }
            public bool IsDateTimeType { get; set; }
        }

        private ExcelCellWithType ReadExcelCell(Cell cell, WorkbookPart workbookPart)
        {
            var cellValue = cell.CellValue;
            var text = (cellValue == null) ? cell.InnerText : cellValue.Text;
            if (cell.DataType?.Value == CellValues.SharedString)
            {
                text = workbookPart.SharedStringTablePart.SharedStringTable
                    .Elements<SharedStringItem>().ElementAt(
                        Convert.ToInt32(cell.CellValue.Text)).InnerText;
            }

            var cellText = (text ?? string.Empty).Trim();

            var cellWithType = new ExcelCellWithType();

            if (cell.StyleIndex != null)
            {
                var cellFormat = workbookPart.WorkbookStylesPart.Stylesheet.CellFormats.ChildElements[
                    int.Parse(cell.StyleIndex.InnerText)] as CellFormat;

                if (cellFormat != null)
                {
                    cellWithType.ExcelCellFormat = cellFormat.NumberFormatId;

                    var dateFormat = GetDateTimeFormat(cellFormat.NumberFormatId);
                    if (!string.IsNullOrEmpty(dateFormat))
                    {
                        cellWithType.IsDateTimeType = true;

                        if (!string.IsNullOrEmpty(cellText))
                        {
                            if (double.TryParse(cellText, out var cellDouble))
                            {
                                var theDate = DateTime.FromOADate(cellDouble);
                                cellText = theDate.ToString(dateFormat);
                            }
                        }
                    }
                }
            }

            cellWithType.Value = cellText;

            return cellWithType;
        }

        //// https://msdn.microsoft.com/en-GB/library/documentformat.openxml.spreadsheet.numberingformat(v=office.14).aspx
        private readonly Dictionary<uint, string> DateFormatDictionary = new Dictionary<uint, string>()
        {
            [14] = "dd/MM/yyyy",
            [15] = "d-MMM-yy",
            [16] = "d-MMM",
            [17] = "MMM-yy",
            [18] = "h:mm AM/PM",
            [19] = "h:mm:ss AM/PM",
            [20] = "h:mm",
            [21] = "h:mm:ss",
            [22] = "M/d/yy h:mm",
            [30] = "M/d/yy",
            [34] = "yyyy-MM-dd",
            [45] = "mm:ss",
            [46] = "[h]:mm:ss",
            [47] = "mmss.0",
            [51] = "MM-dd",
            [52] = "yyyy-MM-dd",
            [53] = "yyyy-MM-dd",
            [55] = "yyyy-MM-dd",
            [56] = "yyyy-MM-dd",
            [58] = "MM-dd",
            [165] = "M/d/yy",
            [166] = "dd MMMM yyyy",
            [167] = "dd/MM/yyyy",
            [168] = "dd/MM/yy",
            [169] = "d.M.yy",
            [170] = "yyyy-MM-dd",
            [171] = "dd MMMM yyyy",
            [172] = "d MMMM yyyy",
            [173] = "M/d",
            [174] = "M/d/yy",
            [175] = "MM/dd/yy",
            [176] = "d-MMM",
            [177] = "d-MMM-yy",
            [178] = "dd-MMM-yy",
            [179] = "MMM-yy",
            [180] = "MMMM-yy",
            [181] = "MMMM d, yyyy",
            [182] = "M/d/yy hh:mm t",
            [183] = "M/d/y HH:mm",
            [184] = "MMM",
            [185] = "MMM-dd",
            [186] = "M/d/yyyy",
            [187] = "d-MMM-yyyy"
        };

        private string GetDateTimeFormat(UInt32Value numberFormatId)
        {
            return DateFormatDictionary.ContainsKey(numberFormatId) ? DateFormatDictionary[numberFormatId] : string.Empty;
        }
    }
}