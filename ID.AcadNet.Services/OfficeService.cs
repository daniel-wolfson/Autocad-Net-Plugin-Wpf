namespace Intellidesk.AcadNet.Services
{
    public class AcadOfficeTools
    {
        public class ExcelHelper
        {
            //            public class CreateExcelDoc
            //            {
            //                private Excel.Application app = null;
            //                private Excel.Workbook workbook = null;
            //                private Excel.Worksheet worksheet = null;

            //                private Excel.Range workSheet_range = null;
            //                public CreateExcelDoc()
            //                {
            //                    createDoc();
            //                }

            //                public static void createDoc()
            //                {
            //                    try {
            //                        if (Process.GetProcessesByName("Excel").Length > 0) {
            //                            app = (Excel.Application)Interaction.GetObject(, "Excel.Application");
            //                         Else createObject
            //                        } else {
            //                            app = new Excel.Application[];
            //                        }
            //                        Tools.Visible = true;
            //                        workbook = Tools.Workbooks.Add(1);
            //                        worksheet = (Excel.Worksheet)workbook.Sheets(1);
            //                    } catch (Exception e) {
            //                        Console.Write("Error");
            //                    } finally {
            //                    }
            //                }

            //                public void createHeaders(int row, int col, string htext, string cell1, string cell2, int mergeColumns, System.Drawing.Color cellColor, bool isBold, int columnWidth, int fontSize, System.Drawing.Color fontColor)
            //                {
            //                    worksheet.Cells(row, col) = htext;
            //                    workSheet_range = worksheet.Range(cell1, cell2);
            //                    workSheet_range.Merge(mergeColumns);
            //                    workSheet_range.Interior.Color = cellColor;
            //                    workSheet_range.Font.Bold = isBold;
            //                    workSheet_range.Font.Size = fontSize;
            //                    workSheet_range.ColumnWidth = columnWidth;
            //                    workSheet_range.Font.Color = fontColor;
            //                }

            //                public void addData(int row, int col, string data, string cell1, string cell2, string format)
            //                {
            //                    worksheet.Cells(row, col) = data;
            //                    workSheet_range = worksheet.Range(cell1, cell2);
            //                    workSheet_range.Borders.Color = System.Drawing.Color.Black.ToArgb();
            //                    workSheet_range.NumberFormat = format;
            //                }

            //            }
        }
    }
}
