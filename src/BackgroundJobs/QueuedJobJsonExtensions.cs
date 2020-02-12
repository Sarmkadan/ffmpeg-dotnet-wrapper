using System.Text.Json;

namespace FFmpegDotnetWrapper.BackgroundJobs
{
    public static class QueuedJobJsonExtensions
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string ToJson(this QueuedJob value, bool indented = false)
        {
            var options = indented
                ? new JsonSerializerOptions(_options) { WriteIndented = true }
                : _options;

            return JsonSerializer.Serialize(value, options);
        }

        public static QueuedJob? FromJson(string json)
        {
            return JsonSerializer.Deserialize<QueuedJob>(json, _options);
        }

        public static bool TryFromJson(string json, out QueuedJob? value)
        {
            try
            {
                value = JsonSerializer.Deserialize<QueuedJob>(json, _options);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}
