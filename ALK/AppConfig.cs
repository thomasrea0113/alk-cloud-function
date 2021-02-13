using System;

namespace ALK
{
    public class AppConfig
    {
        public YouTubeConfig? YouTube { get; set; }
        public PushBulletConfig? PushBullet { get; set; }
        public string? AllansNumber { get; set; }
        public Uri? SiteUri { get; set; }
    }

    public class YouTubeConfig
    {
        public bool SkipSend { get; set; }
        public string? ApiKey { get; set; }
        public int? PageCount { get; set; }
    }

    public class PushBulletConfig
    {
        public bool SkipSend { get; set; }
        public string? ApiKey { get; set; }
        public string? DeviceIdentifier { get; set; }
    }
}