using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Vayu.NodePriceGraph.ViewModels;

namespace Vayu.NodePriceGraph.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class NodeDetail : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the NodeDetail class.
        /// </summary>

        private Type myType;
        /// <summary>
        /// The double values
        /// </summary>
        private List<double?> doubleValues;
        /// <summary>
        /// Gets or sets the row head.
        /// </summary>
        /// <value>
        /// The row head.
        /// </value>
        public string RowHead { get; set; }
        /// <summary>
        /// Gets or sets the name of the path.
        /// </summary>
        /// <value>
        /// The name of the path.
        /// </value>
        public string PathName { get; set; }


        /// <summary>
        /// Gets the h1.
        /// </summary>
        /// <value>
        /// The h1.
        /// </value>
        public double? H1 { get { return doubleValues[0]; } }
        /// <summary>
        /// Gets the h2.
        /// </summary>
        /// <value>
        /// The h2.
        /// </value>
        public double? H2 { get { return doubleValues[1]; } }
        /// <summary>
        /// Gets the h3.
        /// </summary>
        /// <value>
        /// The h3.
        /// </value>
        public double? H3 { get { return doubleValues[2]; } }
        /// <summary>
        /// Gets the h4.
        /// </summary>
        /// <value>
        /// The h4.
        /// </value>
        public double? H4 { get { return doubleValues[3]; } }
        /// <summary>
        /// Gets the h5.
        /// </summary>
        /// <value>
        /// The h5.
        /// </value>
        public double? H5 { get { return doubleValues[4]; } }
        /// <summary>
        /// Gets the h6.
        /// </summary>
        /// <value>
        /// The h6.
        /// </value>
        public double? H6 { get { return doubleValues[5]; } }
        /// <summary>
        /// Gets the h7.
        /// </summary>
        /// <value>
        /// The h7.
        /// </value>
        public double? H7 { get { return doubleValues[6]; } }
        /// <summary>
        /// Gets the h8.
        /// </summary>
        /// <value>
        /// The h8.
        /// </value>
        public double? H8 { get { return doubleValues[7]; } }
        /// <summary>
        /// Gets the h9.
        /// </summary>
        /// <value>
        /// The h9.
        /// </value>
        public double? H9 { get { return doubleValues[8]; } }
        /// <summary>
        /// Gets the H10.
        /// </summary>
        /// <value>
        /// The H10.
        /// </value>
        public double? H10 { get { return doubleValues[9]; } }
        /// <summary>
        /// Gets the H11.
        /// </summary>
        /// <value>
        /// The H11.
        /// </value>
        public double? H11 { get { return doubleValues[10]; } }
        /// <summary>
        /// Gets the H12.
        /// </summary>
        /// <value>
        /// The H12.
        /// </value>
        public double? H12 { get { return doubleValues[11]; } }
        /// <summary>
        /// Gets the H13.
        /// </summary>
        /// <value>
        /// The H13.
        /// </value>
        public double? H13 { get { return doubleValues[12]; } }
        /// <summary>
        /// Gets the H14.
        /// </summary>
        /// <value>
        /// The H14.
        /// </value>
        public double? H14 { get { return doubleValues[13]; } }
        /// <summary>
        /// Gets the H15.
        /// </summary>
        /// <value>
        /// The H15.
        /// </value>
        public double? H15 { get { return doubleValues[14]; } }
        /// <summary>
        /// Gets the H16.
        /// </summary>
        /// <value>
        /// The H16.
        /// </value>
        public double? H16 { get { return doubleValues[15]; } }
        /// <summary>
        /// Gets the H17.
        /// </summary>
        /// <value>
        /// The H17.
        /// </value>
        public double? H17 { get { return doubleValues[16]; } }
        /// <summary>
        /// Gets the H18.
        /// </summary>
        /// <value>
        /// The H18.
        /// </value>
        public double? H18 { get { return doubleValues[17]; } }
        /// <summary>
        /// Gets the H19.
        /// </summary>
        /// <value>
        /// The H19.
        /// </value>
        public double? H19 { get { return doubleValues[18]; } }
        /// <summary>
        /// Gets the H20.
        /// </summary>
        /// <value>
        /// The H20.
        /// </value>
        public double? H20 { get { return doubleValues[19]; } }
        /// <summary>
        /// Gets the H21.
        /// </summary>
        /// <value>
        /// The H21.
        /// </value>
        public double? H21 { get { return doubleValues[20]; } }
        /// <summary>
        /// Gets the H22.
        /// </summary>
        /// <value>
        /// The H22.
        /// </value>
        public double? H22 { get { return doubleValues[21]; } }
        /// <summary>
        /// Gets the H23.
        /// </summary>
        /// <value>
        /// The H23.
        /// </value>
        public double? H23 { get { return doubleValues[22]; } }
        /// <summary>
        /// Gets the H24.
        /// </summary>
        /// <value>
        /// The H24.
        /// </value>
        public double? H24
        {
            get { return doubleValues[23]; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeDetail"/> class.
        /// </summary>
        public NodeDetail()
        {
            doubleValues = new List<double?>();
            for (int i = 0; i < 24; i++)
                doubleValues.Add(null);

            myType = this.GetType();
        }

        /// <summary>
        /// Assigns the value.
        /// </summary>
        /// <param name="hourIndex">Index of the hour.</param>
        /// <param name="dValue">The d value.</param>
        public void AssignValue(int hourIndex, double dValue)
        {
            if (hourIndex < 0 || hourIndex > 23)
                return;

            doubleValues[hourIndex] = Math.Round(dValue, 2);
            RaisePropertyChanged("H" + hourIndex);
        }

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propName">Name of the property.</param>
        private void RaisePropertyChanged([CallerMemberName] string propName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Collections.ObjectModel.ObservableCollection{Vayu.NodePriceGraph.ViewModel.NodeDetail}" />
    public class NodeDetailList : ObservableCollection<NodeDetail>
    {
        /// <summary>
        /// Builds the test.
        /// </summary>
        public void BuildTest()
        {
            NodeDetail p1 = new NodeDetail();
            //NodeDetail detail1 = new NodeDetail();
            p1.RowHead = "Row1";
            //p1.H1 = 2.5;
            //p1.H2 = 3.5;
            this.Add(p1);

            NodeDetail p2 = new NodeDetail();
            //NodeDetail detail1 = new NodeDetail();
            p2.RowHead = "Row2";
            //p2.H1 = 4.5;
            //p2.H2 = 5.5;
            this.Add(p2);
        }

        /// <summary>
        /// Fills the by item list.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="itemList">The item list.</param>
        /// <param name="rowHead">The row head.</param>
        /// <param name="pathName">Name of the path.</param>
        public void FillByItemList(DateTime date, IEnumerable<ChartItem> itemList, string rowHead, string pathName)
        {
            if (itemList == null || itemList.Count() == 0)
                return;

            int maxX = itemList.Max(x => x.X);
            NodeDetail detail = new NodeDetail();
            detail.RowHead = rowHead;
            detail.PathName = pathName;

            foreach (var item in itemList)
                detail.AssignValue(item.X, item.Y);

            this.Add(detail);
        }
    }
}
