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
            _httpListener = new HttpListener { Prefixes = { "http://localhost:20549/" } };
        }

        public void Initialize()
        {
            _active = true;
            _httpListener.Start();
            _thread.Start();
        }

        public void Dispose()
        {
            _active = false;
            _httpListener.Close();
        }

        private async void UpdateThread()
        {
            while (_active)
            {
                try
                {
                    if (_active)
                    {
                        HttpListenerContext ctx = await _httpListener.GetContextAsync();
                        if (_active)
                            await OnContext(ctx);
                    }
                }
                catch (Exception e)
                {
                    if (_active)
                        _siraLog.Error(e);
                }
            }
        }

        private async Task OnContext(HttpListenerContext ctx)
        {
            _siraLog.Info("Context received. Processing...");
            using var response = ctx.Response;
            try
            {
                string grant = ctx.Request.QueryString["grant"];
                if (string.IsNullOrEmpty(grant))
                {
                    response.StatusCode = 403;
                    _siraLog.Warning("Could not find Sorigin authorization grant!");
                    using Stream noGrantStream = new MemoryStream(Encoding.UTF8.GetBytes("Missing authorization grant!"));
                    await noGrantStream.CopyToAsync(ctx.Response.OutputStream);
                    return;
                }
                _siraLog.Info("Grant received. Processing...");
                GrantReceived?.Invoke(grant);
                response.StatusCode = 200;
                using Stream grantStream = new MemoryStream(Encoding.UTF8.GetBytes("Grant received. You can close this window."));
                await grantStream.CopyToAsync(ctx.Response.OutputStream);
            }
            catch (Exception e)
            {
                _siraLog.Error(e);
                response.StatusCode = 500;
                using Stream errorStream = new MemoryStream(Encoding.UTF8.GetBytes("An internal server error has occured! You should report this."));
                await errorStream.CopyToAsync(ctx.Response.OutputStream);
            }
        }
    }
}