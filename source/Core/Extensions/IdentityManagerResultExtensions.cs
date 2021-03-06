﻿/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using System;
using System.Linq;
using Thinktecture.IdentityManager.Api.Models;

namespace Thinktecture.IdentityManager
{
    static class IdentityManagerResultExtensions
    {
        public static ErrorModel ToError(this IdentityManagerResult result)
        {
            if (result == null) throw new ArgumentNullException("result");

            return new ErrorModel
            {
                Errors = result.Errors.ToArray()
            };
        }
    }
}
