namespace LLM_Chatbot
{
    public partial class SettingsForm : Form
    {
        private readonly Services.ChatService _chatService;
        private TextBox? _txtApiKey;
        private ComboBox? _cmbModel;
        private TrackBar? _trkTemperature;

        public SettingsForm(Services.ChatService chatService)
        {
            _chatService = chatService;
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Ayarlar";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            int yPos = 20;

            // Title
            var lblTitle = new Label
            {
                Text = "API Ayarlarý",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(0, yPos),
                AutoSize = true
            };
            mainPanel.Controls.Add(lblTitle);
            yPos += 40;

            // API Key Label
            var lblApiKey = new Label
            {
                Text = "OpenAI API Anahtarý:",
                AutoSize = true,
                Location = new Point(0, yPos)
            };
            mainPanel.Controls.Add(lblApiKey);
            yPos += 25;

            // API Key Input
            _txtApiKey = new TextBox
            {
                Width = 400,
                Height = 40,
                PasswordChar = '*',
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, yPos)
            };
            mainPanel.Controls.Add(_txtApiKey);
            yPos += 50;

            // Model Label
            var lblModel = new Label
            {
                Text = "Model:",
                AutoSize = true,
                Location = new Point(0, yPos)
            };
            mainPanel.Controls.Add(lblModel);
            yPos += 25;

            // Model Combo Box
            _cmbModel = new ComboBox
            {
                Width = 400,
                Location = new Point(0, yPos),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbModel.Items.AddRange(new object[] { "gpt-4", "gpt-3.5-turbo", "gpt-3.5-turbo-16k" });
            _cmbModel.SelectedItem = _chatService.Model;
            mainPanel.Controls.Add(_cmbModel);
            yPos += 50;

            // Temperature Label
            var lblTemperature = new Label
            {
                Text = $"Sýcaklýk (0.0 - 2.0): {_chatService.Temperature:F2}",
                AutoSize = true,
                Location = new Point(0, yPos)
            };
            mainPanel.Controls.Add(lblTemperature);
            yPos += 25;

            // Temperature Slider
            _trkTemperature = new TrackBar
            {
                Minimum = 0,
                Maximum = 200,
                Value = (int)(_chatService.Temperature * 100),
                Width = 400,
                Location = new Point(0, yPos)
            };
            _trkTemperature.ValueChanged += (s, e) =>
            {
                var temp = _trkTemperature.Value / 100.0;
                lblTemperature.Text = $"Sýcaklýk (0.0 - 2.0): {temp:F2}";
            };
            mainPanel.Controls.Add(_trkTemperature);

            // Button Panel at the bottom
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                AutoSize = false,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0),
                BackColor = Color.FromArgb(240, 242, 245)
            };

            var btnSave = new Button
            {
                Text = "Kaydet",
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(5, 0, 0, 0)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            var btnCancel = new Button
            {
                Text = "Ýptal",
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(107, 114, 128),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 0)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            var btnTestConnection = new Button
            {
                Text = "Baðlantýyý Test Et",
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(59, 130, 246),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(5, 0, 0, 0)
            };
            btnTestConnection.FlatAppearance.BorderSize = 0;
            btnTestConnection.Click += BtnTestConnection_Click;

            buttonPanel.Controls.Add(btnSave);
            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Controls.Add(btnTestConnection);

            // Add all to main panel
            mainPanel.Controls.Add(buttonPanel);

            this.Controls.Add(mainPanel);
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_txtApiKey != null && !string.IsNullOrWhiteSpace(_txtApiKey.Text))
                {
                    _chatService.SetApiKey(_txtApiKey.Text);
                }

                if (_cmbModel != null && _cmbModel.SelectedItem != null)
                {
                    _chatService.Model = _cmbModel.SelectedItem.ToString() ?? "gpt-3.5-turbo";
                }

                if (_trkTemperature != null)
                {
                    _chatService.Temperature = _trkTemperature.Value / 100.0;
                }

                MessageBox.Show("Ayarlar baþarýyla kaydedildi!", "Baþarýlý", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ayarlar kaydedilirken hata oluþtu:\n\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnTestConnection_Click(object? sender, EventArgs e)
        {
            try
            {
                var btnTestConnection = sender as Button;
                if (btnTestConnection != null)
                    btnTestConnection.Enabled = false;

                MessageBox.Show("Baðlantý test ediliyor...", "Test Ediliyor", MessageBoxButtons.OK, MessageBoxIcon.Information);

                bool isConnected = await _chatService.TestConnectionAsync();

                if (isConnected)
                {
                    MessageBox.Show("Baðlantý baþarýlý!", "Baþarýlý", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Baðlantý baþarýsýz. Lütfen API anahtarýnýzý kontrol edin.", "Baþarýsýz", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (btnTestConnection != null)
                    btnTestConnection.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Baðlantý test edilirken hata oluþtu:\n\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
