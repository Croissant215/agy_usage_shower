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
                        Arguments = "/c npx -y antigravity-usage usage --json",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    psi.EnvironmentVariables["NO_UPDATE_NOTIFIER"] = "1";
                    psi.EnvironmentVariables["npm_config_update_notifier"] = "false";

                    using var process = System.Diagnostics.Process.Start(psi);
                    if (process != null)
                    {
                        var cts = new System.Threading.CancellationTokenSource(30000); // 30 seconds timeout
                        var readOutputTask = process.StandardOutput.ReadToEndAsync();
                        var readErrorTask = process.StandardError.ReadToEndAsync(); // Drain stderr to prevent pipe buffer deadlock
                        
                        try 
                        {
                            await process.WaitForExitAsync(cts.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            if (!process.HasExited)
                            {
                                process.Kill(true); // Kill cmd.exe and its children (npx, node.js)
                            }
                            throw;
                        }

                        string respString = await readOutputTask;
                        string errString = await readErrorTask; // Drained

                        if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(respString))
                        {
                            // Strip any non-JSON noise (npm warnings, etc.) before parsing
                            int firstBrace = respString.IndexOf('{');
                            int lastBrace = respString.LastIndexOf('}');
                            if (firstBrace >= 0 && lastBrace > firstBrace)
                            {
                                respString = respString.Substring(firstBrace, lastBrace - firstBrace + 1);
                            }

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
                            string claudeResetIn = "Quota available";

                            if (respRoot.TryGetProperty("models", out var modelsArr) && modelsArr.ValueKind == JsonValueKind.Array)
                            {
                                JsonElement? bestGeminiModel = null;
                                int bestGeminiScore = -1;

                                JsonElement? bestClaudeModel = null;
                                int bestClaudeScore = -1;

                                foreach (var m in modelsArr.EnumerateArray())
                                {
                                    string label = m.TryGetProperty("label", out var l) ? l.GetString() ?? "" : "";
                                    string modelId = m.TryGetProperty("modelId", out var mid) ? mid.GetString() ?? "" : "";
                                    bool isAutocomplete = m.TryGetProperty("isAutocompleteOnly", out var ac) && ac.GetBoolean();

                                    // Match Gemini Models: Prioritize main interactive models over autocomplete-only
                                    if (label.Contains("Gemini", StringComparison.OrdinalIgnoreCase) || modelId.Contains("gemini", StringComparison.OrdinalIgnoreCase))
                                    {
                                        int score = 1;
                                        if (!isAutocomplete) score += 10;
                                        if (label.Contains("3.8", StringComparison.OrdinalIgnoreCase) || modelId.Contains("3.8", StringComparison.OrdinalIgnoreCase)) score += 50;
                                        else if (label.Contains("3.1", StringComparison.OrdinalIgnoreCase) || modelId.Contains("3.1", StringComparison.OrdinalIgnoreCase)) score += 40;
                                        else if (label.Contains("3.6", StringComparison.OrdinalIgnoreCase) || modelId.Contains("3.6", StringComparison.OrdinalIgnoreCase)) score += 30;
                                        else if (label.Contains("3.5", StringComparison.OrdinalIgnoreCase) || modelId.Contains("3.5", StringComparison.OrdinalIgnoreCase)) score += 20;
                                        else if (label.Contains("3", StringComparison.OrdinalIgnoreCase)) score += 15;

                                        if (label.Contains("High", StringComparison.OrdinalIgnoreCase) || modelId.Contains("high", StringComparison.OrdinalIgnoreCase)) score += 5;

                                        if (score > bestGeminiScore)
                                        {
                                            bestGeminiScore = score;
                                            bestGeminiModel = m;
                                        }
                                    }
                                    // Match Claude Models: Opus / Sonnet
                                    else if (label.Contains("Claude", StringComparison.OrdinalIgnoreCase) || modelId.Contains("claude", StringComparison.OrdinalIgnoreCase))
                                    {
                                        int score = 1;
                                        if (label.Contains("Sonnet", StringComparison.OrdinalIgnoreCase) || modelId.Contains("sonnet", StringComparison.OrdinalIgnoreCase)) score += 30;
                                        else if (label.Contains("Opus", StringComparison.OrdinalIgnoreCase) || modelId.Contains("opus", StringComparison.OrdinalIgnoreCase)) score += 20;

                                        if (score > bestClaudeScore)
                                        {
                                            bestClaudeScore = score;
                                            bestClaudeModel = m;
                                        }
                                    }
                                }

                                if (bestGeminiModel.HasValue)
                                {
                                    var gm = bestGeminiModel.Value;
                                    if (gm.TryGetProperty("remainingPercentage", out var rp))
                                    {
                                        geminiRem = Math.Round(rp.GetDouble() * 100.0, 2);
                                    }
                                    if (gm.TryGetProperty("timeUntilResetMs", out var ms))
                                    {
                                        long msVal = ms.GetInt64();
                                        TimeSpan ts = TimeSpan.FromMilliseconds(msVal);
                                        resetIn = ts.Hours > 0 ? $"{ts.Hours}h {ts.Minutes}m" : $"{ts.Minutes}m";
                                    }
                                }

                                if (bestClaudeModel.HasValue)
                                {
                                    var cm = bestClaudeModel.Value;
                                    if (cm.TryGetProperty("remainingPercentage", out var rp))
                                    {
                                        claudeRem = Math.Round(rp.GetDouble() * 100.0, 2);
                                    }
                                    if (cm.TryGetProperty("timeUntilResetMs", out var ms))
                                    {
                                        long msVal = ms.GetInt64();
                                        TimeSpan ts = TimeSpan.FromMilliseconds(msVal);
                                        claudeResetIn = ts.Hours > 0 ? $"{ts.Hours}h {ts.Minutes}m" : $"{ts.Minutes}m";
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
                                ClaudeResetTime = claudeResetIn,
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
                    Arguments = "/c npx -y antigravity-usage usage",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                psi.EnvironmentVariables["NO_UPDATE_NOTIFIER"] = "1";
                psi.EnvironmentVariables["npm_config_update_notifier"] = "false";

                using var process = System.Diagnostics.Process.Start(psi);
                if (process != null)
                {
                    var cts = new System.Threading.CancellationTokenSource(20000);
                    var outTask = process.StandardOutput.ReadToEndAsync();
                    var errTask = process.StandardError.ReadToEndAsync();
                    try
                    {
                        await process.WaitForExitAsync(cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        if (!process.HasExited)
                        {
                            process.Kill(true);
                        }
                        throw;
                    }
                    await outTask;
                    await errTask;
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
