using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp;
using CefSharp.Wpf;
using CSAssist.Properties;
using Newtonsoft.Json.Linq;

namespace CSAssist
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        CefSettings cefsettings;
        BrowserSettings browsersettings;
        ChromeReqeustHandler reqhandler;

        bool loggedin = false;

        public MainWindow()
        {
            InitSettings();
            InitCef();
            InitializeComponent();

            browser.RequestHandler = reqhandler;
            browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;
            browser.FrameLoadEnd += Browser_FrameLoadEnd;
        }

        private void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (!loggedin)
            {
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    Thread.Sleep(3000);
                    browser.ExecuteScriptAsync("document.getElementsByTagName('a')[0].click()");
                });
            }

            if (e.Url.StartsWith("https://twitter.com/login"))
            {
                string id = Settings.Default.USERNAME, pw = Settings.Default.PASSWORD;
                browser.ExecuteScriptAsync($"document.getElementsByClassName('js-username-field')[0].value = '{id}'");
                browser.ExecuteScriptAsync($"document.getElementsByClassName('js-password-field')[0].value = '{pw}'");
                browser.ExecuteScriptAsync($"document.getElementsByClassName('submit')[1].click()");
                loggedin = true;
            }
        }

        void InitSettings()
        {
            cefsettings = new CefSettings
            {
                CachePath = null,
                LogSeverity = LogSeverity.Disable,
                LogFile = null,
                WindowlessRenderingEnabled = true,
                CefCommandLineArgs =
                {
                    { "no-proxy-server"          , "1" },
                    { "mute-audio"               , "1" },
                    { "disable-application-cache", "1" },
                    { "disable-extensions"       , "1" },
                    { "disable-features"         , "AsyncWheelEvents,TouchpadAndWheelScrollLatching" },
                    { "disable-gpu"              , "1" },
                    { "disable-gpu-vsync"        , "1" },
                    { "disable-gpu-compositing"  , "1" },
                }
            };
            cefsettings.DisableGpuAcceleration();
            cefsettings.SetOffScreenRenderingBestPerformanceArgs();

            browsersettings = new BrowserSettings()
            {
                DefaultEncoding = "UTF-8",
                WebGl = CefState.Disabled,
                Plugins = CefState.Disabled,
                JavascriptAccessClipboard = CefState.Disabled,
                ImageLoading = CefState.Disabled,
                JavascriptCloseWindows = CefState.Disabled,
                ApplicationCache = CefState.Disabled,
                RemoteFonts = CefState.Disabled,
                WindowlessFrameRate = 1,
                Databases = CefState.Disabled,
            };

            CefSharpSettings.SubprocessExitIfParentProcessClosed = true;
            CefSharpSettings.ShutdownOnExit = true;
            CefSharpSettings.WcfEnabled = false;
            CefSharpSettings.Proxy = null;
        }

        void InitCef()
        {
            //Cef.Initialize(cefsettings, false, null);
            Cef.EnableHighDPISupport();

            reqhandler = new ChromeReqeustHandler();
            reqhandler.OnReceive += Reqhandler_OnReceive;
        }

        private void Browser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (browser.IsInitialized)
            {
                browser.Load("https://tweetdeck.twitter.com");

                browser.ExecuteScriptAsync("document.getElementsByTagName('a')[0].click()");
                Thread.Sleep(3000);
                string id = Settings.Default.USERNAME, pw = Settings.Default.PASSWORD;
                browser.ExecuteScriptAsync($"document.getElementsByClassName('js-username-field')[0].value = '{id}'");
                browser.ExecuteScriptAsync($"document.getElementsByClassName('js-password-field')[0].value = '{pw}'");
                browser.ExecuteScriptAsync($"document.getElementsByClassName('submit')[1].click()");
            }
        }

        private void Reqhandler_OnReceive(string obj)
        {
            Dispatcher.Invoke(() =>
            {
                var json = JArray.Parse(obj);
                foreach (var t in json)
                {
                    chatlist.Add("", t["full_text"].ToString());
                }
            });
        }

        private void Browser_BrowserInitialized(object sender, EventArgs e)
        {
            browser.Load("https://tweetdeck.twitter.com");

            browser.ExecuteScriptAsync("document.getElementsByTagName('a')[0].click()");
            Thread.Sleep(3000);
            string id = Settings.Default.USERNAME, pw = Settings.Default.PASSWORD;
            browser.ExecuteScriptAsync($"document.getElementsByClassName('js-username-field')[0].value = '{id}'");
            browser.ExecuteScriptAsync($"document.getElementsByClassName('js-password-field')[0].value = '{pw}'");
            browser.ExecuteScriptAsync($"document.getElementsByClassName('submit')[1].click()");
        }
    }
}
