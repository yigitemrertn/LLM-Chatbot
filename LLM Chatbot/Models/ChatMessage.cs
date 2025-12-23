namespace LLM_Chatbot.Models
{
    /// <summary>
    /// Represents a single message in the conversation
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Message content
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Message sender role (User or Assistant)
        /// </summary>
        public MessageRole Role { get; set; }

        /// <summary>
        /// Timestamp when the message was created
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ChatMessage()
        {
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        public ChatMessage(string content, MessageRole role)
        {
            Content = content;
            Role = role;
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// Returns a display-friendly string representation
        /// </summary>
        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {Role}: {Content}";
        }
    }

    /// <summary>
    /// Enumeration for message roles
    /// </summary>
    public enum MessageRole
    {
        /// <summary>
        /// Message from the user
        /// </summary>
        User = 0,

        /// <summary>
        /// Message from the AI Assistant
        /// </summary>
        Assistant = 1
    }
}
