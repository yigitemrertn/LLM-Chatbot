using System.Security.Cryptography;
using System.Text;

namespace LLM_Chatbot.Services
{
    /// <summary>
    /// Manages application configuration and secure API key storage
    /// </summary>
    public class ConfigurationManager
    {
        private static readonly string ConfigDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LLM_Chatbot");

        private static readonly string ConfigFilePath = Path.Combine(ConfigDirectory, "config.json");
        private static readonly string KeyFile = Path.Combine(ConfigDirectory, "keystore");

        private Dictionary<string, string> _config = [];

        public ConfigurationManager()
        {
            EnsureConfigDirectory();
            LoadConfiguration();
        }

        /// <summary>
        /// Ensures the configuration directory exists
        /// </summary>
        private void EnsureConfigDirectory()
        {
            try
            {
                if (!Directory.Exists(ConfigDirectory))
                {
                    Directory.CreateDirectory(ConfigDirectory);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating config directory: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads configuration from file
        /// </summary>
        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    _config = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves configuration to file
        /// </summary>
        private void SaveConfiguration()
        {
            try
            {
                string json = System.Text.Json.JsonSerializer.Serialize(_config, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving configuration: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets an API key securely
        /// </summary>
        public void SetApiKey(string keyName, string keyValue)
        {
            try
            {
                if (string.IsNullOrEmpty(keyName) || string.IsNullOrEmpty(keyValue))
                    throw new ArgumentException("Key name and value cannot be empty");

                _config[keyName] = EncryptData(keyValue);
                SaveConfiguration();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting API key: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets an API key securely
        /// </summary>
        public string? GetApiKey(string keyName)
        {
            try
            {
                if (_config.TryGetValue(keyName, out var encryptedValue))
                {
                    return DecryptData(encryptedValue);
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting API key: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Checks if an API key exists
        /// </summary>
        public bool HasApiKey(string keyName)
        {
            return _config.ContainsKey(keyName);
        }

        /// <summary>
        /// Removes an API key
        /// </summary>
        public void RemoveApiKey(string keyName)
        {
            try
            {
                if (_config.ContainsKey(keyName))
                {
                    _config.Remove(keyName);
                    SaveConfiguration();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing API key: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a configuration value
        /// </summary>
        public string? GetValue(string key)
        {
            return _config.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Sets a configuration value
        /// </summary>
        public void SetValue(string key, string value)
        {
            try
            {
                _config[key] = value;
                SaveConfiguration();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting configuration value: {ex.Message}");
            }
        }

        /// <summary>
        /// Encrypts data using DPAPI
        /// </summary>
        private string EncryptData(string data)
        {
            try
            {
                byte[] dataToEncrypt = Encoding.UTF8.GetBytes(data);
                byte[] encryptedData = ProtectedData.Protect(dataToEncrypt, null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Encryption error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Decrypts data using DPAPI
        /// </summary>
        private string DecryptData(string encryptedData)
        {
            try
            {
                byte[] dataToDecrypt = Convert.FromBase64String(encryptedData);
                byte[] decryptedData = ProtectedData.Unprotect(dataToDecrypt, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decryptedData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Decryption error: {ex.Message}");
                throw;
            }
        }
    }
}
