﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

using Jobbr.Common.Model;

using Newtonsoft.Json;

namespace Jobbr.Client
{
    public class JobbrClient
    {
        protected HttpClient httpClient;

        private readonly string backend;

        public JobbrClient(string backend)
        {
            this.backend = backend + (backend.EndsWith("/") ? string.Empty  : "/") + "api/";
            this.httpClient = new HttpClient { BaseAddress = new Uri(this.backend) };
        }

        public string Backend
        {
            get
            {
                return this.backend;
            }
        }

        public List<JobDto> GetAllJobs()
        {
            var url = string.Format("jobs/");

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = this.httpClient.SendAsync(request).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var contentString = response.Content.ReadAsStringAsync().Result;

                var responseDto = JsonConvert.DeserializeObject<List<JobDto>>(contentString);

                return responseDto;
            }

            return null;
        }

        public T TriggerJob<T>(long jobId, T triggerDto) where T : JobTriggerDtoBase
        {
            var url = string.Format("jobs/{0}/trigger", jobId);
            return this.PostTrigger(triggerDto, url);
        }

        public T TriggerJob<T>(string uniqueName, T triggerDto) where T : JobTriggerDtoBase
        {
            var url = string.Format("jobs/{0}/trigger", uniqueName);
            return this.PostTrigger(triggerDto, url);
        }

        public List<JobRunDto> GetJobRunsByTriggerId(long triggerId)
        {
            // Get the JobRun by this triggerId
            var url = string.Format("jobruns/?triggerId={0}", triggerId);

            var requestResult = this.httpClient.GetAsync(url).Result;

            if (requestResult.StatusCode == HttpStatusCode.OK)
            {
                var json = requestResult.Content.ReadAsStringAsync().Result;

                var runs = JsonConvert.DeserializeObject<List<JobRunDto>>(json);

                return runs;
            }

            return null;
        }

        public JobRunDto GetJobRunById(long jobRunId)
        {
            // Get the JobRun by this triggerId
            var url = string.Format("jobruns/{0}", jobRunId);

            var requestResult = this.httpClient.GetAsync(url).Result;

            if (requestResult.StatusCode == HttpStatusCode.OK)
            {
                var json = requestResult.Content.ReadAsStringAsync().Result;

                var run = JsonConvert.DeserializeObject<JobRunDto>(json);

                return run;
            }

            return null;
        }

        private T PostTrigger<T>(T triggerDto, string url) where T : JobTriggerDtoBase
        {
            var json = JsonConvert.SerializeObject(triggerDto);
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = this.httpClient.SendAsync(request).Result;

            if (response.StatusCode == HttpStatusCode.Created)
            {
                var contentString = response.Content.ReadAsStringAsync().Result;

                var responseDto = JsonConvert.DeserializeObject<T>(contentString);

                return responseDto;
            }

            return null;
        }
    }
}
