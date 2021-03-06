﻿/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;

namespace Thinktecture.IdentityManager
{
    public class UserDetail : UserSummary
    {
        public IEnumerable<Property> Properties { get; set; }
        public IEnumerable<Property> Claims { get; set; }
    }
}
