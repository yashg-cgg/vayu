using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Vayu.PowerMap.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    public partial class MainWindow
    {
        /// <summary>
        /// The can execute play
        /// </summary>
        private bool canExecutePlay = true;
        /// <summary>
        /// The hour
        /// </summary>
        private int hour = 0;
        // private Task PlayTask;

        /// <summary>
        /// Resets the state.
        /// </summary>
        private void ResetState()
        {
            StopButton.IsEnabled = true;
            PlayButton.IsEnabled = true;
            PauseButton.IsEnabled = true;
        }
        /// <summary>
        /// Plays the specified sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Play(object sender, RoutedEventArgs e)
        {
            if (!canExecutePlay)
                return;

            if (!StopButton.IsEnabled)
            {
                ResetState();
                return;
            }

            Task t1 = null;
            PlayButton.IsEnabled = false;
            PauseButton.IsEnabled = true;

            if (PauseButton.IsEnabled)
            {
                try
                {
                    canExecutePlay = false;
                    hour++;
                    PlayHourleyData(hour);
                    FlyTo(mMarketLocation[mMarketInContext], 6);
                    t1 = Task.Run(() => Thread.Sleep(10000));
                    await t1;
                }
                catch { }
                finally
                {
                    canExecutePlay = true;
                }
            }

            if (hour < 25)
            {
                if (PauseButton.IsEnabled == true)
                {
                    Play(null, null);
                }
                else
                {
                    PlayButton.IsEnabled = true;
                }
            }
            else
            {
                ResetState();
                hour = 0;
            }
        }

        /// <summary>
        /// Plays the hourley data.
        /// </summary>
        /// <param name="i">The i.</param>
        private void PlayHourleyData(int i)
        {
            EnableEvents = false;
            InitParameter(i);
            LiveCheckBox.IsChecked = false;
            EnableEvents = true;
            MainCalendarSilder.HE = i;
        }

        /// <summary>
        /// Pauses the specified sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Pause(object sender, RoutedEventArgs e)
        {
            PauseButton.IsEnabled = false;
            PlayButton.IsEnabled = true;
        }

        /// <summary>
        /// Stops the specified sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Stop(object sender, RoutedEventArgs e)
        {
            StopButton.IsEnabled = false;
            PlayButton.IsEnabled = true;
        }
    }
}
