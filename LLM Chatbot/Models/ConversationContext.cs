namespace LLM_Chatbot.Models
{
    /// <summary>
    /// Manages conversation history and context awareness
    /// </summary>
    public class ConversationContext
    {
        private readonly List<ChatMessage> _messages = [];
        private const int MAX_CONTEXT_MESSAGES = 20;
        private const int MAX_TOKENS_ESTIMATE = 4000;

        /// <summary>
        /// Gets the list of all messages in the conversation
        /// </summary>
        public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();

        /// <summary>
        /// Gets the number of messages in the conversation
        /// </summary>
        public int MessageCount => _messages.Count;

        /// <summary>
        /// Gets the conversation ID for tracking
        /// </summary>
        public string ConversationId { get; }

        /// <summary>
        /// Gets the creation time of the conversation
        /// </summary>
        public DateTime CreatedAt { get; }

        /// <summary>
        /// Gets the last message timestamp
        /// </summary>
        public DateTime? LastMessageTime => _messages.Count > 0 ? _messages[^1].Timestamp : null;

        public ConversationContext()
        {
            ConversationId = Guid.NewGuid().ToString();
            CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// Adds a message to the conversation history
        /// </summary>
        public void AddMessage(ChatMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            _messages.Add(message);

            // Keep only recent messages to manage context
            if (_messages.Count > MAX_CONTEXT_MESSAGES)
            {
                _messages.RemoveAt(0);
            }
        }

        /// <summary>
        /// Gets the recent conversation context for API requests
        /// </summary>
        public List<ChatMessage> GetContextMessages()
        {
            return new List<ChatMessage>(_messages);
        }

        /// <summary>
        /// Gets formatted context for API (e.g., OpenAI format)
        /// </summary>
        public string GetFormattedContext()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var message in _messages)
            {
                sb.AppendLine($"{message.Role}: {message.Content}");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// Clears all messages from the conversation
        /// </summary>
        public void Clear()
        {
            _messages.Clear();
        }

        /// <summary>
        /// Gets summary statistics about the conversation
        /// </summary>
        public string GetStatistics()
        {
            var userMessages = _messages.Count(m => m.Role == MessageRole.User);
            var assistantMessages = _messages.Count(m => m.Role == MessageRole.Assistant);
            var totalWords = _messages.Sum(m => m.Content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length);

            return $"Conversation Stats:\n" +
                   $"Total Messages: {MessageCount}\n" +
                   $"User Messages: {userMessages}\n" +
                   $"Assistant Messages: {assistantMessages}\n" +
                   $"Total Words: {totalWords}\n" +
                   $"Duration: {(LastMessageTime.HasValue ? (LastMessageTime.Value - CreatedAt).TotalMinutes.ToString("F1") : "0")} minutes";
        }
    }
}
