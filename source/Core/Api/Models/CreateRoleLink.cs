﻿/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System.Collections.Generic;
using System.Web.Http.Routing;

namespace Thinktecture.IdentityManager.Api.Models
{
    public class CreateRoleLink : Dictionary<string, object>
    {
        public CreateRoleLink(UrlHelper url, RoleMetadata roleMetadata)
        {
            this["href"] = url.Link(Constants.RouteNames.CreateRole, null);
            this["meta"] = roleMetadata.GetCreateProperties();
        }
    }
}
