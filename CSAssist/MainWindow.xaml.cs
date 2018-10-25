using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CSAssist.Json;
using CSAssist.Properties;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;
using System.Text;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.EventArguments;
using System.Net;
using System.Windows.Input;

namespace CSAssist
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        ChromeDriver driver;
        public MainWindow()
        {
            InitializeComponent();

            InitDriver();
            InitProxy();
            Login();
            
            this.Closed += MainWindow_Closed;
            this.Deactivated += MainWindow_Deactivated;
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            this.Topmost = true;
            this.Activate();
            
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            driver.Close();
            driver.Dispose();
        }

        void InitDriver()
        {
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            var options = new ChromeOptions()
            {
                Proxy = new Proxy()
                {
                    HttpProxy = "localhost:8881",
                    SslProxy = "localhost:8881"
                }
            };
            
            options.AddArguments("headless"); 
            //, "disable-gpu");
            options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            driver = new ChromeDriver(service, options);

        }

        void InitProxy()
        {
            var proxy = new ProxyServer();
            proxy.AddEndPoint(new ExplicitProxyEndPoint(IPAddress.Any, 8881, true));
            proxy.BeforeResponse += Proxy_BeforeResponse;
            proxy.Start();
        }

        private async Task Proxy_BeforeResponse(object sender, SessionEventArgs e)
        {
            if (e.WebSession.Request.Method == "GET")
            {
                if (Uri.TryCreate(e.WebSession.Request.Url, UriKind.Absolute, out var url))
                {
                    if (url.AbsolutePath == "/1.1/statuses/home_timeline.json")
                    {
                        // if (e.WebSession.Response.StatusCode == 200)
                        {
                            try
                            {
                                var body = await e.GetResponseBodyAsString();
                                Debug.WriteLine(body);
                                var json = JToken.Parse(body);
                                var statuses = json.ToObject<TStatuses>()
                                    .Where(i => lastStatus < i.ID)
                                    .OrderBy(i => i.ID);

                                long max = lastStatus;

                                if (statuses.Count() == 0)
                                    return;
                                if (lastStatus == 0)
                                    max = statuses.Last().ID;

                                foreach (var s in statuses)
                                {
                                    StatusUpdated(s);
                                    max = Math.Max(max, s.ID);
                                }

                                lastStatus = max;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                        }
                    }
                }
            }
        }

        void Login()
        {
            driver.Url = "https://tweetdeck.twitter.com";
            WaitUntilLoadOne(By.TagName("a")).Click();

            Thread.Sleep(500);
            WaitUntilLoadOne(By.ClassName("js-username-field"));

            driver.ExecuteScript($"document.getElementsByClassName('js-username-field')[0].value='{Settings.Default.USERNAME}'");
            driver.ExecuteScript($"document.getElementsByClassName('js-password-field')[0].value='{Settings.Default.PASSWORD}'");
            driver.ExecuteScript($"document.getElementsByClassName('submit')[1].click()");
        }

        ReadOnlyCollection<IWebElement> WaitUntilLoad(By option)
        {
            while (true)
            {
                var elems = driver.FindElements(option);
                if (elems.Count > 0)
                    return elems;

                Thread.Sleep(100);
            }
        }

        IWebElement WaitUntilLoadOne(By option)
            => WaitUntilLoad(option)[0];

        long lastStatus;

        void StatusUpdated(TStatus status)
        {
            Dispatcher.Invoke(() =>
            {
                chatlist.Add(status.User.UserName, status.Text);
            });
        }

        private void chatlist_AnywayMouseDown()
        {
            try
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                    DragMove();
            }
            catch { }
        }
    }
}
