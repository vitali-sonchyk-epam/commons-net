﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ReportPortal.Client
{
    public class RetryWithExponentialBackoffHttpClientHandler : DelegatingHandler
    {
        public int MaxRetries { get; }

        public RetryWithExponentialBackoffHttpClientHandler(int maxRetries)
            : this(maxRetries, new HttpClientHandler())
        {
        }

        public RetryWithExponentialBackoffHttpClientHandler(int maxRetries, IWebProxy proxy)
            : this(maxRetries, new HttpClientHandler { Proxy = proxy })
        {
        }

        public RetryWithExponentialBackoffHttpClientHandler(int maxRetries, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            MaxRetries = maxRetries;
        }

        private List<HttpStatusCode> ResponseStatusCodesForRetrying => new List<HttpStatusCode> { HttpStatusCode.InternalServerError, HttpStatusCode.NotImplemented, HttpStatusCode.BadGateway, HttpStatusCode.ServiceUnavailable, HttpStatusCode.GatewayTimeout, HttpStatusCode.MethodNotAllowed };

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;

            for (var i = 0; i < MaxRetries; i++)
            {
                try
                {
                    response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                    if (!ResponseStatusCodesForRetrying.Contains(response.StatusCode))
                    {
                        return response;
                    }

                    //TODO: commented until VerifySuccessStatusCode becomes public
                    //response.VerifySuccessStatusCode();
                }

                catch (Exception exp) when (exp is TaskCanceledException || exp is HttpRequestException)
                {
                    if (i < MaxRetries - 1)
                    {
                        await Task.Delay((int)Math.Pow(2, i + MaxRetries) * 1000, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return response;
        }
    }
}
