﻿namespace SecuringWebAPI.Model.DTO
{
    public class LoginUserModel
    {
        public string? username { get; set; }
        public string? password { get; set; }
        public string? grant_type { get; set; }
    }
}
