using System.Text.Json.Serialization;

namespace OpenAI.Chat
{
    public sealed class Message
    {
        [JsonInclude]
        [JsonPropertyName("role")]
        public string Role { get; private set; }

        [JsonInclude]
        [JsonPropertyName("content")]
        public string Content { get; private set; }

        public override string ToString() => Content;

        public static implicit operator string(Message message) => message.Content;
    }
}
