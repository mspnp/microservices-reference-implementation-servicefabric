// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace PackageService.Models
{
    public enum PackageSize
    {
        Invalid,
        Small,
        Medium,
        Large
    }

    public enum PackageUpsertStatusCode
    {
        Invalid,
        Created,
        Updated
    }
}
