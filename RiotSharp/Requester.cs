﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace RiotSharp
{
    class Requester
    {
        private static Requester instance;
        protected Requester() { }
        public static Requester Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Requester();
                }
                return instance;
            }
        }

        public static string RootDomain { get; set; }
        public static string ApiKey { get; set; }

        public virtual string CreateRequest(string relativeUrl, Region region, List<string> addedArguments = null)
        {
            RootDomain = "global.api.pvp.net";
            var request = PrepareRequest(relativeUrl, addedArguments);
            return GetResponse(request);
        }

        public virtual async Task<string> CreateRequestAsync(string relativeUrl, Region region,
            List<string> addedArguments = null)
        {
            RootDomain = "global.api.pvp.net";
            var request = PrepareRequest(relativeUrl, addedArguments);
            return await GetResponseAsync(request);
        }

        protected HttpWebRequest PrepareRequest(string relativeUrl, List<string> addedArguments)
        {
            HttpWebRequest request = null;
            if (addedArguments == null)
            {
                request = (HttpWebRequest)WebRequest.Create(string.Format("https://{0}{1}?api_key={2}"
                    , RootDomain, relativeUrl, ApiKey));
            }
            else
            {
                request = (HttpWebRequest)WebRequest.Create(string.Format("https://{0}{1}?{2}api_key={3}"
                    , RootDomain, relativeUrl, BuildArgumentsString(addedArguments), ApiKey));
            }
            request.Method = "GET";

            return request;
        }

        protected string GetResponse(HttpWebRequest request)
        {
            string result = string.Empty;
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }
            }
            catch(WebException ex)
            {
                HandleWebException(ex);
            }
            return result;
        }

        protected async Task<string> GetResponseAsync(HttpWebRequest request)
        {
            string result = string.Empty;
            try
            {
                var response = (HttpWebResponse)(await request.GetResponseAsync());
                
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    result = await reader.ReadToEndAsync();
                }
            }
            catch(WebException ex)
            {
                HandleWebException(ex);
            }
            return result;
        }

        protected string BuildArgumentsString(List<string> arguments)
        {
            string result = string.Empty;
            foreach (string arg in arguments)
            {
                if (arg != string.Empty)
                {
                    result += arg + "&";
                }
            }
            return result;
        }
        
        private void HandleWebException(WebException ex)
        {
            HttpWebResponse response = (HttpWebResponse)ex.Response;
            switch (response.StatusCode)
            {
                case HttpStatusCode.ServiceUnavailable:
                    throw new RiotSharpException("503, Service unavailable");
                case HttpStatusCode.InternalServerError:
                    throw new RiotSharpException("500, Internal server error");
                case HttpStatusCode.Unauthorized:
                    throw new RiotSharpException("401, Unauthorized");
                case HttpStatusCode.BadRequest:
                    throw new RiotSharpException("400, Bad request");
                case HttpStatusCode.NotFound:
                    throw new RiotSharpException("404, Resource not found");
            }
        }
    }
}
