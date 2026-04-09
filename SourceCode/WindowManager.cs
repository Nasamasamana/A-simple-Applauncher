using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace AppLauncher
{
    public class WindowState
    {
        [JsonPropertyName("width")]
        public int Width { get; set; } = 820;

        [JsonPropertyName("height")]
        public int Height { get; set; } = 520;

        [JsonPropertyName("x")]
        public int X { get; set; } = 200;

        [JsonPropertyName("y")]
        public int Y { get; set; } = 120;
    }

    public class WindowManager
    {
        private string WindowJsonPath => Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "window.json");

        public WindowState LoadWindowState()
        {
            try
            {
                if (!File.Exists(WindowJsonPath))
                    return new WindowState();

                var json = File.ReadAllText(WindowJsonPath);
                return JsonSerializer.Deserialize<WindowState>(json) ?? new WindowState();
            }
            catch
            {
                return new WindowState();
            }
        }

        public void SaveWindowState(Form form)
        {
            try
            {
                var state = new WindowState
                {
                    Width = form.Width,
                    Height = form.Height,
                    X = form.Location.X,
                    Y = form.Location.Y
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(state, options);
                File.WriteAllText(WindowJsonPath, json);
            }
            catch { }
        }
    }
}
