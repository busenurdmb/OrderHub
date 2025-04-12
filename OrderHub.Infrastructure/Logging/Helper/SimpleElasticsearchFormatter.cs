using Serilog.Events;
using Serilog.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OrderHub.Infrastructure.Logging.Helper
{
    public class SimpleElasticsearchFormatter : ITextFormatter
    {
        public void Format(LogEvent logEvent, TextWriter output)
        {
            var log = new Dictionary<string, object?>
            {
                ["@timestamp"] = logEvent.Timestamp.UtcDateTime,
                ["level"] = logEvent.Level.ToString(),
                ["message"] = logEvent.RenderMessage()
            };

            foreach (var prop in logEvent.Properties)
            {
                log[prop.Key] = prop.Value.ToString().Trim('"'); // özel alanları da ekle
            }

            var json = JsonSerializer.Serialize(log);
            output.WriteLine(json);
        }
    }
}
