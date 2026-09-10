using Vayu.CommonControls;

namespace Vayu.MarketViewNameSpace.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Vayu.ZoneInfo" />
    public class ZoneInfoUI : ZoneInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZoneInfoUI"/> class.
        /// </summary>
        public ZoneInfoUI() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoneInfoUI"/> class.
        /// </summary>
        /// <param name="inf">The inf.</param>
        public ZoneInfoUI(ZoneInfo inf)
        {
            this.IsSelected = inf.IsSelected;
            this.Name = inf.Name;
            this.StringColor = inf.StringColor;
            this.latitude = inf.latitude;
        }

        //public event EventHandler SelectionChanged;

        //protected override void OnSelectionChanged()
        //{
        //    if (SelectionChanged != null)
        //        SelectionChanged(this, new EventArgs());
        //}

        //public static ObservableCollection<ZoneInfoUI> GetZoneList(int marketKey)
        //{
        //    List<ZoneInfo> zlist = ZoneModel.GetZoneList(marketKey);
        //    //ZoneList = new ObservableCollection<ZoneInfoUI>(zlist.Select(x=>new ZoneInfoUI()));
        //}
    }
}
