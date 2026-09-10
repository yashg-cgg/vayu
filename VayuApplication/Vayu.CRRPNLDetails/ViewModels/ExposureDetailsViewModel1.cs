using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Vayu.CRRPNLDetails.Model;
using Vayu.CRRPNLDetails.Views;

namespace Vayu.CRRPNLDetails.ViewModels
{
    public class ExposureDetailsViewModel1 : BindableBase
    {
        private List<Model.FTRExposure> _FTRExposureList;
        public DelegateCommand RunPathDetailsCommand { private set; get; }

        public DelegateCommand RunHistoricalConstOpenCmd { private set; get; }
        Dictionary<int, List<PathExposure>> pathExposureDict;

        private FTRExposure _SelectedConstraintPathValue;

        public FTRExposure SelectedConstraintPathValue
        {
            get { return _SelectedConstraintPathValue; }
            set
            {
                _SelectedConstraintPathValue = value;
                RaisePropertyChanged("SelectedConstraintPathValue");
            }
        }


        public List<Model.FTRExposure> FTRExposureList
        {
            get { return _FTRExposureList; }
            set
            {
                _FTRExposureList = value;
                RaisePropertyChanged("FTRExposureList");
            }
        }

        /// <summary>
        /// Initializes a new instance of the ExposureDetailsViewModel1 class.
        /// </summary>
        public ExposureDetailsViewModel1(MainWindowViewModel parentViewModel, List<Model.FTRDetailsData> FTRDetailsDataList)
        {
            FTRExposureList = CalculateExposure(FTRDetailsDataList);
            RunPathDetailsCommand = new DelegateCommand(PathDetailsCommand);

            RunHistoricalConstOpenCmd = new DelegateCommand(OpenHistoricalConstraints);
        }

        private List<FTRExposure> CalculateExposure(List<Model.FTRDetailsData> FTRDetailsDataList)
        {

            Dictionary<int, string> nodeDict = Vayu.DBLibrary.DBAccess.GetNodeFromNames(9);
            List<string> nodeList = FTRDetailsDataList.Select(a => a.Source).ToList();
            DataService ds = new DataService();
            DateTime startDate = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
            DateTime endDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month + 1, 1);
            endDate = endDate.AddDays(-1);
            PeriodHours hours = ds.GetPeriodHours(startDate, endDate, 1);
            nodeList.AddRange(FTRDetailsDataList.Select(a => a.Sink).ToList());
            nodeList = nodeList.Distinct().ToList();
            List<int> nodeKeyList = new List<int>();
            foreach (string node in nodeList)
            {
                if (nodeDict.ContainsValue(node))
                {
                    int nodeKey = nodeDict.FirstOrDefault(a => a.Value == node).Key;
                    if (!nodeKeyList.Contains(nodeKey))
                        nodeKeyList.Add(nodeKey);
                }
            }
            if (nodeKeyList == null || nodeKeyList.Count == 0)
                return null;
            Dictionary<int, Dictionary<string, List<ExposureHelper>>> exposureDict = GetExposures(nodeKeyList);
            List<FTRExposure> exposureList = new List<FTRExposure>();
            pathExposureDict = new Dictionary<int, List<PathExposure>>();
            foreach (var constrItem in exposureDict)
            {

                FTRExposure exposure = new FTRExposure();
                int constraintId = constrItem.Key;
                Dictionary<string, List<ExposureHelper>> constraintInfoDict = constrItem.Value;
                string[] constStringArr = constraintInfoDict.Keys.ToArray();
                string constString = constStringArr[0];
                string[] constArr = constString.Split('?');
                string constraint = constArr[0];
                string contingency = constArr[1];
                List<ExposureHelper> exposureDetailList = constraintInfoDict[constString];
                double MWExposure = 0; double dollarExposure = 0; double PeakMWExposure = 0; double OffPeakMWExposure = 0; double PeakDollarExposure = 0; double OffPeakDollarExposure = 0;
                foreach (FTRDetailsData item in FTRDetailsDataList)
                {
                    List<PathExposure> pathExpList;
                    if (pathExposureDict.ContainsKey(constraintId))
                    {
                        pathExpList = pathExposureDict[constraintId];
                    }
                    else
                        pathExpList = new List<PathExposure>();
                    PathExposure pathExp = new PathExposure();
                    if (item.Source == null || item.Sink == null)
                        continue;
                    int sourcenodeKey = nodeDict.FirstOrDefault(a => a.Value == item.Source).Key;
                    int sinknodeKey = nodeDict.FirstOrDefault(a => a.Value == item.Sink).Key;
                    bool sourceExpExits = exposureDetailList.Exists(a => a.nodeKey == sourcenodeKey);
                    bool sinkExpExists = exposureDetailList.Exists(a => a.nodeKey == sinknodeKey);
                    if (!sourceExpExits || !sinkExpExists)
                        continue;
                    double pathMWExposure = 0.0; double pathDollarExposure = 0.0;
                    ExposureHelper sourceExposure = exposureDetailList.First(a => a.nodeKey == sourcenodeKey);
                    ExposureHelper sinkExposure = exposureDetailList.First(a => a.nodeKey == sinknodeKey);
                    if (item.ClassType.ToLower() == "peak" || item.ClassType.ToLower() == "onpeak")
                    {
                        pathMWExposure = (sinkExposure.Sensitivity - sourceExposure.Sensitivity) * item.MWTotal * sourceExposure.ShiftFactor; //* hours.peakHours;
                        pathDollarExposure = (sinkExposure.Sensitivity - sourceExposure.Sensitivity) * item.MWTotal * sourceExposure.ShiftFactor * sourceExposure.DollarImpact * hours.peakHours;
                        MWExposure += pathMWExposure;
                        dollarExposure += pathDollarExposure;
                        PeakMWExposure += pathMWExposure;
                        PeakDollarExposure += pathDollarExposure;

                    }
                    else if (item.ClassType.ToLower() == "off-peak")
                    {
                        pathMWExposure = (sinkExposure.Sensitivity - sourceExposure.Sensitivity) * item.MWTotal * sourceExposure.ShiftFactor;//* hours.offpeakHours;
                        pathDollarExposure = (sinkExposure.Sensitivity - sourceExposure.Sensitivity) * item.MWTotal * sourceExposure.ShiftFactor * sourceExposure.DollarImpact * hours.offpeakHours;
                        MWExposure += pathMWExposure;
                        dollarExposure += pathDollarExposure;
                        OffPeakMWExposure += pathMWExposure;
                        OffPeakDollarExposure += pathDollarExposure;
                    }
                    pathExp.Source = item.Source;
                    pathExp.Sink = item.Sink;
                    pathExp.ClassType = item.ClassType;
                    pathExp.MW = item.MWTotal;
                    pathExp.TotalMWExposure = pathMWExposure;
                    pathExp.TotalDollarExposure = pathDollarExposure;
                    pathExp.Participant = item.Participant;
                    pathExp.Auction = item.Auction;

                    if (!pathExposureDict.ContainsKey(constraintId))
                    {
                        pathExpList.Add(pathExp);
                        pathExposureDict.Add(constraintId, pathExpList);
                    }
                    else
                    {
                        bool exists = pathExpList.Exists(a => a.Source == pathExp.Source && a.Sink == pathExp.Sink && a.ClassType == pathExp.ClassType && a.Participant == pathExp.Participant && a.MW == pathExp.MW && item.Auction == pathExp.Auction && item.PeriodType == pathExp.Period);
                        if (exists)
                        {
                            PathExposure existingExp = pathExpList.Find(a => a.Source == pathExp.Source && a.Sink == pathExp.Sink && a.ClassType == pathExp.ClassType && a.Participant == pathExp.Participant && a.MW == pathExp.MW && item.Auction == pathExp.Auction && item.PeriodType == pathExp.Period);
                            existingExp.TotalDollarExposure += pathDollarExposure;
                            existingExp.TotalMWExposure += pathMWExposure; ;
                        }
                        else
                            pathExpList.Add(pathExp);
                        pathExposureDict[constraintId] = pathExpList;
                    }


                }
                exposure.ConstraintId = constraintId;
                exposure.Constraint = constraint;
                exposure.Contingency = contingency;
                exposure.TotalMWExposure = MWExposure;
                exposure.TotalDollarExposure = dollarExposure;
                exposure.PeakMWExposure = PeakMWExposure;
                exposure.OffPeakMWExposure = OffPeakMWExposure;
                exposure.PeakDollarExposure = PeakDollarExposure;
                exposure.OffPeakDollarExposure = OffPeakDollarExposure;
                exposureList.Add(exposure);
            }

            return exposureList;
        }
        public void PathDetailsCommand()
        {
            List<PathExposure> pathList = pathExposureDict[SelectedConstraintPathValue.ConstraintId];
            PathExposureDetails pathDetail = new PathExposureDetails();
            PathDetailViewModel pathDetailViewModel = new PathDetailViewModel(this, pathList);
            pathDetail.DataContext = pathDetailViewModel;
            pathDetail.ResizeMode = ResizeMode.CanResize;
            pathDetail.Show();// .ShowDialog();
        }

        private Dictionary<int, Dictionary<string, List<ExposureHelper>>> GetExposures(List<int> nodeList)
        {

            DataService ds = new DataService();
            Dictionary<int, Dictionary<string, List<ExposureHelper>>> exposureDict = ds.GetExposures(nodeList);
            return exposureDict;
        }

        private void OpenHistoricalConstraints()
        {
            Vayu.ConstraintContingencyHistory.Views.MainWindow window = new ConstraintContingencyHistory.Views.MainWindow();
            var datacontext = new Vayu.ConstraintContingencyHistory.ViewModels.MainWindowViewModel(new Vayu.ConstraintContingencyHistory.Model.DataService());
            window.DataContext = datacontext;
            datacontext.ShowHistoricalConstraintForExposure(SelectedConstraintPathValue.Constraint, SelectedConstraintPathValue.Contingency, SelectedConstraintPathValue.ConstraintId, 1);
            window.Show();
        }


    }
}
