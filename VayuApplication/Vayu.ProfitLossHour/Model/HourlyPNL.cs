using System;
using System.Collections.Generic;
using System.Linq;

namespace Vayu.ProfitLossHour.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class HourlyPNL
    {
        #region  Prpperties
        /// <summary>
        /// The m hourly list
        /// </summary>
        private List<HourData> mHourlyList;

        /// <summary>
        /// Gets or sets the hourly list.
        /// </summary>
        /// <value>
        /// The hourly list.
        /// </value>
        public List<HourData> HourlyList
        {
            get
            {
                return mHourlyList;
            }
            set
            {
                mHourlyList = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the node.
        /// </summary>
        /// <value>
        /// The name of the node.
        /// </value>
        public string NodeName
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the hour1.
        /// </summary>
        /// <value>
        /// The hour1.
        /// </value>
        public HourData Hour1
        {
            get
            {
                return HourlyList[0];
            }
            set
            {
                HourlyList[0] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour2.
        /// </summary>
        /// <value>
        /// The hour2.
        /// </value>
        public HourData Hour2
        {
            get
            {
                return HourlyList[1];
            }
            set
            {
                HourlyList[1] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour3.
        /// </summary>
        /// <value>
        /// The hour3.
        /// </value>
        public HourData Hour3
        {
            get
            {
                return HourlyList[2];
            }
            set
            {
                HourlyList[2] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour4.
        /// </summary>
        /// <value>
        /// The hour4.
        /// </value>
        public HourData Hour4
        {
            get
            {
                return HourlyList[3];
            }
            set
            {
                HourlyList[3] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour5.
        /// </summary>
        /// <value>
        /// The hour5.
        /// </value>
        public HourData Hour5
        {
            get
            {
                return HourlyList[4];
            }
            set
            {
                HourlyList[4] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour6.
        /// </summary>
        /// <value>
        /// The hour6.
        /// </value>
        public HourData Hour6
        {
            get
            {
                return HourlyList[5];
            }
            set
            {
                HourlyList[5] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour7.
        /// </summary>
        /// <value>
        /// The hour7.
        /// </value>
        public HourData Hour7
        {
            get
            {
                return HourlyList[6];
            }
            set
            {
                HourlyList[6] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour8.
        /// </summary>
        /// <value>
        /// The hour8.
        /// </value>
        public HourData Hour8
        {
            get
            {
                return HourlyList[7];
            }
            set
            {
                HourlyList[7] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour9.
        /// </summary>
        /// <value>
        /// The hour9.
        /// </value>
        public HourData Hour9
        {
            get
            {
                return HourlyList[8];
            }
            set
            {
                HourlyList[8] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour10.
        /// </summary>
        /// <value>
        /// The hour10.
        /// </value>
        public HourData Hour10
        {
            get
            {
                return HourlyList[9];
            }
            set
            {
                HourlyList[9] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour11.
        /// </summary>
        /// <value>
        /// The hour11.
        /// </value>
        public HourData Hour11
        {
            get
            {
                return HourlyList[10];
            }
            set
            {
                HourlyList[10] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour12.
        /// </summary>
        /// <value>
        /// The hour12.
        /// </value>
        public HourData Hour12
        {
            get
            {
                return HourlyList[11];
            }
            set
            {
                HourlyList[11] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour13.
        /// </summary>
        /// <value>
        /// The hour13.
        /// </value>
        public HourData Hour13
        {
            get
            {
                return HourlyList[12];
            }
            set
            {
                HourlyList[12] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour14.
        /// </summary>
        /// <value>
        /// The hour14.
        /// </value>
        public HourData Hour14
        {
            get
            {
                return HourlyList[13];
            }
            set
            {
                HourlyList[13] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour15.
        /// </summary>
        /// <value>
        /// The hour15.
        /// </value>
        public HourData Hour15
        {
            get
            {
                return HourlyList[14];
            }
            set
            {
                HourlyList[14] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour16.
        /// </summary>
        /// <value>
        /// The hour16.
        /// </value>
        public HourData Hour16
        {
            get
            {
                return HourlyList[15];
            }
            set
            {
                HourlyList[15] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour17.
        /// </summary>
        /// <value>
        /// The hour17.
        /// </value>
        public HourData Hour17
        {
            get
            {
                return HourlyList[16];
            }
            set
            {
                HourlyList[16] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour18.
        /// </summary>
        /// <value>
        /// The hour18.
        /// </value>
        public HourData Hour18
        {
            get
            {
                return HourlyList[17];
            }
            set
            {
                HourlyList[17] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour19.
        /// </summary>
        /// <value>
        /// The hour19.
        /// </value>
        public HourData Hour19
        {
            get
            {
                return HourlyList[18];
            }
            set
            {
                HourlyList[18] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour20.
        /// </summary>
        /// <value>
        /// The hour20.
        /// </value>
        public HourData Hour20
        {
            get
            {
                return HourlyList[19];
            }
            set
            {
                HourlyList[19] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour21.
        /// </summary>
        /// <value>
        /// The hour21.
        /// </value>
        public HourData Hour21
        {
            get
            {
                return HourlyList[20];
            }
            set
            {
                HourlyList[20] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour22.
        /// </summary>
        /// <value>
        /// The hour22.
        /// </value>
        public HourData Hour22
        {
            get
            {
                return HourlyList[21];
            }
            set
            {
                HourlyList[21] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour23.
        /// </summary>
        /// <value>
        /// The hour23.
        /// </value>
        public HourData Hour23
        {
            get
            {
                return HourlyList[22];
            }
            set
            {
                HourlyList[22] = value;
            }
        }
        /// <summary>
        /// Gets or sets the hour24.
        /// </summary>
        /// <value>
        /// The hour24.
        /// </value>
        public HourData Hour24
        {
            get
            {
                return HourlyList[23];
            }
            set
            {
                HourlyList[23] = value;
            }
        }
        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        /// <value>
        /// The total.
        /// </value>
        public HourData Total
        {
            get
            {
                return HourlyList[24];
            }
            set
            {
                HourlyList[24] = value;
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="HourlyPNL"/> class.
        /// </summary>
        public HourlyPNL()
        {
            mHourlyList = new List<HourData>();
            for (int i = 0; i < 25; i++)
            {
                mHourlyList.Add(new HourData());
            }
        }
        /// <summary>
        /// Updates the total.
        /// </summary>
        public void UpdateTotal()
        {
            Total.ClearedMW = mHourlyList.Sum(x => x.ClearedMW);
            Total.PNL = mHourlyList.Sum(x => x.PNL);
        }
        /// <summary>
        /// Sorts the value.
        /// </summary>
        /// <param name="headerInfo">The header information.</param>
        /// <returns></returns>
        public object SortValue(string headerInfo)
        {
            if (string.IsNullOrEmpty(headerInfo))
            {
                return null;
            }
            if (headerInfo.Equals("node", StringComparison.InvariantCultureIgnoreCase))
            {
                return NodeName;
            }
            else if (headerInfo.Equals("", StringComparison.InvariantCultureIgnoreCase))
            {
                return Total.PNL;
            }
            else
            {
                string intiger = headerInfo.Trim('H', 'h', ' ');
                int index = 0;
                if (!int.TryParse(intiger, out index) || (index--) < 0)
                {
                    return null;
                }
                return mHourlyList[index].PNL;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class HourData
    {
        /// <summary>
        /// The m PNL
        /// </summary>
        private double? mPnl;
        /// <summary>
        /// Gets or sets the PNL.
        /// </summary>
        /// <value>
        /// The PNL.
        /// </value>
        public double? PNL
        {
            get
            {
                return mPnl;
            }
            set
            {
                if (double.IsNaN(value.GetValueOrDefault()))
                {
                    mPnl = 0;
                }
                else
                {
                    mPnl = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the cleared mw.
        /// </summary>
        /// <value>
        /// The cleared mw.
        /// </value>
        public double? ClearedMW { get; set; }
    }
}
