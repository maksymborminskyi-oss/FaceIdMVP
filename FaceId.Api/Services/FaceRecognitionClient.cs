using System.Net.Http.Headers;
using System.Text.Json;

namespace FaceId.Api.Servises;

public class FaceRecognitionClient
{
    private readonly HttpClient _httpClient;

    public FaceRecognitionClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<float[]> ExtractEmbeddingAsync(
        Stream imageStream,
        string fileName,
        CancellationToken cancellationToken = default)

    {
        using var multipartContent = new MultipartFormDataContent();
        
        using var fileContent = new StreamContent(imageStream);

        fileContent.Headers.ContentType = 
            new MediaTypeHeaderValue("application/octet-stream");

        multipartContent.Add(
            fileContent,
            "file",
            fileName);
        
        using var response = await _httpClient.PostAsync(
            "/extract-embedding",
            multipartContent,
            cancellationToken);
        
        var responseBody = await response.Content.ReadAsStringAsync(
            cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Python service returned {(int)response.StatusCode}: " +
                responseBody,
                null,
                response.StatusCode);
        }

        var result = JsonSerializer.Deserialize<EmbeddingResponse>(
            responseBody,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (result?.Embedding is null || result.Embedding.Length == 0)
        {
            throw new InvalidOperationException(
                "Python service returned an empty embedding.");
        }

        if (result.Dimensions != result.Embedding.Length)
        {
            throw new InvalidOperationException(
                "Embedding dimensions do not match the vector length.");
        }

        return result.Embedding;
    }

    private sealed class EmbeddingResponse
    {
        public float[] Embedding { get; set; } = [];

        public int Dimensions { get; set; }
    }
}