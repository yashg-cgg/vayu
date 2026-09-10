using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using Vayu.CRRPNLDetails.Model;

namespace Vayu.CRRPNLDetails.ViewModels
{
    public class FTRFilterViewModel : BindableBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets the move paticipant command.
        /// </summary>
        /// <value>
        /// The move paticipant command.
        /// </value>
        public DelegateCommand MovePaticipantCommand { private set; get; }
        /// <summary>
        /// Gets or sets the delete participant command.
        /// </summary>
        /// <value>
        /// The delete participant command.
        /// </value>
        public DelegateCommand DeleteParticipantCommand { private set; get; }
        /// <summary>
        /// Gets or sets the combine trades command.
        /// </summary>
        /// <value>
        /// The combine trades command.
        /// </value>
        public DelegateCommand CombineTradesCommand { private set; get; }

        /// <summary>
        /// Gets or sets the temporary selected.
        /// </summary>
        /// <value>
        /// The temporary selected.
        /// </value>
        public List<string> tempSelected { get; set; }
        /// <summary>
        /// Gets or sets the temporary deleted.
        /// </summary>
        /// <value>
        /// The temporary deleted.
        /// </value>
        public List<string> tempDeleted { get; set; }
        /// <summary>
        /// The m combined parameter list
        /// </summary>
        private List<string> mCombinedParameterList;
        /// <summary>
        /// Gets or sets the combined parameter list.
        /// </summary>
        /// <value>
        /// The combined parameter list.
        /// </value>
        public List<string> CombinedParameterList
        {
            get { return mCombinedParameterList; }
            set
            {
                mCombinedParameterList = value;
                RaisePropertyChanged("CombinedParameterList");
            }
        }
        /// <summary>
        /// The m selected list
        /// </summary>
        private List<string> mSelectedList;
        /// <summary>
        /// Gets or sets the selected list.
        /// </summary>
        /// <value>
        /// The selected list.
        /// </value>
        public List<string> SelectedList
        {
            get { return mSelectedList; }
            set
            {
                mSelectedList = value;
                RaisePropertyChanged("SelectedList");
            }
        }
        /// <summary>
        /// The m parent model
        /// </summary>
        private Vayu.CRRPNLDetails.ViewModels.MainWindowViewModel mParentModel;
        /// <summary>
        /// Gets or sets the parent model.
        /// </summary>
        /// <value>
        /// The parent model.
        /// </value>
        public Vayu.CRRPNLDetails.ViewModels.MainWindowViewModel ParentModel
        {
            get
            {
                return mParentModel;
            }
            set
            {
                mParentModel = value;
                RaisePropertyChanged("ParentModel");
            }
        }

        #endregion

        /// <summary>
        /// The data service
        /// </summary>
        private IDataService dataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FTRFilterViewModel"/> class.
        /// </summary>
        /// <param name="dataService">The data service.</param>
        /// <param name="columns">The columns.</param>
        public FTRFilterViewModel(IDataService dataService, List<string> columns)
        {
            CombinedParameterList = columns.ToList();
            MovePaticipantCommand = new DelegateCommand(MoveParticipants);
            DeleteParticipantCommand = new DelegateCommand(DeleteParticipants);
            CombineTradesCommand = new DelegateCommand(Combine);
        }

        /// <summary>
        /// Moves the participants.
        /// </summary>
        private void MoveParticipants()
        {
            if (tempSelected != null)
            {
                if (tempSelected.Count > 0)
                {
                    SelectedList = tempSelected.ToList();
                }

            }
        }
        /// <summary>
        /// Deletes the participants.
        /// </summary>
        private void DeleteParticipants()
        {
            if (tempDeleted != null)
            {
                if (tempDeleted.Count > 0)
                {
                    List<string> tempList = new List<string>();
                    foreach (var item in SelectedList)
                    {
                        tempList.Add(item);
                    }
                    foreach (var item in tempDeleted)
                    {
                        if (tempList.Contains(item))
                        {
                            tempList.Remove(item);
                        }
                    }

                    SelectedList = tempList.ToList();
                }
                else
                {
                    SelectedList = null;
                }

            }
            else
            {
                SelectedList = null;
            }
        }
        /// <summary>
        /// Combines Parent Model.
        /// </summary>
        private void Combine()
        {
            List<string> tempSelectedList = new List<string>();
            tempSelectedList = SelectedList;
            if (tempSelectedList != null)
            {
                if (tempSelectedList.Count > 0)
                {
                    ParentModel.Combined(tempSelectedList.ToList());
                }
            }
        }

        /// <summary>
        /// Selected participants.
        /// </summary>
        /// <param name="selectedItems">The selected items.</param>
        internal void SelectedParticipant(List<string> selectedItems)
        {
            tempSelected = selectedItems.ToList();
        }
        /// <summary>
        /// Deletes the dates.
        /// </summary>
        /// <param name="selectedItems">The selected items.</param>
        internal void DeleteDates(List<string> selectedItems)
        {
            tempDeleted = selectedItems.ToList();
        }
    }
}
