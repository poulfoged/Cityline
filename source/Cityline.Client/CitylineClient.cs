using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Cityline.Client
{
    public class CitylineClient : EventEmitter, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _serverUrl;
        private readonly Func<HttpClient> _httpClientFactory;
        private readonly Action<HttpRequestMessage> _messageModifier;
        private readonly IDictionary<string, Frame> _frames = new Dictionary<string, Frame>();
        private readonly IDictionary<string, string> _idCache = new Dictionary<string, string>();
        private CancellationTokenSource _internalTokenSource = new CancellationTokenSource();
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), Formatting = Formatting.None };
    
        public CitylineClient(Uri serverUrl, Func<HttpClient> httpClientFactory = null, Action<HttpRequestMessage> messageModifier = null)
        {
            _serverUrl = serverUrl;
            _httpClientFactory = httpClientFactory ?? new Func<HttpClient>(() => new HttpClient() { Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite) });
            _messageModifier = messageModifier ?? new Action<HttpRequestMessage>((message) => {});

            _httpClient = _httpClientFactory.Invoke();
        }

        public void Dispose()
        {
            _internalTokenSource?.Cancel();
            _httpClient?.Dispose();
        }

        // var handler = new HttpClientHandler
        // {
        //     ClientCertificateOptions = ClientCertificateOption.Manual,
        //     ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => { return true; }
        // };
        
        public async Task StartListening(CancellationToken externalToken = default(CancellationToken))
        {
            var buffer = new Buffer();
            var json = JsonConvert.SerializeObject(new { tickets = _idCache}, settings);

            using (CancellationTokenSource linkedToken = CancellationTokenSource.CreateLinkedTokenSource(_internalTokenSource.Token, externalToken)) 
            using (var message = new HttpRequestMessage(HttpMethod.Post, this._serverUrl))
            using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                _messageModifier.Invoke(message);
                message.Content = content;

                using (var response = await _httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead))
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
