using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSAssist
{
    public class ChromeReqeustHandler : IRequestHandler
    {
        public event Action<string> OnReceive;
        readonly Dictionary<ulong, ResponseFilter> filterdict = new Dictionary<ulong, ResponseFilter>();

        public bool CanGetCookies(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request)
            => true;

        public bool CanSetCookie(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, Cookie cookie)
            => true;

        public bool GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
            => false;

        public IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            if (request.Method == "GET" && request.Url.Contains("api.twitter.com"))
                if (Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
                    if (uri.AbsolutePath == "/1.1/statuses/home_timeline.json")
                    {
                        var filter = new ResponseFilter();
                        filterdict.Add(request.Identifier, filter);
                        return filter;
                    }
            return null;
        }

        public bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool isRedirect)
            => false;

        public CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            if (!callback.IsDisposed)
                callback.Dispose();
            return CefReturnValue.Continue;
        }

        public bool OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            if (!callback.IsDisposed)
                callback.Dispose();
            return false;
        }

        public bool OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            return false;
        }

        public void OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath)
        {
        }

        public bool OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, string url)
            => false;

        public bool OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
        {
            if (!callback.IsDisposed)
                callback.Dispose();
            return false;
        }

        public void OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status)
        {
            throw new NotImplementedException();
        }

        public void OnRenderViewReady(IWebBrowser browserControl, IBrowser browser)
        {
        }

        public void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            if (request.Method == "GET" && request.Url.Contains("api.twitter.com"))
            {

                if (this.filterdict.TryGetValue(request.Identifier, out var filter))
                    this.filterdict.Remove(request.Identifier);
                else
                    return;

                if (response.StatusCode != 200)
                {
                    filter.Dispose();
                    return;
                }

                OnReceive?.Invoke(filter.ResponseBody);
            }
        }

        public void OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl)
        {
        }

        public bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            return false;
        }

        public bool OnSelectClientCertificate(IWebBrowser browserControl, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
        {
            if (!callback.IsDisposed)
                callback.Dispose();
            return false;
        }
    }

    public class ResponseFilter : IResponseFilter
    {
        public ResponseFilter()
        {

        }

        private readonly MemoryStream m_buffer = new MemoryStream(32768);

        public string ResponseBody
            => Encoding.UTF8.GetString(this.m_buffer.ToArray());
        
        bool IResponseFilter.InitFilter()
            => true;
        
        FilterStatus IResponseFilter.Filter(Stream dataIn, out long dataInRead, Stream dataOut, out long dataOutWritten)
        {
            if (dataIn == null)
            {
                dataInRead = dataOutWritten = 0;

                return FilterStatus.Done;
            }

            var len = Math.Min(dataIn.Length, dataOut.Length);
            var buffer = new byte[len];

            var read = dataIn.Read(buffer, 0, (int)len);

            this.m_buffer.Write(buffer, 0, read);
            dataOut.Write(buffer, 0, read);

            dataInRead = dataOutWritten = read;

            return FilterStatus.Done;
        }

        public void Dispose()
        {
            this.m_buffer.Dispose();
        }
    }
}
