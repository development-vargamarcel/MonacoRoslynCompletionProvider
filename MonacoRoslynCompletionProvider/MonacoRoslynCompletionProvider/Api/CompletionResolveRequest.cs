using System.Text.Json.Serialization;

namespace MonacoRoslynCompletionProvider.Api
{
    public class CompletionResolveRequest : TabCompletionRequest
    {
        [JsonPropertyName("Suggestion")]
        public string Suggestion { get; set; }
    }
}
