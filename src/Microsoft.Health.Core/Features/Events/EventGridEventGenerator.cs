// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.EventGrid;
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
            await SendEventAsync(eventGridEvent);
        }

        /// <inheritdoc />
        public async Task WriteAsync(IReadOnlyCollection<T> data)
        {
            var events = data.Select(d => new EventGridEvent(d.Subject, d.EventType, d.DataVersion, d.Data));
            await SendEventsAsync(events);
        }
    }
}
