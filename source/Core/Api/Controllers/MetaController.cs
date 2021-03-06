﻿/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityManager.Api.Filters;

namespace Thinktecture.IdentityManager.Api.Models.Controllers
{
    [NoCache]
    [RoutePrefix(Constants.MetadataRoutePrefix)]
    public class MetaController : ApiController
    {
        IIdentityManagerService userManager;
        IdentityManagerConfiguration config;
        public MetaController(IdentityManagerConfiguration config, IIdentityManagerService userManager)
        {
            if (config == null) throw new ArgumentNullException("config");
            if (userManager == null) throw new ArgumentNullException("userManager");

            this.config = config;
            this.userManager = userManager;
        }

        IdentityManagerMetadata _metadata;
        async Task<IdentityManagerMetadata> GetMetadataAsync()
        {
            if (_metadata == null)
            {
                _metadata = await this.userManager.GetMetadataAsync();
                if (_metadata == null) throw new InvalidOperationException("GetMetadataAsync returned null");
                _metadata.Validate();
            }

            return _metadata;
        }

        [Route("")]
        public async Task<IHttpActionResult> Get()
        {
            var meta = await GetMetadataAsync();

            var data = new Dictionary<string, object>();
            
            var cp = (ClaimsPrincipal)User;
            var name = cp.Claims.Where(x => x.Type == Constants.ClaimTypes.Name).Select(x => x.Value).FirstOrDefault();
            data.Add("currentUser", new {
                username = name
            });

            var links = new Dictionary<string, object>();
            links["users"] = Url.Link(Constants.RouteNames.GetUsers, null);
            if (meta.RoleMetadata.SupportsListing)
            {
                links["roles"] = Url.Link(Constants.RouteNames.GetRoles, null);
            }
            if (meta.UserMetadata.SupportsCreate)
            {
                links["createUser"] = new CreateUserLink(Url, meta.UserMetadata);
            }
            if (meta.RoleMetadata.SupportsCreate)
            {
                links["createRole"] = new CreateRoleLink(Url, meta.RoleMetadata);
            }

            return Ok(new 
            {
                Data = data,
                Links = links
            });
        }
    }
}
