using System.Net;

namespace TMOScraper.Core
{
    public enum PageFetchExceptionResult { Failure, Banned, NotFound, RateLimited }

    public class PageFetchException : Exception
    {
        public PageFetchException(string message) : base(message)
        {
        }
    }

    public class PageFetchFailureException : PageFetchException
    {
        public PageFetchFailureException(HttpStatusCode statusCode) : base($"Failed to retrieve page. Status code : {statusCode}") { }
    }

    public class PageFetchBannedException : PageFetchException
    {
        public PageFetchBannedException() : base("403 Forbidden. Your IP might be banned from TMO.") { }
    }

    public class PageFetchNotFoundException : PageFetchException
    {
        public PageFetchNotFoundException() : base("404 Page not found. It might have been moved/deleted or your URL is wrong.") { }
    }

    public class PageFetchRateLimitedException : PageFetchException
    {
        public PageFetchRateLimitedException() : base("Too many requests. Increase the delay(s) if you get this often or you might get banned from TMO.") { }
    }
}
