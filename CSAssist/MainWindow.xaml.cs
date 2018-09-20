using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using CefSharp.OffScreen;

namespace CSAssist
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        CefSettings cefsettings;
        BrowserSettings browsersettings;
        ChromiumWebBrowser browser;
        public MainWindow()
        {
            InitializeComponent();
            InitSettings();
            InitCef();
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
            Cef.Initialize(cefsettings, false, null);
            Cef.EnableHighDPISupport();

            browser = new ChromiumWebBrowser("")
            {
            RequestHandler   = null 
            };
        }
    }
}
