using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using AgyUsageShower.Models;

namespace AgyUsageShower.Services
{
    public class AntigravityUsageService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        private readonly string _tokensPath;
        private UsageData _lastData = new UsageData();

        public event Action? OnRealtimeUsageChanged;

        public AntigravityUsageService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _tokensPath = Path.Combine(appData, "antigravity-usage", "tokens.json");
        }

        private string? GetTokenFilePath()
        {
            if (File.Exists(_tokensPath)) return _tokensPath;

            // Check for tokens.json in accounts subdirectories (npx antigravity-usage login behavior)
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string accountsDir = Path.Combine(appData, "antigravity-usage", "accounts");
            if (Directory.Exists(accountsDir))
            {
                var files = Directory.GetFiles(accountsDir, "tokens.json", SearchOption.AllDirectories);
                if (files.Length > 0) return files[0];
            }

            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string alt1 = Path.Combine(userProfile, ".gemini", "antigravity-usage", "tokens.json");
            if (File.Exists(alt1)) return alt1;

            string alt2 = Path.Combine(userProfile, ".gemini", "antigravity", "tokens.json");
            if (File.Exists(alt2)) return alt2;

            return null;
        }

        public bool CheckIsLoggedIn()
        {
            return GetTokenFilePath() != null;
        }

        public async Task<UsageData> FetchUsageAsync(bool forceRefresh = false, bool isRetry = false)
        {
            try
            {
                bool loggedIn = CheckIsLoggedIn();
                if (loggedIn)
                {
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c npx antigravity-usage usage --json",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using var process = System.Diagnostics.Process.Start(psi);
                    if (process != null)
                    {
                        var cts = new System.Threading.CancellationTokenSource(15000); // 15 seconds timeout
                        
                        // Read stdout first to prevent pipe buffer deadlock
                        var readOutputTask = process.StandardOutput.ReadToEndAsync();
                        
                        await process.WaitForExitAsync(cts.Token);
                        string respString = await readOutputTask;

                        if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(respString))
                        {
                            using JsonDocument respDoc = JsonDocument.Parse(respString);
                            JsonElement respRoot = respDoc.RootElement;

                            string email = "Connected Account";
                            if (respRoot.TryGetProperty("email", out var emailProp))
                            {
                                email = emailProp.GetString() ?? email;
                            }

                            double geminiRem = 100.0;
                            double geminiWeeklyRem = 100.0;
                            double claudeRem = 100.0;
                            string resetIn = "Quota available";
                            bool foundGemini5h = false;

                            if (respRoot.TryGetProperty("models", out var modelsArr) && modelsArr.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var m in modelsArr.EnumerateArray())
                                {
                                    string label = m.TryGetProperty("label", out var l) ? l.GetString() ?? "" : "";
                                    double remPct = m.TryGetProperty("remainingPercentage", out var rp) ? rp.GetDouble() * 100.0 : 100.0;

                                    if (label.Contains("Gemini", StringComparison.OrdinalIgnoreCase))
                                    {
                                        bool isWeekly = label.Contains("Weekly", StringComparison.OrdinalIgnoreCase) || label.Contains("7d", StringComparison.OrdinalIgnoreCase);
                                        bool is5h = label.Contains("5h", StringComparison.OrdinalIgnoreCase) || label.Contains("5-hour", StringComparison.OrdinalIgnoreCase) || label.Contains("Hourly", StringComparison.OrdinalIgnoreCase) || label.Contains("Medium", StringComparison.OrdinalIgnoreCase) || label.Contains("High", StringComparison.OrdinalIgnoreCase);

                                        if (isWeekly)
                                        {
                                            geminiWeeklyRem = Math.Round(remPct, 2);
                                        }
                                        else if (is5h || !foundGemini5h)
                                        {
                                            geminiRem = Math.Round(remPct, 2);
                                            foundGemini5h = true;
                                            if (m.TryGetProperty("timeUntilResetMs", out var ms))
                                            {
                                                long msVal = ms.GetInt64();
                                                TimeSpan ts = TimeSpan.FromMilliseconds(msVal);
                                                resetIn = ts.Hours > 0 ? $"{ts.Hours}h {ts.Minutes}m" : $"{ts.Minutes}m";
                                            }
                                        }
                                    }
                                    else if (label.Contains("Claude", StringComparison.OrdinalIgnoreCase))
                                    {
                                        claudeRem = Math.Round(remPct, 2);
                                    }
                                }
                            }

                            _lastData = new UsageData
                            {
                                AccountEmail = email,
                                GeminiWeeklyPercent = geminiWeeklyRem,
                                Gemini5hPercent = geminiRem,
                                GeminiResetTime = resetIn,
                                ClaudeWeeklyPercent = claudeRem,
                                Claude5hPercent = claudeRem,
                                ClaudeResetTime = "Quota available",
                                IsRealData = true,
                                IsOffline = false,
                                IsLoggedIn = true,
                                IsRefreshingToken = false
                            };
                            OnRealtimeUsageChanged?.Invoke();
                            return _lastData;
                        }
                        else if (!isRetry)
                        {
                            // If exit code is not 0 (e.g., auth error), try to refresh token
                            return await TryRefreshTokenAndRetryAsync(forceRefresh);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            bool loggedInFinal = CheckIsLoggedIn();
            _lastData = new UsageData
            {
                AccountEmail = loggedInFinal ? "Connected Account" : "Not Logged In",
                GeminiWeeklyPercent = 0.0,
                Gemini5hPercent = 0.0,
                GeminiResetTime = "-",
                ClaudeWeeklyPercent = 0.0,
                Claude5hPercent = 0.0,
                ClaudeResetTime = "-",
                IsRealData = false,
                IsOffline = !loggedInFinal,
                IsLoggedIn = loggedInFinal,
                IsRefreshingToken = false
            };

            OnRealtimeUsageChanged?.Invoke();
            return _lastData;
        }

        private async Task<UsageData> TryRefreshTokenAndRetryAsync(bool forceRefresh)
        {
            _lastData.IsRefreshingToken = true;
            OnRealtimeUsageChanged?.Invoke();

            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c npx antigravity-usage usage",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                };

                using var process = System.Diagnostics.Process.Start(psi);
                if (process != null)
                {
                    var cts = new System.Threading.CancellationTokenSource(10000);
                    await process.WaitForExitAsync(cts.Token);
                }
            }
            catch (Exception)
            {
            }

            return await FetchUsageAsync(forceRefresh, isRetry: true);
        }

        public static void TriggerGoogleLogin()
        {
            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c start cmd /k npx antigravity-usage login",
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception)
            {
            }
        }

        public static void TriggerLogout()
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                
                string tokensPath = Path.Combine(appData, "antigravity-usage", "tokens.json");
                if (File.Exists(tokensPath)) File.Delete(tokensPath);

                string accountsDir = Path.Combine(appData, "antigravity-usage", "accounts");
                if (Directory.Exists(accountsDir))
                {
                    var files = Directory.GetFiles(accountsDir, "tokens.json", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
