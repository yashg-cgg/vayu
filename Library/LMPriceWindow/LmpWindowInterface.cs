using System;
using System.Collections.Generic;


namespace Vayu.LMPriceWindow
{
    /// <summary>
    /// 
    /// </summary>
    public interface LmpWindowInterface
    {
        /// <summary>
        /// Shows the LMP graphs.
        /// </summary>
        /// <param name="day">The day.</param>
        void ShowLMPGraphs(string day);
        /// <summary>
        /// Shows the node analyzer.
        /// </summary>
        void ShowNodeAnalyzer();
        /// <summary>
        /// Sets the source sinks.
        /// </summary>
        /// <param name="sourceSinkList">The source sink list.</param>
        void SetSourceSinks(List<Tuple<string, string>> sourceSinkList);
    }
}
