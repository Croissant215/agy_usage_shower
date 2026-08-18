using System;
using System.Threading;
using System.Windows;
using Microsoft.Win32;

namespace AgyUsageShower
{
    public partial class App : System.Windows.Application
    {
        private static Mutex? _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "AgyUsageShower_SingleInstanceMutex";
            
            _mutex = new Mutex(true, appName, out bool createdNew);

            if (!createdNew)
            {
                // 이미 실행 중인 경우
                System.Windows.MessageBox.Show("안티그래비티 사용량 표시기가 이미 실행 중입니다.", "Agy Usage Shower", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                Current.Shutdown();
                return;
            }

            // 시작 프로그램 등록
            RegisterInStartup();

            base.OnStartup(e);
        }

        private void RegisterInStartup()
        {
            try
            {
                using RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (registryKey != null)
                {
                    string? processPath = Environment.ProcessPath;
                    if (!string.IsNullOrEmpty(processPath))
                    {
                        // 따옴표로 감싸주어 경로에 공백이 있어도 안전하게 실행되도록 함
                        registryKey.SetValue("AgyUsageShower", $"\"{processPath}\"");
                    }
                }
            }
            catch
            {
                // 권한 문제 등으로 실패 시 조용히 무시 (선택사항)
            }
        }
    }
}
