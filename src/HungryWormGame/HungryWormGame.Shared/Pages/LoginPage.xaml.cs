using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace HungryWormGame
{
    public sealed partial class LoginPage : Page
    {
        #region Fields

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly Random _random = new();

        private double _windowHeight, _windowWidth;
        private double _scale;

        private int _gameSpeed = 8;

        private int _markNum;

        private Uri[] _collectibles;

        private readonly IBackendService _backendService;

        #endregion

        #region Ctor

        public LoginPage()
        {
            this.InitializeComponent();
            _backendService = (Application.Current as App).Host.Services.GetRequiredService<IBackendService>();

            _windowHeight = Window.Current.Bounds.Height;
            _windowWidth = Window.Current.Bounds.Width;

            LoadGameElements();
            PopulateGameViews();

            this.Loaded += LoginPage_Loaded;
            this.Unloaded += LoginPage_Unloaded;
        }

        #endregion

        #region Events

        #region Page

        private void LoginPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetLocalization();

            SizeChanged += GamePage_SizeChanged;
            StartAnimation();

            // if user was already logged in or came here after sign up
            if (PlayerCredentialsHelper.GetCachedPlayerCredentials() is PlayerCredentials authCredentials
                && !authCredentials.UserName.IsNullOrBlank()
                && !authCredentials.Password.IsNullOrBlank())
            {
                UserNameBox.Text = authCredentials.UserName;
                PasswordBox.Text = authCredentials.Password;
            }
            else
            {
                UserNameBox.Text = null;
                PasswordBox.Text = null;
            }
        }

        private void LoginPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= GamePage_SizeChanged;
            StopAnimation();
        }

        private void GamePage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            _windowWidth = args.NewSize.Width;
            _windowHeight = args.NewSize.Height;

            SetViewSize();

#if DEBUG
            Console.WriteLine($"WINDOWS SIZE: {_windowWidth}x{_windowHeight}");
#endif
        }

        #endregion

        #region Buttons

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(StartPage));
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {            
            NavigateToPage(typeof(SignUpPage));
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginButton.IsEnabled)
                await PerformLogin();
        }

        #endregion

        #region Input Fields

        private void UserNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableLoginButton();
        }

        private void PasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableLoginButton();
        }

        private async void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && LoginButton.IsEnabled)
                await PerformLogin();
        }

        #endregion

        #endregion

        #region Methods      

        #region Logic

        private async Task PerformLogin()
        {
            this.RunProgressBar();

            if (await Authenticate() && await GetGameProfile() && await GenerateSession())
            {
                if (PlayerScoreHelper.GameScoreSubmissionPending)
                {
                    if (await SubmitScore())
                        PlayerScoreHelper.GameScoreSubmissionPending = false;
                }

                this.StopProgressBar();
                //TODO: to leaderboard page
                //NavigateToPage(typeof(LeaderboardPage));
            }
        }

        private async Task<bool> Authenticate()
        {
            (bool IsSuccess, string Message) = await _backendService.AuthenticateUser(
                userNameOrEmail: UserNameBox.Text.Trim(),
                password: PasswordBox.Text.Trim());

            if (!IsSuccess)
            {
                var error = Message;
                this.ShowError(error);
                return false;
            }

            return true;
        }

        private async Task<bool> GetGameProfile()
        {
            (bool IsSuccess, string Message, _) = await _backendService.GetUserGameProfile();

            if (!IsSuccess)
            {
                var error = Message;
                this.ShowError(error);
                return false;
            }

            return true;
        }

        private async Task<bool> GenerateSession()
        {
            (bool IsSuccess, string Message) = await _backendService.GenerateUserSession();

            if (!IsSuccess)
            {
                var error = Message;
                this.ShowError(error);
                return false;
            }

            return true;
        }

        private async Task<bool> SubmitScore()
        {
            (bool IsSuccess, string Message) = await _backendService.SubmitUserGameScore(PlayerScoreHelper.PlayerScore.Score);

            if (!IsSuccess)
            {
                var error = Message;
                this.ShowError(error);
                return false;
            }

            return true;
        }

        private void EnableLoginButton()
        {
            LoginButton.IsEnabled = !UserNameBox.Text.IsNullOrBlank() && !PasswordBox.Text.IsNullOrBlank();
        }

        #endregion

        #region Page

        private void SetViewSize()
        {
            _scale = ScalingHelper.GetGameObjectScale(_windowWidth);

            UnderView.Width = _windowWidth;
            UnderView.Height = _windowHeight;
        }

        private void NavigateToPage(Type pageType)
        {
            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            App.NavigateToPage(pageType);
        }

        #endregion

        #region Animation

        #region Game

        private void PopulateGameViews()
        {
#if DEBUG
            Console.WriteLine("INITIALIZING GAME");
#endif
            SetViewSize();
            PopulateUnderView();
        }

        private void LoadGameElements()
        {
            _collectibles = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.COLLECTIBLE).Select(x => x.Value).ToArray();
        }

        private void PopulateUnderView()
        {
            // add some dirt underneath
            for (int i = 0; i < 15; i++)
            {
                SpawnDirt();
            }

            // add some clouds underneath
            for (int i = 0; i < 10; i++)
            {
                SpawnCollectible();
            }
        }

        private void StartAnimation()
        {
#if DEBUG
            Console.WriteLine("GAME STARTED");
#endif      
            RecycleGameObjects();
            RunGame();
        }

        private void RecycleGameObjects()
        {
            foreach (GameObject x in UnderView.Children.OfType<GameObject>())
            {
                switch ((ElementType)x.Tag)
                {
                    case ElementType.COLLECTIBLE:
                        {
                            RecyleCollectible(x);
                        }
                        break;
                    case ElementType.DIRT:
                        {
                            RecyleDirt(x);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private async void RunGame()
        {
            _gameViewTimer = new PeriodicTimer(_frameTime);

            while (await _gameViewTimer.WaitForNextTickAsync())
            {
                GameViewLoop();
            }
        }

        private void GameViewLoop()
        {
            UpdateGameObjects();
        }

        private void UpdateGameObjects()
        {
            foreach (GameObject x in UnderView.Children.OfType<GameObject>())
            {
                switch ((ElementType)x.Tag)
                {
                    case ElementType.DIRT:
                        {
                            UpdateDirt(x);
                        }
                        break;
                    case ElementType.COLLECTIBLE:
                        {
                            UpdateCollectible(x);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void StopAnimation()
        {
            _gameViewTimer?.Dispose();
        }

        #endregion

        #region Dirt

        private void SpawnDirt()
        {
            var dirt = new Dirt((double)_random.Next(5, 100) * _scale);
            UnderView.Children.Add(dirt);
        }

        private void UpdateDirt(GameObject dirt)
        {
            dirt.SetTop(dirt.GetTop() + _gameSpeed);

            if (dirt.GetTop() > UnderView.Height)
            {
                RecyleDirt(dirt);
            }
        }

        private void RecyleDirt(GameObject dirt)
        {
            RandomizeDirtPosition(dirt);
        }

        private void RandomizeDirtPosition(GameObject dirt)
        {
            dirt.SetPosition(
                left: _random.Next(0, (int)UnderView.Width) - (100 * _scale),
                top: _random.Next(100 * (int)_scale, (int)UnderView.Height) * -1);
        }

        #endregion

        #region Collectible

        private void SpawnCollectible()
        {
            Collectible collectible = new(Constants.COLLECTIBLE_SIZE * _scale);
            RandomizeCollectiblePosition(collectible);

            UnderView.Children.Add(collectible);
        }

        private void UpdateCollectible(GameObject Collectible)
        {
            Collectible.SetTop(Collectible.GetTop() + _gameSpeed);

            if (Collectible.GetTop() > UnderView.Height)
            {
                RecyleCollectible(Collectible);
            }
        }

        private void RecyleCollectible(GameObject collectible)
        {
            _markNum = _random.Next(0, _collectibles.Length);
            collectible.SetContent(_collectibles[_markNum]);
            RandomizeCollectiblePosition(collectible);
        }

        private void RandomizeCollectiblePosition(GameObject Collectible)
        {
            Collectible.SetPosition(
                left: _random.Next(0, (int)UnderView.Width) - (100 * _scale),
                top: _random.Next(100 * (int)_scale, (int)UnderView.Height) * -1);
        }

        #endregion

        #region Sound

        private void StartGameSounds()
        {
            SoundHelper.RandomizeSound(SoundType.INTRO);
            SoundHelper.PlaySound(SoundType.INTRO);
        }

        #endregion        

        #endregion

        #endregion
    }
}
