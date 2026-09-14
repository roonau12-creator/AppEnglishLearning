namespace AppLearningEnglish.Models
{
    public static class MediaUrlHelper
    {
        public static string? ToYoutubeEmbed(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            url = url.Trim();

            if (url.Contains("youtube.com/embed/", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }

            string? id = null;

            if (url.Contains("youtu.be/", StringComparison.OrdinalIgnoreCase))
            {
                var start = url.IndexOf("youtu.be/", StringComparison.OrdinalIgnoreCase) + 9;
                id = url[start..].Split(new[] { '?', '&', '/' }, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault();
            }
            else if (url.Contains("v=", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var part in url.Split(new[] { '?', '&' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (part.StartsWith("v=", StringComparison.OrdinalIgnoreCase))
                    {
                        id = part[2..];
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return $"https://www.youtube.com/embed/{id}";
        }
    }
}
