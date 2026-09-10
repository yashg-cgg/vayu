using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;

namespace Vayu.WorkbookStatistics.ViewModels
{
    public class CopyWindowViewModel : BindableBase
    {
        #region Declaration

        private MainWindowViewModel mParentModel;


        public DelegateCommand CopyCommand { private set; get; }

        private DateTime mSelectedDate;

        public DateTime SelectedDate
        {
            get
            {
                return mSelectedDate;
            }
            set
            {
                mSelectedDate = value;
                RaisePropertyChanged("SelectedDate");
            }
        }

        private bool mAllChecked;

        public bool AllChecked
        {
            get
            {
                return mAllChecked;
            }
            set
            {
                mAllChecked = value;
                RaisePropertyChanged("AllChecked");
            }
        }

        private bool mSelectedChecked;

        public bool SelectedChecked
        {
            get
            {
                return mSelectedChecked;
            }
            set
            {
                mSelectedChecked = value;
                RaisePropertyChanged("SelectedChecked");
            }
        }

        #endregion
        public CopyWindowViewModel(MainWindowViewModel parentModel)
        {
            mParentModel = parentModel;
            CopyCommand = new DelegateCommand(Copy);
            SelectedDate = DateTime.Today.AddDays(1);
        }
        public void SetChecked(bool isAll)
        {
            AllChecked = isAll;
            mSelectedChecked = !isAll;
        }
        private void Copy()
        {
            if (SelectedDate <= DateTime.Today)
            {
                MessageBox.Show("Date has to be greater than today");
                return;
            }
            mParentModel.CopyPaths(AllChecked, SelectedDate);
        }
    }
}
