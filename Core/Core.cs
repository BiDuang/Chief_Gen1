using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using MahApps.Metro.Controls;
using Nett;
using Newtonsoft.Json.Linq;

namespace Chief.Core
{
    public static class Requests
    {

        public static async Task<List<Models.Core.CommitInfo>> GetReleaseInfo()
        {
            var client = new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(6),
            };

            var response =
                await client.GetAsync(new Uri("https://git.cinogama.net/api/v4/projects/68/repository/tags"));
            response.EnsureSuccessStatusCode();

            var resp = await response.Content.ReadAsStringAsync();

            var data = JArray.Parse(resp);
            var commits =
                from item in data
                select (JObject)item;

            return commits.Select(item => new Models.Core.CommitInfo()
            {
                Version = (string)item["name"]!,
                ID = (string)item["target"]!,
                RelaeseMessage = (string)item["release"]!["description"]!,
                ReleaseDateTime = DateTime.Parse((string)item["commit"]!["created_at"]!),
                Author = (string)item["commit"]!["author_name"]!
            })
                .ToList();
        }
    }

    public static class SystemInfo
    {
        public static string GetSystemThemeColor()
        {
            RegistryKey themeReg =
                Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\DWM")!;
            int themeColor = (int)themeReg.GetValue("ColorizationColor")!;

            return themeColor.ToString("X")[2..];
        }

        public static bool IsLightTheme()
        {
            RegistryKey themeReg =
                Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize")!;

            return (int)themeReg.GetValue("AppsUseLightTheme")! == 1;
        }

        public static bool IsWoolangInstalled()
        {
            var cmd = new ProcessStartInfo("woodriver")
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = new Process()
            {
                StartInfo = cmd
            };
            try
            {
                process.Start();
            }
            catch
            {
                return false;
            }

            return process.StandardOutput.ReadToEnd().Trim().Replace("\u001b[0m", "").StartsWith("Woolang");
        }


        public static string GetWoolangDir()
        {
            var cmd = new ProcessStartInfo("where", "woodriver.exe")
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = new Process()
            {
                StartInfo = cmd
            };

            process.Start();

            return process.StandardOutput.ReadToEnd().Trim().Split("\r\n")[0].Replace("woodriver.exe", "");
        }
    }

    public static class ChiefConfigs
    {
        public static bool AnimationEnable = true;
        public static void InitConfig()
        {
            TomlTable configs;
            try
            {
                configs = Toml.ReadFile("config.toml");
            }
            catch
            {
                const string initConfigs = @"
                    [Animation]
                    enable = true";
                configs = Toml.ReadString(initConfigs);
                Toml.WriteFile(configs, "config.toml");
            }
            AnimationEnable = configs.Get<TomlTable>("Animation")["enable"].Get<bool>();
        }

        public static void SaveConfig()
        {
            var configs = Toml.Create();
            configs.Add("Animation", Toml.Create());
            configs.Get<TomlTable>("Animation").Add("enable", AnimationEnable);
            Toml.WriteFile(configs, "config.toml");
        }
    }

    public class AnimationControl
    {
        public void FadeSwitch(Page nowPage, Page target, double fadeTime = 0.5)
        {
            foreach (var control in nowPage.FindChildren<Control>())
            {
                control.IsEnabled = false;
            }

            var window = Window.GetWindow(nowPage);
            var contentControl = window!.FindName("ContentControl") as ContentControl;

            if (!ChiefConfigs.AnimationEnable)
            {
                contentControl!.Content = new Frame()
                {
                    Content = target
                };
                return;
            }

            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(fadeTime));
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(fadeTime));
            fadeOut.Completed += (_, _) =>
            {
                contentControl!.Content = new Frame()
                {
                    Content = target
                };
                contentControl!.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
            contentControl!.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }
}