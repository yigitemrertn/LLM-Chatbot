namespace LLM_Chatbot.Services
{
    /// <summary>
    /// Service for interacting with LLM (Large Language Model) APIs
    /// Supports both real OpenAI API and mock service for testing
    /// </summary>
    public class ChatService
    {
        private const string OPENAI_API_URL = "https://api.openai.com/v1/chat/completions";
        private const string OPENAI_API_KEY_NAME = "OpenAI_API_Key";
        private const string DEFAULT_MODEL = "gpt-3.5-turbo";

        private readonly ConfigurationManager _configManager;
        private readonly bool _useMockService;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Gets or sets the model to use
        /// </summary>
        public string Model { get; set; } = DEFAULT_MODEL;

        /// <summary>
        /// Gets or sets the temperature for response generation (0.0 - 2.0)
        /// </summary>
        public double Temperature { get; set; } = 0.7;

        /// <summary>
        /// Gets or sets the maximum tokens for response
        /// </summary>
        public int MaxTokens { get; set; } = 2000;

        /// <summary>
        /// Indicates if the service is currently processing a request
        /// </summary>
        public bool IsProcessing { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="useMockService">If true, uses mock service instead of real API</param>
        public ChatService(bool useMockService = false)
        {
            _configManager = new ConfigurationManager();
            _useMockService = useMockService;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        }

        /// <summary>
        /// Sets the OpenAI API key
        /// </summary>
        public void SetApiKey(string apiKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiKey))
                    throw new ArgumentException("API key cannot be empty");

                _configManager.SetApiKey(OPENAI_API_KEY_NAME, apiKey);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set API key: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sends a message and gets a response from the LLM
        /// </summary>
        public async Task<string> SendMessageAsync(string userMessage, List<Models.ChatMessage> conversationHistory)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                throw new ArgumentException("Message cannot be empty");

            try
            {
                IsProcessing = true;

                if (_useMockService)
                {
                    return await GetMockResponseAsync(userMessage);
                }

                return await GetOpenAIResponseAsync(userMessage, conversationHistory);
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Network error: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new InvalidOperationException($"Request timeout: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error processing message: {ex.Message}", ex);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        /// <summary>
        /// Gets response from OpenAI API
        /// </summary>
        private async Task<string> GetOpenAIResponseAsync(string userMessage, List<Models.ChatMessage> conversationHistory)
        {
            var apiKey = _configManager.GetApiKey(OPENAI_API_KEY_NAME);
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException(
                    "OpenAI API key not configured. Please set it in the application settings.");
            }

            try
            {
                // Prepare messages for API
                var messages = new List<object>();

                // Add conversation history (limit to last 10 messages for context)
                var historyToSend = conversationHistory.TakeLast(10).ToList();
                foreach (var msg in historyToSend)
                {
                    messages.Add(new
                    {
                        role = msg.Role == Models.MessageRole.User ? "user" : "assistant",
                        content = msg.Content
                    });
                }

                var requestBody = new
                {
                    model = Model,
                    messages = messages,
                    temperature = Temperature,
                    max_tokens = MaxTokens
                };

                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(requestBody),
                    System.Text.Encoding.UTF8,
                    "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, OPENAI_API_URL);
                request.Headers.Add("Authorization", $"Bearer {apiKey}");
                request.Content = content;

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new InvalidOperationException(
                            "Invalid API key. Please check your OpenAI API key.");
                    }
                    throw new InvalidOperationException(
                        $"API Error ({response.StatusCode}): {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonDoc = System.Text.Json.JsonDocument.Parse(responseContent);

                var assistantMessage = jsonDoc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return assistantMessage ?? "No response received from API";
            }
            catch (System.Text.Json.JsonException ex)
            {
                throw new InvalidOperationException($"Failed to parse API response: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets mock response for testing without API
        /// </summary>
        private async Task<string> GetMockResponseAsync(string userMessage)
        {
            // Simulate network delay
            await Task.Delay(Random.Shared.Next(1000, 3000));

            // Mock responses based on keywords
            return userMessage.ToLower() switch
            {
                var msg when msg.Contains("hello") || msg.Contains("hi") =>
                    "Hello! ?? I'm your AI assistant. How can I help you today?",

                var msg when msg.Contains("weather") =>
                    "I don't have access to real-time weather data, but you could check a weather service like weather.com for accurate information.",

                var msg when msg.Contains("time") =>
                    $"The current time is {DateTime.Now:HH:mm:ss}.",

                var msg when msg.Contains("who are you") || msg.Contains("what are you") =>
                    "I'm an AI chatbot created to assist and answer your questions. I'm running in mock mode for demonstration purposes.",

                var msg when msg.Contains("joke") =>
                    "Why don't scientists trust atoms? Because they make up everything! ??",

                var msg when msg.Contains("thanks") || msg.Contains("thank you") =>
                    "You're welcome! Is there anything else I can help you with?",

                _ => $"That's an interesting question: \"{userMessage}\". I understood you wanted to know about that topic. In a real scenario, I would provide a more detailed response based on the OpenAI API. For now, this is a mock response to demonstrate the chat interface functionality."
            };
        }

        /// <summary>
        /// Tests the API connection
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                if (_useMockService)
                {
                    return true; // Mock service always works
                }

                var apiKey = _configManager.GetApiKey(OPENAI_API_KEY_NAME);
                if (string.IsNullOrEmpty(apiKey))
                {
                    return false;
                }

                // Try a minimal request
                var testMessage = new List<Models.ChatMessage>
                {
                    new ("Test", Models.MessageRole.User)
                };

                var result = await GetOpenAIResponseAsync("Test", testMessage);
                return !string.IsNullOrEmpty(result);
            }
            catch
            {
                return false;
            }
        }
    }
}
