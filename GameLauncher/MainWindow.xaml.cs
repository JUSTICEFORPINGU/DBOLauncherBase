using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace GameLauncher
{
    enum LauncherStatus
    {
        ready,
        updated,
        failed,
        downloadingGame,
        downloadingUpdate
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string rootPath;
        private string versionFile;
        private string gameZip;
        private string gameExe;

        private LauncherStatus _status;
        internal LauncherStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case LauncherStatus.ready:
                        PlayButton_Copy.Content = "Buscar Actualizacion";
                        break;
                    case LauncherStatus.updated:
                        PlayButton_Copy.Content = "Actualizado";
                        break;
                    case LauncherStatus.failed:
                        PlayButton_Copy.Content = "Error Al Actualizar";
                        break;
                    case LauncherStatus.downloadingGame:
                        PlayButton_Copy.Content = "Descargando Actualizacion";
                        break;
                    case LauncherStatus.downloadingUpdate:
                        PlayButton_Copy.Content = "Descargando Version";
                        break;
                    default:
                        break;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(MainWindow_Loaded);

            rootPath = Directory.GetCurrentDirectory();
            versionFile = Path.Combine(rootPath, "Version.txt");
            gameZip = Path.Combine(rootPath, "att.zip");
            gameExe = Path.Combine(rootPath, "DBO.exe");
        }

        private void myMediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            MediaElement _element = sender as MediaElement;
            _element.Play();
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            myMediaElement.Play();
        }

        private void CheckForUpdates()
        {
            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = localVersion.ToString();

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("Export link version .txt"));

                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        InstallGameFiles(true, onlineVersion);
                    }
                    else
                    {
                        Status = LauncherStatus.ready;
                    }
                }
                catch (Exception ex)
                {
                    Status = LauncherStatus.failed;
                    MessageBox.Show($"Error buscando nuevas actualizaciones: {ex}");
                }
            }
            else
            {
                InstallGameFiles(false, Version.zero);
            }
        }

        //For links: https://www.wonderplugin.com/online-tools/google-drive-direct-link-generator/
        private void InstallGameFiles(bool _isUpdate, Version _onlineVersion)
        {
            try
            {
                WebClient webClient = new WebClient();
                if (_isUpdate)
                {
                    Status = LauncherStatus.downloadingUpdate;
                    {
                        Progressbar1.Minimum = 0;
                        Progressbar1.Maximum = 100;
                        Progressbar1.Value = 25;
                    }
                }
                else
                {
                    Status = LauncherStatus.downloadingGame;
                    {
                        Progressbar1.Minimum = 0;
                        Progressbar1.Maximum = 100;
                        Progressbar1.Value = 50;
                    }
                    _onlineVersion = new Version(webClient.DownloadString("Export link version.txt"));
                    {
                        Progressbar1.Minimum = 0;
                        Progressbar1.Maximum = 100;
                        Progressbar1.Value = 75;
                    }
                }
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
                webClient.DownloadFileAsync(new Uri("Download link att.zip"), gameZip, _onlineVersion);
                {
                    Progressbar1.Minimum = 0;
                    Progressbar1.Maximum = 100;
                    Progressbar1.Value = 100;
                }
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error instalando actualizaciones: {ex}");
            }
        }

        private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                string onlineVersion = ((Version)e.UserState).ToString();
                ZipFile.ExtractToDirectory(gameZip, rootPath, true);
                File.Delete(gameZip);

                File.WriteAllText(versionFile, onlineVersion);

                VersionText.Text = onlineVersion;
                Status = LauncherStatus.updated;
                {
                    Progressbar1.Minimum = 0;
                    Progressbar1.Maximum = 100;
                    Progressbar1.Value = 0;
                }
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error terminando la descarga: {ex}");
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CheckForUpdates();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Grito.wav");
            mediaPlayer.Play();
            System.Threading.Thread.Sleep(900);
            if (File.Exists(gameExe) && Status == LauncherStatus.ready)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                //startInfo.WorkingDirectory = Path.Combine(rootPath, "");
                Process.Start(startInfo);

                Close();
            }
            else if (Status == LauncherStatus.failed)
            {
                CheckForUpdates();
            }
        }

        private void Update_Click(object sender, EventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Click.wav");
            mediaPlayer.Play();
            CheckForUpdates();
        }

        private void Click_close(object sender, EventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Close.wav");
            mediaPlayer.Play();
            System.Threading.Thread.Sleep(500);
            Close();
        }

        private void Click_minimize(object sender, EventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Click.wav");
            mediaPlayer.Play();
            WindowState = WindowState.Minimized;
        }

        private void Click_register(object sender, EventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Click.wav");
            mediaPlayer.Play();
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "Your register web link",
                UseShellExecute = true
            });
        }

        private void Click_web(object sender, RoutedEventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Click.wav");
            mediaPlayer.Play();
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "Your main web link",
                UseShellExecute = true
            });
        }

        private void Click_discord(object sender, RoutedEventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Click.wav");
            mediaPlayer.Play();
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "Your discord link",
                UseShellExecute = true
            });
        }

        private void Click_twitch(object sender, EventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Click.wav");
            mediaPlayer.Play();
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "Your twitch link",
                UseShellExecute = true
            });
        }

        private void Click_youtube(object sender, RoutedEventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Click.wav");
            mediaPlayer.Play();
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "Your youtube link",
                UseShellExecute = true
            });
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void myMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            myMediaElement.Position = TimeSpan.FromMilliseconds(1);
        }

        private void btnMute_Click(object sender, RoutedEventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Click.wav");
            mediaPlayer.Play();
            myMediaElement.IsMuted = !myMediaElement.IsMuted;
        }

        private void RegisterButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Button.wav");
            mediaPlayer.Play();
        }

        private void PlayButton_Copy_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Button.wav");
            mediaPlayer.Play();
        }

        private void Button_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Button.wav");
            mediaPlayer.Play();
        }

        private void RegisterButton_Copy1_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Button.wav");
            mediaPlayer.Play();
        }

        private void RegisterButton_Copy2_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Button.wav");
            mediaPlayer.Play();
        }

        private void Button_MouseEnter_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Button.wav");
            mediaPlayer.Play();
        }

        private void Button_MouseEnter_2(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Button.wav");
            mediaPlayer.Play();
        }

        private void Button_MouseEnter_3(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Button.wav");
            mediaPlayer.Play();
        }

        private void Button_MouseEnter_4(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Button.wav");
            mediaPlayer.Play();
        }

        private void PlayButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Media.SoundPlayer mediaPlayer = new System.Media.SoundPlayer(@"Audio\Gokuveg.wav");
            mediaPlayer.Play();
        }
    }

    struct Version
    {
        internal static Version zero = new Version(0, 0, 0);

        private short major;
        private short minor;
        private short subMinor;

        internal Version(short _major, short _minor, short _subMinor)
        {
            major = _major;
            minor = _minor;
            subMinor = _subMinor;
        }
        internal Version(string _version)
        {
            string[] versionStrings = _version.Split('.');
            if (versionStrings.Length != 3)
            {
                major = 0;
                minor = 0;
                subMinor = 0;
                return;
            }

            major = short.Parse(versionStrings[0]);
            minor = short.Parse(versionStrings[1]);
            subMinor = short.Parse(versionStrings[2]);
        }

        internal bool IsDifferentThan(Version _otherVersion)
        {
            if (major != _otherVersion.major)
            {
                return true;
            }
            else
            {
                if (minor != _otherVersion.minor)
                {
                    return true;
                }
                else
                {
                    if (subMinor != _otherVersion.subMinor)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            return $"{major}.{minor}.{subMinor}";
        }
    }
}
