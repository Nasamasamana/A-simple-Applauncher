using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppLauncher
{
    public class AppItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("path")]
        public string Path { get; set; } = "";
    }

    public class AppManager
    {
        private string AppsJsonPath => System.IO.Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "apps.json");

        public List<AppItem> LoadApps()
        {
            try
            {
                if (!File.Exists(AppsJsonPath))
                    return new List<AppItem>();

                var json = File.ReadAllText(AppsJsonPath);
                return JsonSerializer.Deserialize<List<AppItem>>(json) ?? new List<AppItem>();
            }
            catch
            {
                return new List<AppItem>();
            }
        }

        public void SaveApps(List<AppItem> apps)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(apps, options);
                File.WriteAllText(AppsJsonPath, json);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error saving apps: {ex.Message}");
            }
        }

        public void AddApp(string name, string path)
        {
            var apps = LoadApps();
            apps.Add(new AppItem { Name = name, Path = path });
            SaveApps(apps);
        }

        public void RemoveApp(string name)
        {
            var apps = LoadApps();
            apps.RemoveAll(a => a.Name == name);
            SaveApps(apps);
        }

        public void RenameApp(string oldName, string newName)
        {
            var apps = LoadApps();
            var app = apps.Find(a => a.Name == oldName);
            if (app != null)
            {
                app.Name = newName;
                SaveApps(apps);
            }
        }
    }
}
