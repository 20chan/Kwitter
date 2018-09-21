using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using CSAssist.Json;
using CSAssist.Properties;
using Fiddler;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;

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
            Login();
        }

        void InitDriver()
        {
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            var options = new ChromeOptions()
            {
                Proxy = new OpenQA.Selenium.Proxy()
                {
                    HttpProxy = "localhost:8880",
                    SslProxy = "localhost:8880"
                }
            };
            driver = new ChromeDriver(service, options);

            FiddlerApplication.BeforeResponse += FiddlerApplication_BeforeResponse;
            FiddlerApplication.Startup(8880, false, true);
        }

        void Login()
        {
            driver.Url = "https://tweetdeck.twitter.com";
            WaitUntilLoadOne(By.TagName("a")).Click();

            WaitUntilLoadOne(By.ClassName("js-username-field"))
                .SendKeys(Settings.Default.USERNAME);
            driver.FindElementByClassName("js-password-field")
                .SendKeys(Settings.Default.PASSWORD);
            driver.FindElementsByClassName("submit")[1]
                .Click();
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
        private void FiddlerApplication_BeforeResponse(Session oSession)
        {
            if (oSession.RequestMethod == "GET")
            {
                var body = oSession.GetResponseBodyAsString();
                Debug.WriteLine(body);
                if (Uri.TryCreate(oSession.fullUrl, UriKind.Absolute, out var url))
                {
                    if (url.AbsolutePath == "/1.1/statuses/home_timeline.json")
                    {
                        if (oSession.responseCode == 200)
                        {
                            try
                            {
                                //var body = oSession.GetResponseBodyAsString();
                                var json = JToken.Parse(body);
                                var statuses = json.ToObject<TStatuses>()
                                    .Where(i => lastStatus < i.ID)
                                    .OrderBy(i => i.ID);

                                long max = lastStatus;

                                if (statuses.Count() == 0)
                                    return;
                                if (lastStatus == 0)
                                    max = statuses.Last().ID;
                                else
                                {
                                    foreach (var s in statuses)
                                    {
                                        StatusUpdated(s);
                                        max = Math.Max(max, s.ID);
                                    }
                                }

                                lastStatus = max;
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        void StatusUpdated(TStatus status)
        {
            Dispatcher.Invoke(() =>
            {
                chatlist.Add(status.User.UserName, status.Text);
            });
        }
    }
}
