using System.Linq;
using System.Collections.Generic;
using System;

namespace ALK
{
    public class User
    {
        public string? Name { get; set; }
        public string? Number { get; set; }
    }

    public class AppConfig
    {
        public YouTubeConfig? YouTube { get; set; }
        public PushBulletConfig? PushBullet { get; set; }

        public IList<string>? Numbers { get; set; }
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