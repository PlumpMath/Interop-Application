﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Prism.Events;

using Interop.Infrastructure.Events;
using Interop.Infrastructure.Interfaces;
using Interop.Infrastructure.Models;
using Interop.Modules.Client.Requests;

namespace Interop.Modules.Client.Services
{
    public class HttpService : IHttpService
    {
        const string USER = "simpleuser";
        const string PASS = "simplepass";

        static object[] REQUESTS = { new GetServerInfo(), new GetTargets(), new GetObstacles()};
        BackgroundWorker bw = new BackgroundWorker();

        IEventAggregator _eventAggregator;
        CookieContainer _cookieContainer;

        public HttpService(IEventAggregator eventAggregator)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            _eventAggregator = eventAggregator;

            if (true == Login())
            {
                bw.WorkerSupportsCancellation = true;
                bw.WorkerReportsProgress = true;

                bw.DoWork += Bw_DoWork;
                bw.RunWorkerAsync();
                _eventAggregator.GetEvent<UpdateLoginStatusEvent>().Publish(string.Format("Connected as {0}", USER));
            }
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            //TODO: Latency monitoring, handle timeout if server is down.
            while(!(sender as BackgroundWorker).CancellationPending)
            { 
                Task<ServerInfo> serverInfoTask = Task.Run(() => RunAsync<ServerInfo>((IRequest)REQUESTS[0]).Result);
                Task<List<Target>> targetsTask = Task.Run(() => RunAsync<List<Target>>((IRequest)REQUESTS[1]).Result);
                Task<Obstacles> obstaclesTask = Task.Run(() => RunAsync<Obstacles>((IRequest)REQUESTS[2]).Result);

                serverInfoTask.Wait();
                targetsTask.Wait();
                obstaclesTask.Wait();

                _eventAggregator.GetEvent<UpdateServerInfoEvent>().Publish(serverInfoTask.Result);
                _eventAggregator.GetEvent<UpdateTargetsEvent>().Publish(targetsTask.Result);
                _eventAggregator.GetEvent<UpdateObstaclesEvent>().Publish(obstaclesTask.Result);

                Console.WriteLine(serverInfoTask.Result.server_time);
                Console.WriteLine(targetsTask.Result);
                Console.WriteLine(obstaclesTask.Result);

                // You can decrease the value to get faster refresh
                Thread.Sleep(500);
            }
        }

        private async Task<T> RunAsync<T>(IRequest request) where T: class
        {
            using (var handler = new HttpClientHandler() { CookieContainer = _cookieContainer })
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri("http://mikaelferland.com:80/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                // HTTP GET
                HttpResponseMessage response = await client.GetAsync(request.Endpoint);
                if (response.IsSuccessStatusCode)
                {
                    string output = await response.Content.ReadAsStringAsync();
                    request.Data = JsonConvert.DeserializeObject<T>(output);
                    return request.Data as T;
                }
            }
            return null;
        }

        public bool Login()
        {
            var baseAddress = new Uri("http://mikaelferland.com:80/");

            _cookieContainer = new CookieContainer();

            using (var handler = new HttpClientHandler() { CookieContainer = _cookieContainer })

            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", "simpleuser"),
                    new KeyValuePair<string, string>("password", "simplepass"),
                });
                _cookieContainer.Add(baseAddress, new Cookie("CookieName", "cookie_value"));
                var result = client.PostAsync("/api/login", content).Result;
                result.EnsureSuccessStatusCode();
                return result.IsSuccessStatusCode;
            }
        }
    }
}