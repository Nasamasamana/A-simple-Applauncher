using System;
using System.Drawing;
using System.Windows.Forms;

namespace AppLauncher
{
    public partial class SettingsForm : Form
    {
        private AppManager appManager;
        private MainForm parentForm;
        private ListBox appListBox;
        private Button addButton;
        private Button removeButton;
        private Button renameButton;
        private Button exitButton;

        public SettingsForm(AppManager manager, MainForm parent)
        {
            appManager = manager;
            parentForm = parent;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "AppLauncher Settings";
            this.Size = new Size(400, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var label = new Label
            {
                Text = "Installed Apps:",
                Location = new Point(10, 10),
                AutoSize = true
            };

            appListBox = new ListBox
            {
                Location = new Point(10, 30),
                Size = new Size(360, 250),
                SelectionMode = SelectionMode.One
            };

            ReloadAppList();

            addButton = new Button
            {
                Text = "Add App",
                Location = new Point(10, 290),
                Size = new Size(80, 30)
            };
            addButton.Click += AddButton_Click;

            removeButton = new Button
            {
                Text = "Remove",
                Location = new Point(100, 290),
                Size = new Size(80, 30)
            };
            removeButton.Click += RemoveButton_Click;

            renameButton = new Button
            {
                Text = "Rename",
                Location = new Point(190, 290),
                Size = new Size(80, 30)
            };
            renameButton.Click += RenameButton_Click;

            exitButton = new Button
            {
                Text = "Exit",
                Location = new Point(290, 290),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };

            panel.Controls.Add(label);
            panel.Controls.Add(appListBox);
            panel.Controls.Add(addButton);
            panel.Controls.Add(removeButton);
            panel.Controls.Add(renameButton);
            panel.Controls.Add(exitButton);

            this.Controls.Add(panel);
        }

        private void ReloadAppList()
        {
            appListBox.Items.Clear();
            var apps = appManager.LoadApps();
            foreach (var app in apps)
                appListBox.Items.Add(app.Name);
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Executable Files (*.exe;*.lnk;*.bat;*.cmd)|*.exe;*.lnk;*.bat;*.cmd|All Files (*.*)|*.*",
                Title = "Select an application or shortcut"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var fileName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                appManager.AddApp(fileName, dialog.FileName);
                ReloadAppList();
            }
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (appListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select an app to remove.");
                return;
            }

            var appName = appListBox.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(appName))
            {
                appManager.RemoveApp(appName);
                ReloadAppList();
            }
        }

        private void RenameButton_Click(object sender, EventArgs e)
        {
            if (appListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select an app to rename.");
                return;
            }

            var oldName = appListBox.SelectedItem.ToString();
            if (string.IsNullOrEmpty(oldName))
                return;

            var newName = PromptForInput("Enter new name:", oldName);

            if (!string.IsNullOrEmpty(newName) && newName != oldName)
            {
                appManager.RenameApp(oldName, newName);
                ReloadAppList();
            }
        }

        private string PromptForInput(string prompt, string defaultValue = "")
        {
            var form = new Form
            {
                Text = "Rename App",
                Width = 300,
                Height = 150,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label { Text = prompt, Left = 20, Top = 20, Width = 250 };
            var textBox = new TextBox { Text = defaultValue, Left = 20, Top = 50, Width = 250 };
            var okButton = new Button { Text = "OK", Left = 120, Top = 80, Width = 75, DialogResult = DialogResult.OK };
            var cancelButton = new Button { Text = "Cancel", Left = 200, Top = 80, Width = 75, DialogResult = DialogResult.Cancel };

            form.Controls.Add(label);
            form.Controls.Add(textBox);
            form.Controls.Add(okButton);
            form.Controls.Add(cancelButton);
            form.AcceptButton = okButton;
            form.CancelButton = cancelButton;

            return form.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
