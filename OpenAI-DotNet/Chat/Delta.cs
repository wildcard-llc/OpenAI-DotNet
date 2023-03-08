using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OpenAI.Chat
{
    public class Delta
    {
        public Delta(string content)
        {
            Content = content ?? "";
        }

        [JsonPropertyName("content")]
        public string Content { get; }
    }
}
