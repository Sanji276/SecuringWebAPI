namespace SecuredWebAPIBestPractices.Extensions
{
    public static class General
    {
        public static string GetUserId(this HttpContext httpContext)
        {
            if(httpContext.User == null)
            {
                return string.Empty;
            }
            return httpContext.User.Claims.Single(x=>x.Type == "Id").Value;
        }
    }
}
