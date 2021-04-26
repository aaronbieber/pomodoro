using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pomodoro
{
    public partial class MainWindow : Window
    {
        enum Mode
        {
            Working,
            Resting
        }

        enum State
        {
            Running,
            Paused
        }

        private Mode mode = Mode.Working;
        private State state = State.Paused;
        private int remainingSeconds = Properties.Settings.Default.workDuration * 60;
        private Timer timer;
        private Dictionary<Mode, int> timeLengths = new Dictionary<Mode, int>();
        private Dictionary<Mode, String> modeNames = new Dictionary<Mode, string>();
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private SoundPlayer dingPlayer = new SoundPlayer(Properties.Resources.ding);

        private void initSettings()
        {
            timeLengths[Mode.Working] = Properties.Settings.Default.workDuration;
            timeLengths[Mode.Resting] = Properties.Settings.Default.breakDuration;

            modeNames[Mode.Working] = "POMODORO";
            modeNames[Mode.Resting] = "BREAK";
        }

        public MainWindow()
        {
            InitializeComponent();
            initSettings();

            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += timer_Tick;

            Properties.Settings.Default.SettingsSaving += Default_SettingsSaving;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            updateForm();
            reset();

            uiSettingsButton.PreviewKeyDown += HandleSpaceKey;
            uiSwitchButton.PreviewKeyDown += HandleSpaceKey;
            uiResetButton.PreviewKeyDown += HandleSpaceKey;
        }

        private void HandleSpaceKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                startStop();
                e.Handled = true;
            }
        }

        private void Default_SettingsSaving(object sender, System.ComponentModel.CancelEventArgs e)
        {
            initSettings();
            reset();
        }

        private void start()
        {
            state = State.Running;
            timer.Start();
            updateForm();
        }

        private void stop()
        {
            state = State.Paused;
            timer.Stop();
            updateForm();
        }

        private void startStop()
        {
            if (state == State.Paused)
            {
                start();
            } 
            else if (state == State.Running)
            {
                stop();
            }
            updateForm();
        }

        private void reset()
        {
            remainingSeconds = timeLengths[mode] * 60;
            updateForm();
        }

        private void updateForm()
        {
            Dispatcher.Invoke(() =>
            {
                if (state == State.Running)
                {
                    uiTime.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                    uiModeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
                }
                else if (state == State.Paused)
                {
                    uiTime.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
                    uiModeText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Gray"));
                }

                TimeSpan remainingTime = new TimeSpan(0, 0, remainingSeconds);
                if (remainingSeconds > 3600)
                {
                    uiTime.Text = remainingTime.ToString(@"hh\:mm\:ss");
                }
                else
                {
                    uiTime.Text = remainingTime.ToString(@"mm\:ss");
                }
                uiModeText.Text = modeNames[mode].ToString();
            });

        }

        private void timer_Tick(object sender, ElapsedEventArgs e)
        {
            remainingSeconds -= 1;
            if (remainingSeconds < 1)
            {
                if (mode == Mode.Working)
                {
                    mode = Mode.Resting;
                    reset();
                } 
                else if (mode == Mode.Resting)
                {
                    mode = Mode.Working;
                    reset();
                }
                dingPlayer.Play();
            }
            updateForm();

        }

        private void uiModeText_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Received click.");
        }

        private void Start_Stop_Click(object sender, RoutedEventArgs e)
        {
            startStop();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            stop();
            reset();
        }

        private void Switch_Click(object sender, RoutedEventArgs e)
        {
            var restart = state == State.Running;
            if (mode == Mode.Working)
            {
                mode = Mode.Resting;
                reset();
                if (restart) start();
            }
            else if (mode == Mode.Resting)
            {
                mode = Mode.Working;
                reset();
                if (restart) start();
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    startStop();
                    break;

                case Key.Escape:
                    var choice = MessageBox.Show("Really start over?",
                        "Are you sure?",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.No);

                    if (choice == MessageBoxResult.Yes)
                    {
                        stop();
                        reset();
                    }
                    break;

                default:
                    // Do nothing.
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // https://stackoverflow.com/questions/57654546/taskcanceledexception-after-closing-window
            // Hopefully force the UI update timer thread to end without throwing an exception about that
            // task getting canceled.
            Hide();
            Environment.Exit(0);
        }
    }
}
