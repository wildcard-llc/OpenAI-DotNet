using OpenAI.Completions;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.Chat
{
    public sealed class ChatEndpoint : BaseEndPoint
    {
        public ChatEndpoint(OpenAIClient api) : base(api) { }

        protected override string GetEndpoint()
            => $"{Api.BaseUrl}chat";

        /// <summary>
        /// Creates a completion for the chat message
        /// </summary>
        /// <param name="chatRequest">The chat request which contains the message content.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="ChatResponse"/>.</returns>
        public async Task<ChatResponse> GetCompletionAsync(ChatRequest chatRequest, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(chatRequest, Api.JsonSerializationOptions);
            var payload = json.ToJsonStringContent();
            var result = await Api.Client.PostAsync($"{GetEndpoint()}/completions", payload, cancellationToken);
            var resultAsString = await result.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<ChatResponse>(resultAsString, Api.JsonSerializationOptions);
        }

        // TODO Streaming endpoints
        /// <summary>
        /// Ask the API to complete the prompt(s) using the specified request,
        /// and stream the results to the <paramref name="resultHandler"/> as they come in.
        /// </summary>
        /// <param name="completionRequest">The request to send to the API.</param>
        /// <param name="resultHandler">An action to be called as each new result arrives.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <exception cref="HttpRequestException">Raised when the HTTP request fails</exception>
        public async Task StreamCompletionAsync(ChatRequest completionRequest, Action<ChatResponse> resultHandler, CancellationToken cancellationToken = default)
        {
            completionRequest.Stream = true;
            var jsonContent = JsonSerializer.Serialize(completionRequest, Api.JsonSerializationOptions);
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{GetEndpoint()}/completions")
            {
                Content = jsonContent.ToJsonStringContent()
            };
            var response = await Api.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            await response.CheckResponseAsync(cancellationToken).ConfigureAwait(false);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var reader = new StreamReader(stream);
            while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line)
            {
                
                if (line.StartsWith("data: "))
                {
                    line = line["data: ".Length..];
                }

                if (line == "[DONE]")
                {
                    return;
                }

                if (!string.IsNullOrWhiteSpace(line))
                {
                    resultHandler(DeserializeResult(response, line.Trim()));
                }
            }

        }


        private ChatResponse DeserializeResult(HttpResponseMessage response, string json)
        {
            var result = JsonSerializer.Deserialize<ChatResponse>(json, Api.JsonSerializationOptions);
            Console.WriteLine(result.Choices.First().Delta?.Content);
            if (result?.Choices == null || result.Choices.Count == 0)
            {
                throw new HttpRequestException($"{nameof(DeserializeResult)} no completions! HTTP status code: {response.StatusCode}. Response body: {json}");
            }
            result.SetResponseData(response.Headers);
            return result;
        }
    }
}
