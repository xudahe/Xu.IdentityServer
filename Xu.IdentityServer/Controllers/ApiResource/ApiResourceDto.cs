﻿namespace Xu.IdentityServer.Controllers.ApiResource
{
    public class ApiResourceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string UserClaims { get; set; }
        public string Scopes { get; set; }
    }
}