using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace SnakeGame
{
    public static class AssetHelper
    {
        #region Fields
        
        private static string _baseUrl;

        private static bool _assetsPreloaded;

        #endregion

        #region Methods

        public static string GetBaseUrl()
        {
            if (_baseUrl.IsNullOrBlank())
            {
                var indexUrl = Uno.Foundation.WebAssemblyRuntime.InvokeJS("window.location.href;");
                var appPackageId = Environment.GetEnvironmentVariable("UNO_BOOTSTRAP_APP_BASE");
                _baseUrl = $"{indexUrl}{appPackageId}";

#if DEBUG
                Console.WriteLine(_baseUrl);
#endif 
            }
            return _baseUrl;
        }

        public static async void PreloadAssets(ProgressBar progressBar, Action completed = null)
        {
            if (!_assetsPreloaded)
            {
                progressBar.IsIndeterminate = false;
                progressBar.ShowPaused = false;
                progressBar.Value = 0;
                progressBar.Minimum = 0;
                progressBar.Maximum = Constants.ELEMENT_TEMPLATES.Length /*+ Constants.SOUND_TEMPLATES.Length*/;

                foreach (var uri in Constants.ELEMENT_TEMPLATES.Select(x => x.Value).ToArray())
                {
                    await GetFileAsync(uri, progressBar);
                }

                //foreach (var uri in Constants.SOUND_TEMPLATES.Select(x => x.Value).ToArray())
                //{
                //    await GetFileAsync(new Uri($"ms-appx:///{uri}"), progressBar);
                //}

                _assetsPreloaded = true;

                completed?.Invoke();
            }
            else
            {
                completed?.Invoke();
            }
        }

        private static async Task GetFileAsync(Uri uri, ProgressBar progressBar)
        {
            await StorageFile.GetFileFromApplicationUriAsync(uri);
            progressBar.Value++;
        }

        #endregion
    }
}
