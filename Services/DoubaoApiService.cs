using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartNotes.Services
{
    public class DoubaoApiService
    {
        private readonly HttpClient _httpClient;
        private string _apiKey = string.Empty;
        private string _baseUrl = "https://ark.cn-beijing.volces.com/api/v3";
        private string _model = "ep-20260405132959-vq7kd";

        public DoubaoApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(120);
        }

        public void Configure(string apiKey, string? baseUrl = null, string? model = null)
        {
            _apiKey = apiKey;
            if (!string.IsNullOrEmpty(baseUrl))
            {
                _baseUrl = baseUrl.TrimEnd('/');
            }
            if (!string.IsNullOrEmpty(model))
            {
                _model = model;
            }
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public bool IsConfigured => !string.IsNullOrEmpty(_apiKey);

        public async Task<string> GeneratePlanAsync(string prompt)
        {
            if (!IsConfigured)
            {
                throw new InvalidOperationException("请先配置豆包API密钥");
            }

            var systemPrompt = @"你是一个专业的计划制定助手。请根据用户的需求，制定一个清晰、可执行的计划。
计划应该包含：
1. 明确的目标
2. 具体的步骤/任务
3. 合理的优先级
4. 预估的时间安排

请用简洁的中文输出，使用清晰的格式（如序号、列表等）。";

            var requestBody = new DoubaoRequest
            {
                Model = _model,
                Messages = new[]
                {
                    new Message { Role = "system", Content = systemPrompt },
                    new Message { Role = "user", Content = prompt }
                },
                Temperature = 0.7f,
                MaxTokens = 4096
            };

            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var endpoint = $"{_baseUrl}/chat/completions";

            try
            {
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"API请求失败: {response.StatusCode}\n{responseContent}");
                }

                var responseObject = JsonSerializer.Deserialize<DoubaoResponse>(responseContent);
                if (responseObject?.Choices == null || responseObject.Choices.Length == 0)
                {
                    throw new InvalidOperationException("API返回结果为空");
                }

                return responseObject.Choices[0].Message.Content.Trim();
            }
            catch (Exception ex) when (ex is not HttpRequestException && ex is not JsonException)
            {
                throw new HttpRequestException($"调用豆包API时出错: {ex.Message}", ex);
            }
        }

        public async Task<string> ChatAsync(string message, string? systemPrompt = null)
        {
            if (!IsConfigured)
            {
                throw new InvalidOperationException("请先配置豆包API密钥");
            }

            var messages = new System.Collections.Generic.List<Message>();

            if (!string.IsNullOrEmpty(systemPrompt))
            {
                messages.Add(new Message { Role = "system", Content = systemPrompt });
            }

            messages.Add(new Message { Role = "user", Content = message });

            var requestBody = new DoubaoRequest
            {
                Model = _model,
                Messages = messages.ToArray(),
                Temperature = 0.7f,
                MaxTokens = 4096
            };

            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var endpoint = $"{_baseUrl}/chat/completions";

            try
            {
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"API请求失败: {response.StatusCode}\n{responseContent}");
                }

                var responseObject = JsonSerializer.Deserialize<DoubaoResponse>(responseContent);
                if (responseObject?.Choices == null || responseObject.Choices.Length == 0)
                {
                    throw new InvalidOperationException("API返回结果为空");
                }

                return responseObject.Choices[0].Message.Content.Trim();
            }
            catch (Exception ex) when (ex is not HttpRequestException && ex is not JsonException)
            {
                throw new HttpRequestException($"调用豆包API时出错: {ex.Message}", ex);
            }
        }

        private class DoubaoRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("messages")]
            public Message[] Messages { get; set; } = Array.Empty<Message>();

            [JsonPropertyName("temperature")]
            public float Temperature { get; set; } = 0.7f;

            [JsonPropertyName("max_tokens")]
            public int MaxTokens { get; set; } = 4096;
        }

        private class Message
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty;

            [JsonPropertyName("content")]
            public string Content { get; set; } = string.Empty;
        }

        private class DoubaoResponse
        {
            [JsonPropertyName("choices")]
            public Choice[]? Choices { get; set; }

            [JsonPropertyName("usage")]
            public Usage? Usage { get; set; }
        }

        private class Choice
        {
            [JsonPropertyName("message")]
            public Message Message { get; set; } = new Message();
        }

        private class Usage
        {
            [JsonPropertyName("prompt_tokens")]
            public int PromptTokens { get; set; }

            [JsonPropertyName("completion_tokens")]
            public int CompletionTokens { get; set; }

            [JsonPropertyName("total_tokens")]
            public int TotalTokens { get; set; }
        }
    }
}
