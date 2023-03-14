using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Downloader;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Chief.Core
{
    public static class Requests
    {

        public static async Task<List<Models.Core.CommitInfo>> GetReleaseInfo()
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(new Uri("https://git.cinogama.net/api/v4/projects/68/repository/tags"));
            response.EnsureSuccessStatusCode();
            var resp = await response.Content.ReadAsStringAsync();
            var data = JArray.Parse(resp);
            var commits =
                from item in data
                select (JObject)item;
            Models.Core.CommitInfo commitInfo = new();

            return commits.Select(item => new Models.Core.CommitInfo()
            {
                Version = (string)item["name"],
                ID = (string)item["target"],
                RelaeseMessage = (string)item["release"]["description"],
                ReleaseDateTime = DateTime.Parse((string)item["commit"]["created_at"]),
                Author = (string)item["commit"]["author_name"]
            })
                .ToList();
        }
    }

    public static class SystemInfo
    {
        public static string GetSystemThemeColor()
        {
            RegistryKey themeReg =
                Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\DWM");
            int themeColor = (int)themeReg.GetValue("ColorizationColor");

            return themeColor.ToString("X")[2..];
        }

        public static bool isLightTheme()
        {
            RegistryKey themeReg =
                Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");

            return (int)themeReg.GetValue("AppsUseLightTheme") == 1;
        }

        public static bool IsGitInstalled()
        {
            var cmd = new ProcessStartInfo("git", "version")
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

            return process.StandardOutput.ReadToEnd().Trim().StartsWith("git version");
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

            return process.StandardOutput.ReadToEnd().Trim().Replace("woodriver.exe", "");
        }
    }
}