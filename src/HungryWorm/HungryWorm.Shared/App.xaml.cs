using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Microsoft.UI.Xaml.Media;
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.Hosting;
using Uno.Extensions.Hosting;
#if DEBUG
using Microsoft.Extensions.Logging;
#endif

namespace HungryWorm
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        #region Fields

        private readonly SystemNavigationManager _systemNavigationManager;
        private readonly List<Type> _goBackNotAllowedToPages;
        private readonly List<(Type IfGoingBackTo, Type RouteTo)> _goBackPageRoutes;

        private static Window _window;

        #endregion

        #region Properties

        public IHost Host { get; }

        #endregion

        #region Ctor

        public App()
        {
            Host = UnoHost
                   .CreateDefaultBuilder()
                   .ConfigureServices(serviceCollection =>
                   {
                       serviceCollection.AddHttpService(lifeTime: 300, retryCount: 3, retryWait: 1);
                       serviceCollection.AddSingleton<IHttpRequestService, HttpRequestService>();
                       serviceCollection.AddSingleton<IBackendService, BackendService>();
                   })
                   .Build();

            InitializeLogging();
            InitializeComponent();

            Uno.UI.ApplicationHelper.RequestedCustomTheme = "Dark";

#if HAS_UNO || NETFX_CORE
            Suspending += OnSuspending;
#endif
            UnhandledException += App_UnhandledException;

            Uno.UI.FeatureConfiguration.Page.IsPoolingEnabled = true;

            _systemNavigationManager = SystemNavigationManager.GetForCurrentView();

            _goBackNotAllowedToPages = new List<Type>() { typeof(GamePage) };
            _goBackPageRoutes = new List<(Type IfGoingBackTo, Type RouteTo)>() { /*(IfGoingBackTo: typeof(GameOverPage), RouteTo: typeof(GamePage))*/ };

            LocalizationHelper.CurrentCulture = "en";
        }

        #endregion        

        #region Events

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
#if NET6_0_OR_GREATER && WINDOWS && !HAS_UNO
            _window = new Window();
            _window.Activate();
#else
            _window = Window.Current;
#endif
            var rootFrame = _window.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();

                rootFrame.Background = Current.Resources["FrameBackgroundColor"] as SolidColorBrush;

                rootFrame.NavigationFailed += OnNavigationFailed;
                rootFrame.IsNavigationStackEnabled = true;

                if (args.UWPLaunchActivatedEventArgs.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {

                }

                _window.Content = rootFrame;
            }

#if !(NET6_0_OR_GREATER && WINDOWS)

            if (args.UWPLaunchActivatedEventArgs.PrelaunchActivated == false)
#endif
            {
                if (rootFrame.Content == null)
                {
                    //rootFrame.Navigate(typeof(StartPage), args.Arguments);
                    rootFrame.Navigate(typeof(GamePage), args.Arguments);
                }

                _window.Activate();
            }

            _systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            _systemNavigationManager.BackRequested += OnBackRequested;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            var rootFrame = _window.Content as Frame;

            if (rootFrame.CanGoBack)
            {
                var backPage = rootFrame.BackStack.LastOrDefault();

                if (_goBackNotAllowedToPages.Contains(backPage.SourcePageType))
                    return;

                if (_goBackPageRoutes.Any(x => x.IfGoingBackTo == backPage.SourcePageType))
                {
                    var reroute = _goBackPageRoutes.FirstOrDefault(x => x.IfGoingBackTo == backPage.SourcePageType).RouteTo;

                    rootFrame.Navigate(reroute);
                    return;
                }

                rootFrame.GoBack();
            }
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.Message);
            e.Handled = true;
        }

        #endregion

        #region Methods

        #region Public

        public static void EnterFullScreen(bool value)
        {
            var view = ApplicationView.GetForCurrentView();

            if (view is not null)
            {
                if (value)
                {
                    view.TryEnterFullScreenMode();
                }
                else
                {
                    view.ExitFullScreenMode();
                }
            }
        }

        public static void NavigateToPage(Type pageType, object parameter = null)
        {
            var rootFrame = _window.Content as Frame;
            rootFrame.Navigate(pageType, parameter);
        }

        #endregion

        #region Private

        private static void InitializeLogging()
        {
#if DEBUG
            var factory = LoggerFactory.Create(builder =>
            {
#if __WASM__
                builder.AddProvider(new Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
                builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
#elif NETFX_CORE
                builder.AddDebug();
#else
                builder.AddConsole();
#endif
                // Exclude logs below this level
                builder.SetMinimumLevel(LogLevel.Information);

                // Default filters for Uno Platform namespaces
                builder.AddFilter("Uno", LogLevel.Warning);
                builder.AddFilter("Windows", LogLevel.Warning);
                builder.AddFilter("Microsoft", LogLevel.Warning);

                // Generic Xaml events
                //builder.AddFilter("Microsoft.UI.Xaml", LogLevel.Debug);
                //builder.AddFilter("Microsoft.UI.Xaml.VisualStateGroup", LogLevel.Debug);
                //builder.AddFilter("Microsoft.UI.Xaml.StateTriggerBase", LogLevel.Debug);
                //builder.AddFilter("Microsoft.UI.Xaml.UIElement", LogLevel.Debug);
                //builder.AddFilter("Microsoft.UI.Xaml.FrameworkElement", LogLevel.Trace);

                // Layouter specific messages
                // builder.AddFilter("Microsoft.UI.Xaml.Controls", LogLevel.Debug );
                // builder.AddFilter("Microsoft.UI.Xaml.Controls.Layouter", LogLevel.Debug );
                // builder.AddFilter("Microsoft.UI.Xaml.Controls.Panel", LogLevel.Debug );

                //builder.AddFilter("Windows.Storage", LogLevel.Debug);

                // Binding related messages
                //builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug);
                //builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.Debug);

                // Binder memory references tracking
                //builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug);

                // RemoteControl and HotReload related
                //builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

                // Debug JS interop
                //builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug);
            });

            Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
            Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif

#endif
        }

        #endregion

        #endregion
    }
}
