﻿namespace SecuringWebAPI.Model.DTO
{
    public class AuthSuccessResponse
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
