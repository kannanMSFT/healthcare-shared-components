// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Azure;
using Azure.Core.Pipeline;
using Azure.Messaging.EventGrid;
using EnsureThat;
using Microsoft.Health.Abstractions.Data;
using Microsoft.Health.Abstractions.Features.Events;

namespace Microsoft.Health.Core.Features.Events
{
    /// <summary>
    /// EventGridEventGenerator.
    /// </summary>
    /// <typeparam name="T">T of Type IEvent.</typeparam>
    public class EventGridEventGenerator<T> : ISink<T>
        where T : IEvent
    {
        private readonly EventGridPublisherClient _client;
        private readonly HttpClientHandler _httpClientHandler;
        private readonly HttpClient _httpClient;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventGridEventGenerator{T}"/> class.
        /// EventGridEventGenerator constructor.
        /// </summary>
        /// <param name="endpoint">Uri.</param>
        /// <param name="key">string access key.</param>
        public EventGridEventGenerator(Uri endpoint, string key)
        {
            _client = new EventGridPublisherClient(endpoint, new AzureKeyCredential(key));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventGridEventGenerator{T}"/> class.
        /// EventGridEventGenerator constructor.
        /// </summary>
        /// <param name="endpoint">Uri.</param>
        /// <param name="publisherCertificate">publisher certificate.</param>
        public EventGridEventGenerator(Uri endpoint, X509Certificate publisherCertificate)
        {
            _httpClientHandler = new HttpClientHandler();
            _httpClientHandler.CheckCertificateRevocationList = true;
            _httpClientHandler.ClientCertificates.Add(publisherCertificate);
            _httpClient = new HttpClient(_httpClientHandler);
            var options = new EventGridPublisherClientOptions
            {
                Transport = new HttpClientTransport(_httpClient),
            };
            _client = new EventGridPublisherClient(endpoint, new AzureKeyCredential("AzureSystemPublisher"), options);
        }

        private async Task SendEventAsync(EventGridEvent eventGridEvent)
        {
            await _client.SendEventAsync(eventGridEvent);
        }

        private async Task SendEventsAsync(IEnumerable<EventGridEvent> eventGridEvents)
        {
            await _client.SendEventsAsync(eventGridEvents);
        }

        /// <inheritdoc />
        public async Task WriteAsync(T data)
        {
            var eventGridEvent = new EventGridEvent(data.Subject, data.EventType, data.DataVersion, data.Data);
            eventGridEvent.Topic = data.Topic;
            eventGridEvent.EventTime = data.EventTime;
            eventGridEvent.Id = data.Id;
            await SendEventAsync(eventGridEvent);
        }

        /// <inheritdoc />
        public async Task WriteAsync(IReadOnlyCollection<T> data)
        {
            EnsureArg.IsNotNull(data, nameof(data));

            var events = new List<EventGridEvent>();
            foreach (T item in data)
            {
                var eventGridEvent = new EventGridEvent(item.Subject, item.EventType, item.DataVersion, item.Data);
                eventGridEvent.Topic = item.Topic;
                eventGridEvent.EventTime = item.EventTime;
                eventGridEvent.Id = item.Id;
                events.Add(eventGridEvent);
            }

            await SendEventsAsync(events);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (_httpClientHandler != null)
                {
                    _httpClientHandler.Dispose();
                }

                if (_httpClient != null)
                {
                    _httpClient.Dispose();
                }
            }

            _isDisposed = true;
        }
    }
}
