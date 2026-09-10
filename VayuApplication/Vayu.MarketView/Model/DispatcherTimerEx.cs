using System;
using System.Windows.Threading;

namespace Vayu.MarketView.Model
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Threading.DispatcherTimer" />
    public class DispatcherTimerEx : DispatcherTimer
    {
        #region Declaration

        /// <summary>
        /// The event handler
        /// </summary>
        private EventHandler eventHandler;
        /// <summary>
        /// The subscribed
        /// </summary>
        private bool subscribed;
        /// <summary>
        /// The fast beat
        /// </summary>
        private TimeSpan fastBeat;
        /// <summary>
        /// The normal beat
        /// </summary>
        private TimeSpan normalBeat;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [shutdown in progress].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [shutdown in progress]; otherwise, <c>false</c>.
        /// </value>
        public bool ShutdownInProgress { get; set; }

        /// <summary>
        /// Gets or sets the market key.
        /// </summary>
        /// <value>
        /// The market key.
        /// </value>
        public int MarketKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [re run].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [re run]; otherwise, <c>false</c>.
        /// </value>
        public bool ReRun { get; set; }

        /// <summary>
        /// Gets or sets the reconnect count.
        /// </summary>
        /// <value>
        /// The reconnect count.
        /// </value>
        public int ReconnectCount { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DispatcherTimerEx"/> is subscribed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if subscribed; otherwise, <c>false</c>.
        /// </value>
        public bool Subscribed
        {
            get { return subscribed; }
            set
            {
                subscribed = value;
                if (subscribed)
                    this.Interval = normalBeat;
                else
                    this.Interval = fastBeat;
            }
        }

        /// <summary>
        /// Shutdowns this instance.
        /// </summary>
        public void Shutdown()
        {
            this.Dispatcher.Thread.IsBackground = true;

            if (eventHandler != null)
                this.Tick -= eventHandler;

            this.Stop();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherTimerEx"/> class.
        /// </summary>
        public DispatcherTimerEx()
            : base()
        {
            this.Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
            this.Dispatcher.ShutdownFinished += Dispatcher_ShutdownFinished;
            this.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            this.Dispatcher.UnhandledExceptionFilter += Dispatcher_UnhandledExceptionFilter;
            fastBeat = new TimeSpan(0, 0, 20);
            normalBeat = new TimeSpan(0, 1, 30);
            this.Interval = fastBeat;
        }

        #region Events

        /// <summary>
        /// Handles the UnhandledExceptionFilter event of the Dispatcher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DispatcherUnhandledExceptionFilterEventArgs"/> instance containing the event data.</param>
        void Dispatcher_UnhandledExceptionFilter(object sender, DispatcherUnhandledExceptionFilterEventArgs e)
        {
            e.RequestCatch = false;
        }

        /// <summary>
        /// Handles the UnhandledException event of the Dispatcher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DispatcherUnhandledExceptionEventArgs"/> instance containing the event data.</param>
        void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            //e.RequestCatch = false;
        }

        /// <summary>
        /// Handles the ShutdownFinished event of the Dispatcher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void Dispatcher_ShutdownFinished(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Handles the ShutdownStarted event of the Dispatcher control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Creates the specified event handler.
        /// </summary>
        /// <param name="eventHandler">The event handler.</param>
        /// <returns></returns>
        public static DispatcherTimerEx Create(EventHandler eventHandler)
        {
            DispatcherTimerEx timer = new DispatcherTimerEx();
            timer.eventHandler = eventHandler;
            timer.Tick += eventHandler;
            timer.Start();
            return timer;
        }

        #endregion
    }
}
