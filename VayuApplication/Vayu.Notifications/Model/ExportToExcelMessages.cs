using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;

namespace Vayu.Notifications.Model
{
    public class ExportToExcelMessages<T, U>
       where T : class
       where U : List<T>
    {
        #region Declaration

        /// <summary>
        /// The data to print
        /// </summary>
        public List<T> dataToPrint;
        // Excel object references.
        /// <summary>
        /// The excel application
        /// </summary>
        private Excel.Application _excelApp = null;
        /// <summary>
        /// The books
        /// </summary>
        private Excel.Workbooks _books = null;
        /// <summary>
        /// The book
        /// </summary>
        private Excel._Workbook _book = null;
        /// <summary>
        /// The sheets
        /// </summary>
        private Excel.Sheets _sheets = null;
        /// <summary>
        /// The sheet
        /// </summary>
        private Excel._Worksheet _sheet = null;
        /// <summary>
        /// The range
        /// </summary>
        private Excel.Range _range = null;
        /// <summary>
        /// The font
        /// </summary>
        private Excel.Font _font = null;
        // Optional argument variable
        /// <summary>
        /// The optional value
        /// </summary>
        private object _optionalValue = Missing.Value;

        #endregion

        /// <summary>
        /// Generates the report.
        /// </summary>
        public void GenerateReport(string Type)
        {
            try
            {
                if (dataToPrint != null)
                {
                    if (dataToPrint.Count != 0)
                    {
                        Mouse.SetCursor(Cursors.Wait);
                        CreateExcelRef();
                        FillSheet(Type);
                        OpenReport();
                        Mouse.SetCursor(Cursors.Arrow);
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Error while generating Excel report");
            }
            finally
            {
                ReleaseObject(_sheet);
                ReleaseObject(_sheets);
                ReleaseObject(_book);
                ReleaseObject(_books);
                ReleaseObject(_excelApp);
            }
        }

        #region Private Methods

        /// <summary>
        /// Opens the report.
        /// </summary>
        private void OpenReport()
        {
            _excelApp.Visible = true;
        }

        /// <summary>
        /// Fills the sheet.
        /// </summary>
        private void FillSheet(string Type)
        {
            object[] header = CreateHeader(Type);
            WriteData(header);
        }

        /// <summary>
        /// Writes the data.
        /// </summary>
        /// <param name="header">The header.</param>
        private void WriteData(object[] header)
        {
            object[,] objData = new object[dataToPrint.Count, header.Length];

            for (int j = 0; j < dataToPrint.Count; j++)
            {
                var item = dataToPrint[j];
                for (int i = 0; i < header.Length; i++)
                {
                    var y = typeof(T).InvokeMember(header[i].ToString(), BindingFlags.GetProperty, null, item, null);
                    if (y == null)
                    {
                        objData[j, i] = (y == null) ? "" : y.ToString();
                        continue;
                    }
                    Type t = y.GetType();
                    if (t.FullName.Equals("System.Double"))
                    {
                        string temp = (y == null) ? "" : string.Format("{0:N}", y);
                        objData[j, i] = temp;
                    }
                    else
                    {
                        objData[j, i] = (y == null) ? "" : y.ToString();
                    }
                }
            }
            AddExcelRows("A2", dataToPrint.Count, header.Length, objData);
            AutoFitColumns("A1", dataToPrint.Count + 1, header.Length);
        }

        /// <summary>
        /// Automatics the fit columns.
        /// </summary>
        /// <param name="startRange">The start range.</param>
        /// <param name="rowCount">The row count.</param>
        /// <param name="colCount">The col count.</param>
        private void AutoFitColumns(string startRange, int rowCount, int colCount)
        {
            _range = _sheet.get_Range(startRange, _optionalValue);
            _range = _range.get_Resize(rowCount, colCount);
            _range.Columns.AutoFit();
        }

        /// <summary>
        /// Creates the header.
        /// </summary>
        /// <returns></returns>
        private object[] CreateHeader(string Type)
        {
            PropertyInfo[] headerInfo = typeof(T).GetProperties();

            // Create an array for the headers and add it to the
            // worksheet starting at cell A1.
            List<object> objHeaders = new List<object>();
            for (int n = 0; n < headerInfo.Length; n++)
            {

                //if (headerInfo[n].Name.Equals("Date") || headerInfo[n].Name.Equals("Message") || headerInfo[n].Name.Equals("Type") || headerInfo[n].Name.Equals("Status") || headerInfo[n].Name.Equals("CreateDate"))
                //{
                //    continue;
                //}
                objHeaders.Add(headerInfo[n].Name);

            }

            var headerToAdd = objHeaders.ToArray();
            AddExcelRows("A1", 1, headerToAdd.Length, headerToAdd);
            SetHeaderStyle();

            return headerToAdd;
        }

        /// <summary>
        /// Sets the header style.
        /// </summary>
        private void SetHeaderStyle()
        {
            _font = _range.Font;
            _font.Bold = true;
        }

        /// <summary>
        /// Adds the excel rows.
        /// </summary>
        /// <param name="startRange">The start range.</param>
        /// <param name="rowCount">The row count.</param>
        /// <param name="colCount">The col count.</param>
        /// <param name="values">The values.</param>
        private void AddExcelRows(string startRange, int rowCount, int colCount, object values)
        {
            _range = _sheet.get_Range(startRange, _optionalValue);
            _range = _range.get_Resize(rowCount, colCount);
            _range.set_Value(_optionalValue, values);
        }
        /// <summary>
        /// Creates the excel reference.
        /// </summary>
        private void CreateExcelRef()
        {
            _excelApp = new Excel.Application();
            _books = (Excel.Workbooks)_excelApp.Workbooks;
            _book = (Excel._Workbook)(_books.Add(_optionalValue));
            _sheets = (Excel.Sheets)_book.Worksheets;
            _sheet = (Excel._Worksheet)(_sheets.get_Item(1));
        }
        /// <summary>
        /// Releases the object.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void ReleaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                System.Windows.MessageBox.Show(ex.Message.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

        #endregion
    }
}
