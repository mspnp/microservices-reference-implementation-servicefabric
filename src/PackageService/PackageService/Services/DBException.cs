// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace PackageService.Services
{
    public class DBException: Exception
    {
        public DBException() : base() { }

        public DBException(string message) : base(message) { }

        public DBException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public DBException(string message, Exception innerException) : base(message, innerException) { }

    }
}
