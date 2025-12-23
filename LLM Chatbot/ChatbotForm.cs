namespace LLM_Chatbot
{
    public partial class ChatbotForm : Form
    {
        private readonly Services.ChatService _chatService;
        private readonly Models.ConversationContext _conversationContext;
        
        // Kontrol referanslarý
        private TextBox? _txtInput;
        private RichTextBox? _rtbChat;
        private Label? _lblStatus;

        public ChatbotForm()
        {
            InitializeComponent();
            
            _chatService = new Services.ChatService(useMockService: false);
            _conversationContext = new Models.ConversationContext();
            
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "LLM Chatbot - Modern Chat Interface";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 242, 245);

            // Create main layout
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // Chat display area
            var chatDisplayPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Height = 400
            };

            _rtbChat = new RichTextBox
            {
                Name = "rtbChat",
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.None,
                Padding = new Padding(10),
                WordWrap = true
            };
            
            chatDisplayPanel.Controls.Add(_rtbChat);

            // Input area
            var inputPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                Padding = new Padding(0, 10, 0, 0)
            };

            _txtInput = new TextBox
            {
                Name = "txtInput",
                Dock = DockStyle.Fill,
                Multiline = true,
                Font = new Font("Segoe UI", 10),
                ScrollBars = ScrollBars.Vertical,
                PlaceholderText = "Mesajýnýzý yazýn... (Ctrl+Enter veya Gönder butonuna týklayýn)"
            };

            inputPanel.Controls.Add(_txtInput);

            // Button panel
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                AutoSize = false,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 5, 0, 0),
                BackColor = Color.FromArgb(240, 242, 245)
            };

            var btnSend = new Button
            {
                Name = "btnSend",
                Text = "Gönder",
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += BtnSend_Click;

            var btnClear = new Button
            {
                Name = "btnClear",
                Text = "Temizle",
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(107, 114, 128),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(5, 0, 0, 0)
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += BtnClear_Click;

            var btnSettings = new Button
            {
                Name = "btnSettings",
                Text = "Ayarlar",
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(168, 85, 247),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(5, 0, 0, 0)
            };
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.Click += BtnSettings_Click;

            buttonPanel.Controls.Add(btnSend);
            buttonPanel.Controls.Add(btnClear);
            buttonPanel.Controls.Add(btnSettings);

            inputPanel.Controls.Add(buttonPanel);

            // Status bar
            var statusPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.FromArgb(31, 41, 55),
                Padding = new Padding(10, 5, 10, 5)
            };

            _lblStatus = new Label
            {
                Name = "lblStatus",
                Text = "Hazýr",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };

            statusPanel.Controls.Add(_lblStatus);

            // Add all to main panel
            mainPanel.Controls.Add(chatDisplayPanel);
            mainPanel.Controls.Add(inputPanel);
            mainPanel.Controls.Add(statusPanel);

            this.Controls.Add(mainPanel);

            // Handle Ctrl+Enter for send
            _txtInput.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.Enter)
                {
                    BtnSend_Click(btnSend, EventArgs.Empty);
                    e.Handled = true;
                }
            };
        }

        private async void BtnSend_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_txtInput == null || string.IsNullOrWhiteSpace(_txtInput.Text))
                {
                    MessageBox.Show("Lütfen bir mesaj giriniz.", "Boþ Mesaj", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string userMessage = _txtInput.Text.Trim();
                _txtInput.Text = string.Empty;
                _txtInput.Focus();

                AddMessageToChat(userMessage, Models.MessageRole.User);
                _conversationContext.AddMessage(new Models.ChatMessage(userMessage, Models.MessageRole.User));

                UpdateStatus("Ýþleniyor...", Color.FromArgb(251, 191, 36));

                string response = await _chatService.SendMessageAsync(userMessage, _conversationContext.GetContextMessages());

                AddMessageToChat(response, Models.MessageRole.Assistant);
                _conversationContext.AddMessage(new Models.ChatMessage(response, Models.MessageRole.Assistant));

                UpdateStatus("Hazýr", Color.Green);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Hata: {ex.Message}", Color.FromArgb(239, 68, 68));
                MessageBox.Show($"Bir hata oluþtu:\n\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            if (_rtbChat != null)
            {
                _rtbChat.Clear();
                _conversationContext.Clear();
                UpdateStatus("Konuþma temizlendi", Color.Green);
            }
        }

        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            var settingsForm = new SettingsForm(_chatService);
            settingsForm.ShowDialog(this);
        }

        private void AddMessageToChat(string message, Models.MessageRole role)
        {
            if (_rtbChat == null) return;

            Color messageColor = role == Models.MessageRole.User ? 
                Color.FromArgb(59, 130, 246) : Color.FromArgb(34, 197, 94);

            string roleName = role == Models.MessageRole.User ? "Siz" : "Asistan";
            string timeStamp = DateTime.Now.ToString("HH:mm:ss");

            _rtbChat.SelectionStart = _rtbChat.TextLength;
            _rtbChat.SelectionLength = 0;

            _rtbChat.SelectionColor = Color.FromArgb(107, 114, 128);
            _rtbChat.AppendText($"[{timeStamp}] {roleName}:\n");

            _rtbChat.SelectionColor = messageColor;
            _rtbChat.AppendText($"{message}\n");

            _rtbChat.SelectionColor = _rtbChat.ForeColor;
            _rtbChat.AppendText("\n");

            _rtbChat.ScrollToCaret();
        }

        private void UpdateStatus(string message, Color color)
        {
            if (_lblStatus != null)
            {
                _lblStatus.Text = message;
                _lblStatus.ForeColor = color;
            }
        }
    }
}
