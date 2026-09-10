using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using Vayu.WorkbookStatistics.Model;

namespace Vayu.WorkbookStatistics.ViewModels
{
    public class PathErrorDialogViewModel : BindableBase
    {
        #region Declaration

        private Dictionary<string, Dictionary<string, Tuple<DateTime, double>>> mErrorHash;

        private Path mPath;

        private List<Path> mPathList;

        private List<string> mOverrideList;

        public DelegateCommand OverrideCommand { private set; get; }

        private List<PathError> mPathErrorList;

        public List<PathError> PathErrorList
        {
            get
            {
                return mPathErrorList;
            }
            set
            {
                mPathErrorList = value;
                RaisePropertyChanged("PathErrorList");
            }
        }

        #endregion
        public PathErrorDialogViewModel(Path path, Dictionary<string, Dictionary<string, Tuple<DateTime, double>>> errorHash,
                                    List<string> overrideList, List<Path> pathList)
        {
            OverrideCommand = new DelegateCommand(Override);
            mErrorHash = errorHash;
            mPath = path;
            mOverrideList = overrideList;
            mPathList = pathList;
            FillErrorList();
        }
        public void Override()
        {
            List<string> keys = mErrorHash.Keys.ToList<string>();
            List<string> removeKeyList = new List<string>();
            foreach (Path path in mPathList)
            {
                if (path.Sink == null && path.Source == mPath.Source)
                {
                    path.RiskPath = false;
                }
                if (path.Sink != null && path.Source == mPath.Source && path.Sink == mPath.Sink)
                {
                    path.RiskPath = false;
                }
            }
            string sourceSink = mPath.Sink == null ? mPath.Source : mPath.Source + "->" + mPath.Sink;
            foreach (string key in keys)
            {
                Dictionary<string, Tuple<DateTime, double>> pathHash = mErrorHash[key];
                if (pathHash.ContainsKey(sourceSink))
                {
                    pathHash.Remove(sourceSink);
                }
                if (pathHash.Count == 0)
                {
                    removeKeyList.Add(key);
                }
            }
            mOverrideList.Add(sourceSink);
            foreach (string key in removeKeyList)
            {
                mErrorHash.Remove(key);
            }
        }

        private void FillErrorList()
        {
            List<PathError> pathErrorList = new List<PathError>();
            List<string> keys = mErrorHash.Keys.ToList<string>();
            foreach (string key in keys)
            {
                Dictionary<string, Tuple<DateTime, double>> pathHash = mErrorHash[key];
                string sourceSink = mPath.Sink == null ? mPath.Source : mPath.Source + "->" + mPath.Sink;
                if (pathHash.ContainsKey(sourceSink))
                {
                    Tuple<DateTime, double> tuple = pathHash[sourceSink];
                    PathError pathError = new PathError();
                    pathError.Days = key;
                    pathError.AsBid = tuple.Item2;
                    pathError.Date = tuple.Item1;
                    pathErrorList.Add(pathError);
                }
            }
            PathErrorList = null;
            PathErrorList = pathErrorList;
        }
    }
}
