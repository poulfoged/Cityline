using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Net.Http;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Text;

namespace Cityline.Client
{
    public class CitylineClient : EventEmitter, IDisposable
    {
        private readonly Uri _serverUrl;
        private readonly Func<HttpClient> _httpClientFactory;
        private readonly Action<HttpRequestMessage> _messageModifier;
        private readonly IDictionary<string, Frame> _frames = new Dictionary<string, Frame>();
        private readonly IDictionary<string, string> _idCache = new Dictionary<string, string>();
        private CancellationTokenSource _internalTokenSource = new CancellationTokenSource();

        public CitylineClient(Uri serverUrl, Func<HttpClient> httpClientFactory = null, Action<HttpRequestMessage> messageModifier = null)
        {
            _serverUrl = serverUrl;
            _httpClientFactory = httpClientFactory ?? new Func<HttpClient>(() => new HttpClient());
            _messageModifier = messageModifier ?? new Action<HttpRequestMessage>((message) => {});
        }

        public void Dispose()
        {
            if (_internalTokenSource != null)
                _internalTokenSource.Cancel();
        }

        // var handler = new HttpClientHandler
        // {
        //     ClientCertificateOptions = ClientCertificateOption.Manual,
        //     ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => { return true; }
        // };
        
        public async Task StartListening(CancellationToken externalToken = default(CancellationToken))
        {
            var buffer = new Buffer();
            using (CancellationTokenSource linkedToken = CancellationTokenSource.CreateLinkedTokenSource(_internalTokenSource.Token, externalToken)) 
            using (HttpClient httpClient = _httpClientFactory.Invoke())
            using( var message = new HttpRequestMessage(HttpMethod.Post, this._serverUrl))
            {
                httpClient.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
                
                message.Headers.Add("device-id", Guid.NewGuid().ToString("N"));
                message.Content = new StringContent("{ tickets: {} }", Encoding.UTF8, "application/json");

                using (var response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead))
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream && !linkedToken.IsCancellationRequested)
                    {
                        buffer.Add(reader.ReadLine());

                        while (buffer.HasTerminator()) {
                            var chunk = buffer.Take();
                            var frame = Frame.Parse(chunk);
                            AddFrame(frame);
                        }                        
                    }
                }
            }
        }

        private void AddFrame(Frame frame) 
        {
            if (frame == null)
                return;
            
            if (string.IsNullOrWhiteSpace(frame.EventName))
                return;

            if (string.IsNullOrWhiteSpace(frame.Id))
                return;

            _idCache[frame.EventName] = frame.Id;
            _frames[frame.EventName] = frame;
            Emit(frame.EventName, frame);
        }
    }    
}
