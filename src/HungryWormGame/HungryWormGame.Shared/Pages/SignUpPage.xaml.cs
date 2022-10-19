using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace HungryWormGame
{
    public sealed partial class SignUpPage : Page
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

        public SignUpPage()
        {
            this.InitializeComponent();
            _backendService = (Application.Current as App).Host.Services.GetRequiredService<IBackendService>();

            _windowHeight = Window.Current.Bounds.Height;
            _windowWidth = Window.Current.Bounds.Width;

            LoadGameElements();
            PopulateGameViews();

            this.Loaded += SignUpPage_Loaded;
            this.Unloaded += SignUpPage_Unloaded;
        }

        #endregion

        #region Events

        #region Page

        private void SignUpPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetLocalization();

            SizeChanged += GamePage_SizeChanged;
            StartAnimation();
        }

        private void SignUpPage_Unloaded(object sender, RoutedEventArgs e)
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

        private async void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            if (SignupButton.IsEnabled)
                await PerformSignup();
        }

        private void LoginToExistingAccountButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(LoginPage));
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(StartPage));
        }

        #endregion

        #region Input Fields

        private void UserFullNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableSignupButton();
        }

        private void UserEmailBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableSignupButton();
        }

        private void UserNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableSignupButton();
        }

        private void PasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableSignupButton();
        }

        private async void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && SignupButton.IsEnabled)
                await PerformSignup();
        }

        private void ConfirmPasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableSignupButton();
        }

        private void ConfirmCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            EnableSignupButton();
        }

        private void ConfirmCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            EnableSignupButton();
        }

        #endregion

        #endregion

        #region Methods

        #region Logic

        private async Task PerformSignup()
        {
            this.RunProgressBar();

            if (await Signup() && await Authenticate())
            {
                this.StopProgressBar();
                NavigateToPage(typeof(LoginPage));
            }
        }

        private async Task<bool> Signup()
        {
            (bool IsSuccess, string Message) = await _backendService.SignupUser(
                fullName: UserFullNameBox.Text.Trim(),
                userName: UserNameBox.Text.Trim(),
                email: UserEmailBox.Text.ToLower().Trim(),
                password: PasswordBox.Text.Trim(), 
                subscribedNewsletters: SubscribeNewsLettersCheckBox.IsChecked.Value);

            if (!IsSuccess)
            {
                var error = Message;
                this.ShowError(error);
                return false;
            }

            return true;
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

        private void EnableSignupButton()
        {
            SignupButton.IsEnabled =
                !UserFullNameBox.Text.IsNullOrBlank()
                && IsValidFullName()
                && IsStrongPassword()
                && DoPasswordsMatch()
                && !UserNameBox.Text.IsNullOrBlank()
                && !UserEmailBox.Text.IsNullOrBlank()
                && IsValidEmail()
                && ConfirmCheckBox.IsChecked == true;
        }

        private bool IsValidFullName()
        {
            (bool IsValid, string Message) = StringExtensions.IsValidFullName(UserFullNameBox.Text);
            if (!IsValid)
                this.SetProgressBarMessage(message: LocalizationHelper.GetLocalizedResource(Message), isError: true);
            else
                ProgressBarMessageBlock.Visibility = Visibility.Collapsed;

            return IsValid;
        }

        private bool IsStrongPassword()
        {
            (bool IsStrong, string Message) = StringExtensions.IsStrongPassword(PasswordBox.Text);
            this.SetProgressBarMessage(message: LocalizationHelper.GetLocalizedResource(Message), isError: !IsStrong);

            return IsStrong;
        }

        private bool DoPasswordsMatch()
        {
            if (PasswordBox.Text.IsNullOrBlank() || ConfirmPasswordBox.Text.IsNullOrBlank())
                return false;

            if (PasswordBox.Text != ConfirmPasswordBox.Text)
            {
                this.SetProgressBarMessage(message: LocalizationHelper.GetLocalizedResource("PASSWORDS_DIDNT_MATCH"), isError: true);
                return false;
            }
            else
            {
                this.SetProgressBarMessage(message: LocalizationHelper.GetLocalizedResource("PASSWORDS_MATCHED"), isError: false);
            }

            return true;
        }

        private bool IsValidEmail()
        {
            return StringExtensions.IsValidEmail(UserEmailBox.Text);
        }

        #endregion

        #region Page

        private void SetViewSize()
        {
            _scale = ScalingHelper.GetGameObjectScale(_windowWidth);

            UnderView.Width = _windowWidth;
            UnderView.Height = _windowHeight;

            OverView.Width = _windowWidth;
            OverView.Height = _windowHeight;
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
