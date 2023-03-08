using System.Text.Json.Serialization;

namespace OpenAI.Chat
{
    public sealed class Choice
    {
        [JsonConstructor]
        public Choice(
            Message message,
            string finishReason,
            int index,
            Delta delta)
        {
            Message = message;
            FinishReason = finishReason;
            Index = index;
            Delta = delta;
        }

        [JsonPropertyName("message")]
        public Message Message { get; set; }

        [JsonPropertyName("delta")]
        public Delta Delta { get; set; }


        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; }

        [JsonPropertyName("index")]
        public int Index { get;  set;}

        public override string ToString() => Message.ToString();

        public static implicit operator string(Choice choice) => choice.ToString();
    }
}
