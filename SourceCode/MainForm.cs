using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AppLauncher
{
    public partial class MainForm : Form
    {
        private AppManager appManager;
        private WindowManager windowManager;
        private NotifyIcon trayIcon;
        private FlowLayoutPanel flowPanel;
        private Button settingsButton;
        private List<AppItem> apps;

        public MainForm()
        {
            appManager = new AppManager();
            windowManager = new WindowManager();
            apps = appManager.LoadApps();

            InitializeComponent();
            SetupUI();
            SetupTray();
            RestoreWindowState();
        }

        private void InitializeComponent()
        {
            this.Text = "A simple lightweight AppLauncher";
            this.Size = new Size(820, 520);
            this.MinimumSize = new Size(400, 300);
            this.Icon = SystemIcons.Application;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterScreen;

            this.FormClosing += MainForm_FormClosing;
            this.SizeChanged += MainForm_SizeChanged;
        }

        private void SetupUI()
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SystemColors.Control
            };

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = SystemColors.Control
            };

            settingsButton = new Button
            {
                Text = "⚙ Settings",
                Location = new Point(5, 5),
                Size = new Size(90, 30),
                FlatStyle = FlatStyle.System
            };
            settingsButton.Click += SettingsButton_Click;
            headerPanel.Controls.Add(settingsButton);

            flowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = SystemColors.Control,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(5)
            };

            mainPanel.Controls.Add(flowPanel);
            mainPanel.Controls.Add(headerPanel);

            this.Controls.Add(mainPanel);

            RefreshAppGrid();
        }

        private void SetupTray()
        {
            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "AppLauncher"
            };

            trayIcon.MouseClick += TrayIcon_MouseClick;

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Restore", null, (s, e) => ShowWindow());
            contextMenu.Items.Add("Exit", null, (s, e) => ExitApp());

            trayIcon.ContextMenuStrip = contextMenu;
        }

        private void RefreshAppGrid()
        {
            flowPanel.Controls.Clear();

            foreach (var app in apps)
            {
                var appControl = new Panel
                {
                    Size = new Size(80, 100),
                    BorderStyle = BorderStyle.None,
                    Cursor = Cursors.Hand
                };

                var iconBox = new PictureBox
                {
                    Image = ExtractIcon(app.Path),
                    Size = new Size(64, 64),
                    Location = new Point(8, 8),
                    SizeMode = PictureBoxSizeMode.CenterImage,
                    Cursor = Cursors.Hand
                };

                var label = new Label
                {
                    Text = app.Name,
                    Location = new Point(0, 75),
                    Size = new Size(80, 25),
                    TextAlign = ContentAlignment.TopCenter,
                    Font = new Font("Segoe UI", 9),
                    AutoEllipsis = true,
                    Cursor = Cursors.Hand
                };

                appControl.Controls.Add(iconBox);
                appControl.Controls.Add(label);

                iconBox.DoubleClick += (s, e) => LaunchApp(app);
                label.DoubleClick += (s, e) => LaunchApp(app);
                appControl.DoubleClick += (s, e) => LaunchApp(app);

                flowPanel.Controls.Add(appControl);
            }
        }

        private Image ExtractIcon(string path)
        {
            try
            {
                string targetPath = path;
                
                if (Path.GetExtension(path).ToLower() == ".lnk")
                {
                    targetPath = ResolveLnkTarget(path);
                }

                var icon = Icon.ExtractAssociatedIcon(targetPath);
                return icon?.ToBitmap() ?? CreateDefaultIcon();
            }
            catch
            {
                return CreateDefaultIcon();
            }
        }

        private string ResolveLnkTarget(string lnkPath)
        {
            try
            {
                using (var fs = new FileStream(lnkPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);

                    string content = System.Text.Encoding.ASCII.GetString(buffer);
                    
                    foreach (var ext in new[] { ".exe", ".bat", ".cmd", ".com" })
                    {
                        int idx = content.IndexOf(ext, StringComparison.OrdinalIgnoreCase);
                        if (idx > 0)
                        {
                            int start = Math.Max(0, idx - 260);
                            string substring = content.Substring(start, Math.Min(300, content.Length - start));
                            
                            int lastIdx = substring.LastIndexOf(ext, StringComparison.OrdinalIgnoreCase);
                            if (lastIdx > 0)
                            {
                                int pathStart = 0;
                                for (int i = lastIdx - 1; i >= 0; i--)
                                {
                                    if (substring[i] == '\0' || substring[i] == ':')
                                    {
                                        pathStart = (substring[i] == ':') ? i - 1 : i + 1;
                                        while (pathStart < substring.Length && substring[pathStart] == '\0')
                                            pathStart++;
                                        break;
                                    }
                                }
                                
                                string extracted = substring.Substring(pathStart, lastIdx + ext.Length - pathStart);
                                extracted = extracted.Trim('\0');
                                
                                if (File.Exists(extracted) && extracted.Contains(ext))
                                    return extracted;
                            }
                        }
                    }
                }
            }
            catch { }

            return lnkPath;
        }

        private Image CreateDefaultIcon()
        {
            var bmp = new Bitmap(64, 64);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.LightGray);
                using (var brush = new SolidBrush(Color.DarkGray))
                {
                    g.DrawString("?", new Font("Arial", 32, FontStyle.Bold), brush, 15, 10);
                }
            }
            return bmp;
        }

        private void LaunchApp(AppItem app)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = app.Path,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error launching {app.Name}: {ex.Message}");
            }
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            var settingsForm = new SettingsForm(appManager, this);
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                apps = appManager.LoadApps();
                RefreshAppGrid();
            }
        }

        private void TrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                ShowWindow();
        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
                this.WindowState = FormWindowState.Minimized;
                windowManager.SaveWindowState(this);
            }
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
                windowManager.SaveWindowState(this);
        }

        private void RestoreWindowState()
        {
            var state = windowManager.LoadWindowState();
            this.Size = new Size(state.Width, state.Height);
            this.Location = new Point(state.X, state.Y);
        }

        private void ExitApp()
        {
            windowManager.SaveWindowState(this);
            trayIcon?.Dispose();
            this.Dispose();
            Application.Exit();
        }
    }
}
