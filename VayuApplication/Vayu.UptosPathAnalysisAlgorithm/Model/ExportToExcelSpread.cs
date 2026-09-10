using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;
namespace Vayu.UptosPathAnalysisAlgorithm.Model
{
    public class ExportToExcelSpread<T, U>
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
        public void GenerateReport(string type)
        {
            try
            {
                if (dataToPrint != null)
                {
                    if (dataToPrint.Count != 0)
                    {
                        Mouse.SetCursor(Cursors.Wait);
                        CreateExcelRef();
                        FillSheet(type);
                        // OpenReport();
                        Mouse.SetCursor(Cursors.Arrow);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while generating Excel report");
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
        private void FillSheet(string type)
        {
            object[] header = CreateHeader(type);
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

            MessageBox.Show("Data has been sucessfully exported to CSV file", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private object[] CreateHeader(string type)
        {
            PropertyInfo[] headerInfo = typeof(T).GetProperties();

            // Create an array for the headers and add it to the
            // worksheet starting at cell A1.
            List<object> objHeaders = new List<object>();
            for (int n = 0; n < headerInfo.Length; n++)
            {

                if (type == "Block_B1" || type == "Block_B2")
                    if (headerInfo[n].Name.Equals("AnalysisTypeInt") || headerInfo[n].Name.Equals("Correlation") || headerInfo[n].Name.Equals("MW") || headerInfo[n].Name.Equals("CountDays") || headerInfo[n].Name.Equals("CalcNumber") ||
                        headerInfo[n].Name.Equals("PathMinRT") || headerInfo[n].Name.Equals("SourceFuel") || headerInfo[n].Name.Equals("SinkFuel") || headerInfo[n].Name.Equals("Skew") || headerInfo[n].Name.Equals("Kurtosis") ||
                        headerInfo[n].Name.Equals("IncDec") || headerInfo[n].Name.Equals("YearlyDownSide") || headerInfo[n].Name.Equals("SourceNodeKey") || headerInfo[n].Name.Equals("SinkNodeKey") || headerInfo[n].Name.Equals("MarketDate") ||
                        headerInfo[n].Name.Equals("HoursCleared") || headerInfo[n].Name.Equals("WeeklyWin") || headerInfo[n].Name.Equals("StdDev") || headerInfo[n].Name.Equals("ConstraintText") || headerInfo[n].Name.Equals("ContingencyText") ||
                        headerInfo[n].Name.Equals("family") || headerInfo[n].Name.Equals("Sensitivity") || headerInfo[n].Name.Equals("RTMedian") || headerInfo[n].Name.Equals("DAMedian") || headerInfo[n].Name.Equals("RTStdDev") ||
                        headerInfo[n].Name.Equals("DAStdDev") || headerInfo[n].Name.Equals("AvgRtLoss") || headerInfo[n].Name.Equals("AvgRtCong") || headerInfo[n].Name.Equals("CorrelationAsBid") || headerInfo[n].Name.Equals("RTMedianAsBid") ||
                        headerInfo[n].Name.Equals("DAMedianAsBid") || headerInfo[n].Name.Equals("RTStdDevAsBid") || headerInfo[n].Name.Equals("DAStdDevAsBid") || headerInfo[n].Name.Equals("DARTAsBid") || headerInfo[n].Name.Equals("DollarPerMWAsBid"))
                        continue;
                if (type == "ErcotBlock_B1")
                    if (headerInfo[n].Name.Equals("AnalysisTypeInt") || headerInfo[n].Name.Equals("Correlation") || headerInfo[n].Name.Equals("CountDays") || headerInfo[n].Name.Equals("SourceNodeType") || headerInfo[n].Name.Equals("SinkNodeType") ||
                    headerInfo[n].Name.Equals("Skew") || headerInfo[n].Name.Equals("Kurtosis") ||
                    headerInfo[n].Name.Equals("IncDec") || headerInfo[n].Name.Equals("SourceNodeKey") || headerInfo[n].Name.Equals("SinkNodeKey") || headerInfo[n].Name.Equals("MarketDate") ||
                    headerInfo[n].Name.Equals("HoursCleared") || headerInfo[n].Name.Equals("WeeklyWin") || headerInfo[n].Name.Equals("StdDev") || headerInfo[n].Name.Equals("ConstraintText") || headerInfo[n].Name.Equals("ContingencyText") ||
                    headerInfo[n].Name.Equals("family") || headerInfo[n].Name.Equals("Sensitivity") || headerInfo[n].Name.Equals("RTMedian") || headerInfo[n].Name.Equals("DAMedian") || headerInfo[n].Name.Equals("RTStdDev") ||
                    headerInfo[n].Name.Equals("DAStdDev") || headerInfo[n].Name.Equals("AvgRtLoss") || headerInfo[n].Name.Equals("AvgRtCong") || headerInfo[n].Name.Equals("CorrelationAsBid") || headerInfo[n].Name.Equals("RTMedianAsBid") ||
                    headerInfo[n].Name.Equals("DAMedianAsBid") || headerInfo[n].Name.Equals("RTStdDevAsBid") || headerInfo[n].Name.Equals("DAStdDevAsBid") || headerInfo[n].Name.Equals("DARTAsBid") || headerInfo[n].Name.Equals("DollarPerMWAsBid"))
                        continue;
                if (type == "ErcotBlock_B2")
                    if (headerInfo[n].Name.Equals("AnalysisTypeInt") || headerInfo[n].Name.Equals("Correlation") || headerInfo[n].Name.Equals("CountDays") || headerInfo[n].Name.Equals("SourceNodeType") || headerInfo[n].Name.Equals("SinkNodeType") ||
                    headerInfo[n].Name.Equals("IncDec") || headerInfo[n].Name.Equals("SourceNodeKey") || headerInfo[n].Name.Equals("SinkNodeKey") || headerInfo[n].Name.Equals("MarketDate") ||
                    headerInfo[n].Name.Equals("HoursCleared") || headerInfo[n].Name.Equals("WeeklyWin") || headerInfo[n].Name.Equals("StdDev") || headerInfo[n].Name.Equals("ConstraintText") || headerInfo[n].Name.Equals("ContingencyText") ||
                    headerInfo[n].Name.Equals("family") || headerInfo[n].Name.Equals("Sensitivity") || headerInfo[n].Name.Equals("RTMedian") || headerInfo[n].Name.Equals("DAMedian") || headerInfo[n].Name.Equals("RTStdDev") ||
                    headerInfo[n].Name.Equals("DAStdDev") || headerInfo[n].Name.Equals("AvgRtLoss") || headerInfo[n].Name.Equals("AvgRtCong") || headerInfo[n].Name.Equals("CorrelationAsBid") || headerInfo[n].Name.Equals("RTMedianAsBid") ||
                    headerInfo[n].Name.Equals("DAMedianAsBid") || headerInfo[n].Name.Equals("RTStdDevAsBid") || headerInfo[n].Name.Equals("DAStdDevAsBid") || headerInfo[n].Name.Equals("DARTAsBid") || headerInfo[n].Name.Equals("DollarPerMWAsBid"))
                        continue;
                if (type == "Correlations" || type == "NegativeCorrelations")
                    if (headerInfo[n].Name.Equals("AnalysisTypeInt") || headerInfo[n].Name.Equals("MW") || headerInfo[n].Name.Equals("CountDays") || headerInfo[n].Name.Equals("CountCleared") || headerInfo[n].Name.Equals("PctWin") || headerInfo[n].Name.Equals("MaxWin") || headerInfo[n].Name.Equals("MaxLoss") ||
                    headerInfo[n].Name.Equals("Skew") || headerInfo[n].Name.Equals("Kurtosis") || headerInfo[n].Name.Equals("PathMinRT") || headerInfo[n].Name.Equals("SourceFuel") || headerInfo[n].Name.Equals("SinkFuel") || headerInfo[n].Name.Equals("AMustTakeSum") || headerInfo[n].Name.Equals("AAvg") ||
                    headerInfo[n].Name.Equals("IncDec") || headerInfo[n].Name.Equals("YearlyDownSide") || headerInfo[n].Name.Equals("SourceNodeKey") || headerInfo[n].Name.Equals("SinkNodeKey") || headerInfo[n].Name.Equals("MarketDate") ||
                    headerInfo[n].Name.Equals("HoursCleared") || headerInfo[n].Name.Equals("WeeklyWin") || headerInfo[n].Name.Equals("StdDev") || headerInfo[n].Name.Equals("ConstraintText") || headerInfo[n].Name.Equals("ContingencyText") ||
                    headerInfo[n].Name.Equals("family") || headerInfo[n].Name.Equals("Sensitivity") || headerInfo[n].Name.Equals("WeeklySumValue") || headerInfo[n].Name.Equals("WeeklyMaxWin") || headerInfo[n].Name.Equals("WeeklyMaxLossRt") ||
                    headerInfo[n].Name.Equals("WeeklyPctWin") || headerInfo[n].Name.Equals("WeeklyCountCleared") || headerInfo[n].Name.Equals("AnnualMaxLossRt") || headerInfo[n].Name.Equals("MonthlyMaxLossRt") || headerInfo[n].Name.Equals("Sharpe") ||
                    headerInfo[n].Name.Equals("ADailyMustTakeMin") || headerInfo[n].Name.Equals("ASum") || headerInfo[n].Name.Equals("AMin") || headerInfo[n].Name.Equals("AMax") || headerInfo[n].Name.Equals("SumToMax") ||
                    headerInfo[n].Name.Equals("RiskReward") || headerInfo[n].Name.Equals("YearlyUpSide") || headerInfo[n].Name.Equals("YearlyRiskReward") || headerInfo[n].Name.Equals("AvgDA") || headerInfo[n].Name.Equals("CalcNumber") ||
                    headerInfo[n].Name.Equals("MustTakeSum") || headerInfo[n].Name.Equals("DailyMustTakeMin") || headerInfo[n].Name.Equals("AStdDev") || headerInfo[n].Name.Equals("AWinPct") || headerInfo[n].Name.Equals("AClearPct") ||
                    headerInfo[n].Name.Equals("DailyMin") || headerInfo[n].Name.Equals("DailyMax") || headerInfo[n].Name.Equals("DailyAvg") || headerInfo[n].Name.Equals("ADailyMin") || headerInfo[n].Name.Equals("ADailyMax") || headerInfo[n].Name.Equals("ADailyAvg"))
                        continue;
                if (type == "ErcotCorrelations" || type == "ErcotNegativeCorrelations")
                    if (headerInfo[n].Name.Equals("AnalysisTypeInt") || headerInfo[n].Name.Equals("MW") || headerInfo[n].Name.Equals("MaxWin") || headerInfo[n].Name.Equals("MaxLoss") ||
                    headerInfo[n].Name.Equals("Skew") || headerInfo[n].Name.Equals("Kurtosis") || headerInfo[n].Name.Equals("AMustTakeSum") || headerInfo[n].Name.Equals("AAvg") || headerInfo[n].Name.Equals("SourceNodeType") || headerInfo[n].Name.Equals("SinkNodeType") ||
                    headerInfo[n].Name.Equals("IncDec") || headerInfo[n].Name.Equals("YearlyDownSide") || headerInfo[n].Name.Equals("SourceNodeKey") || headerInfo[n].Name.Equals("SinkNodeKey") || headerInfo[n].Name.Equals("MarketDate") ||
                    headerInfo[n].Name.Equals("HoursCleared") || headerInfo[n].Name.Equals("WeeklyWin") || headerInfo[n].Name.Equals("StdDev") || headerInfo[n].Name.Equals("ConstraintText") || headerInfo[n].Name.Equals("ContingencyText") ||
                    headerInfo[n].Name.Equals("family") || headerInfo[n].Name.Equals("Sensitivity") || headerInfo[n].Name.Equals("WeeklySumValue") || headerInfo[n].Name.Equals("WeeklyMaxWin") || headerInfo[n].Name.Equals("WeeklyMaxLossRt") ||
                    headerInfo[n].Name.Equals("WeeklyPctWin") || headerInfo[n].Name.Equals("WeeklyCountCleared") || headerInfo[n].Name.Equals("AnnualMaxLossRt") || headerInfo[n].Name.Equals("MonthlyMaxLossRt") || headerInfo[n].Name.Equals("Sharpe") ||
                    headerInfo[n].Name.Equals("ADailyMustTakeMin") || headerInfo[n].Name.Equals("ASum") || headerInfo[n].Name.Equals("AMin") || headerInfo[n].Name.Equals("AMax") || headerInfo[n].Name.Equals("SumToMax") ||
                    headerInfo[n].Name.Equals("RiskReward") || headerInfo[n].Name.Equals("YearlyUpSide") || headerInfo[n].Name.Equals("YearlyRiskReward") || headerInfo[n].Name.Equals("AvgDA") || headerInfo[n].Name.Equals("CalcNumber") ||
                    headerInfo[n].Name.Equals("MustTakeSum") || headerInfo[n].Name.Equals("DailyMustTakeMin") || headerInfo[n].Name.Equals("AStdDev") || headerInfo[n].Name.Equals("AWinPct") || headerInfo[n].Name.Equals("AClearPct") ||
                    headerInfo[n].Name.Equals("DailyMin") || headerInfo[n].Name.Equals("DailyMax") || headerInfo[n].Name.Equals("DailyAvg") || headerInfo[n].Name.Equals("ADailyMin") || headerInfo[n].Name.Equals("ADailyMax") || headerInfo[n].Name.Equals("ADailyAvg"))
                        continue;
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
                MessageBox.Show(ex.Message.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

        #endregion
    }
}
