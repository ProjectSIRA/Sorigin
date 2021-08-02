using SiraUtil.Tools;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace Sorigin.Services
{
    internal class SoriginGrantService : IInitializable, IDisposable
    {
        private bool _active;
        private readonly Thread _thread;
        private readonly SiraLog _siraLog;
        private readonly HttpListener _httpListener;

        public event Action<string>? GrantReceived;

        public SoriginGrantService(SiraLog siraLog)
        {
            _siraLog = siraLog;
            _thread = new Thread(UpdateThread);
            _httpListener = new HttpListener { Prefixes = { "http://localhost:20549" } };
        }

        public void Initialize()
        {
            _active = true;
            _thread.Start();
            _httpListener.Start();
        }

        public void Dispose()
        {
            _active = false;
            _httpListener.Stop();
        }

        private async void UpdateThread()
        {
            while (_active)
            {
                try
                {
                    await OnContext(await _httpListener.GetContextAsync());
                }
                catch (Exception e)
                {
                    _siraLog.Error(e);
                }
            }
        }

        private async Task OnContext(HttpListenerContext ctx)
        {
            using var response = ctx.Response;
            try
            {
                string grant = ctx.Request.QueryString["grant"];
                if (string.IsNullOrEmpty(grant))
                {
                    using Stream noGrantStream = new MemoryStream(Encoding.UTF8.GetBytes("Missing authorization grant!"));
                    await noGrantStream.CopyToAsync(ctx.Response.OutputStream);
                    response.StatusCode = 403;
                    return;
                }
                GrantReceived?.Invoke(grant);
                using Stream grantStream = new MemoryStream(Encoding.UTF8.GetBytes("Grant received. You can close this window."));
                await grantStream.CopyToAsync(ctx.Response.OutputStream);
                response.StatusCode = 200;
            }
            catch (Exception e)
            {
                _siraLog.Error(e);
                response.StatusCode = 500;
            }
        }
    }
}