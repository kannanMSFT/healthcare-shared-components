// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;

namespace Microsoft.Health.Abstractions.Features.Events
{
    /// <summary>
    /// Common Event interface.
    /// </summary>
    public interface IEvent
    {
        /// <summary>Gets or sets a resource path relative to the topic path.</summary>
        public string Subject { get; set; }

        /// <summary>Gets or sets the type of the event that occurred.</summary>
        public string EventType { get; set; }

        /// <summary>Gets or sets the schema version of the data object.</summary>
        public string DataVersion { get; set; }

        /// <summary>
        /// Gets or sets the Data.
        /// </summary>
        public BinaryData Data { get; set; }
    }
}
