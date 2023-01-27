using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;

namespace HungryWormGame
{
    public sealed partial class GameOverPage : Page
    {
        #region Fields

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly Random _random = new();

        private double _windowHeight, _windowWidth;
        private double _scale;

        private readonly int _gameSpeed = 5;

        private int _markNum;

        private Uri[] _collectibles;

        private readonly IBackendService _backendService;

        #endregion

        #region Ctor

        public GameOverPage()
        {
            this.InitializeComponent();
            _backendService = (Application.Current as App).Host.Services.GetRequiredService<IBackendService>();

            _windowHeight = Window.Current.Bounds.Height;
            _windowWidth = Window.Current.Bounds.Width;

            LoadGameElements();
            PopulateGameViews();

            this.Loaded += GameOverPage_Loaded;
            this.Unloaded += GameOverPage_Unloaded;
        }

        #endregion

        #region Events

        #region Page

        private async void GameOverPage_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += GamePlayPage_SizeChanged;
            StartAnimation();

            this.SetLocalization();

            SetGameResults();
            ShowUserName();

            // if user has not logged in
            if (!GameProfileHelper.HasUserLoggedIn())
            {
                SetLoginContext();
                await ShowGamePrize();
            }
            else
            {
                this.RunProgressBar();

                if (await SubmitScore())
                {
                    SetLeaderboardContext(); // if score submission was successful make leaderboard button visible
                }
                else
                {
                    SetLoginContext();
                }

                this.StopProgressBar();
            }

            await GetCompanyBrand();
        }

        private void GameOverPage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= GamePlayPage_SizeChanged;
            StopAnimation();
        }

        private void GamePlayPage_SizeChanged(object sender, SizeChangedEventArgs args)
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

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(LoginPage));
        }

        private async void PlayAgainButton_Click(object sender, RoutedEventArgs e)
        {
            if (GameProfileHelper.HasUserLoggedIn() ? await GenerateSession() : true)
                NavigateToPage(typeof(GamePlayPage));
        }

        private void LeaderboardButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavigateToPage(typeof(LeaderboardPage));
        }

        #endregion

        #endregion

        #region Methods

        #region Logic

        private async Task<bool> GetCompanyBrand()
        {
            // if company is not already fetched, fetch it
            if (CompanyHelper.Company is null)
            {
                (bool IsSuccess, string Message, Company Company) = await _backendService.GetCompanyBrand();

                if (!IsSuccess)
                {
                    var error = Message;
                    this.ShowError(error);
                    return false;
                }

                if (Company is not null && !Company.WebSiteUrl.IsNullOrBlank())
                {
                    CompanyHelper.Company = Company;
                }
            }

            if (CompanyHelper.Company is not null)
                BrandButton.NavigateUri = new Uri(CompanyHelper.Company.WebSiteUrl);

            return true;
        }

        private async Task<bool> ShowGamePrize()
        {
            (bool IsSuccess, string Message, GamePrizeOfTheDay GamePrize) = await _backendService.GetGameDailyPrize();

            if (!IsSuccess)
            {
                var error = Message;
                this.ShowError(error);
                return false;
            }

            if (GamePrize is not null
                && GamePrize.WinningCriteria is not null
                && GamePrize.WinningCriteria.CriteriaDescriptions is not null
                && GamePrize.PrizeDescriptions is not null
                && GamePrize.WinningCriteria.CriteriaDescriptions.Length > 0
                && GamePrize.PrizeDescriptions.Length > 0)
            {
                ShowGamePlayResult(new GamePlayResult()
                {
                    GameId = GamePrize.GameId,
                    PrizeDescriptions = GamePrize.PrizeDescriptions,
                    PrizeName = GamePrize.Name,
                    WinningDescriptions = GamePrize.WinningCriteria.CriteriaDescriptions,
                });
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
            (bool IsSuccess, string Message, GamePlayResult GamePlayResult) = await _backendService.SubmitUserGameScore(PlayerScoreHelper.PlayerScore.Score);

            if (!IsSuccess)
            {
                var error = Message;
                this.ShowError(error);
                return false;
            }

            ShowGamePlayResult(GamePlayResult);

            return true;
        }

        private void ShowGamePlayResult(GamePlayResult GamePlayResult)
        {
            if (GamePlayResult is not null && !GamePlayResult.PrizeName.IsNullOrBlank())
                PopUpHelper.ShowGamePlayResultPopUp(GamePlayResult);
        }

        private void SetGameResults()
        {
            ScoreNumberText.Text = PlayerScoreHelper.PlayerScore.Score.ToString("#");
            CollectiblesCollectedText.Text = $"{LocalizationHelper.GetLocalizedResource("CollectiblesCollectedText")} x " + PlayerScoreHelper.PlayerScore.CollectiblesCollected;
        }

        private void SetLeaderboardContext()
        {
            SignupPromptPanel.Visibility = Visibility.Collapsed;
            LeaderboardButton.Visibility = Visibility.Visible;
        }

        private void SetLoginContext()
        {
            // submit score on user login, or signup then login
            PlayerScoreHelper.GameScoreSubmissionPending = true;

            SignupPromptPanel.Visibility = Visibility.Visible;
            LeaderboardButton.Visibility = Visibility.Collapsed;
        }

        private void ShowUserName()
        {
            if (GameProfileHelper.HasUserLoggedIn())
            {
                UserName.Text = GameProfileHelper.GameProfile.User.UserName;
                UserPicture.Initials = GameProfileHelper.GameProfile.Initials;
                PlayerNameHolder.Visibility = Visibility.Visible;
            }
            else
            {
                PlayerNameHolder.Visibility = Visibility.Collapsed;
            }
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
            if (pageType == typeof(GamePlayPage))
                SoundHelper.StopSound(SoundType.INTRO);

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
                SpawnSpot();
            }

            // add some clouds underneath
            for (int i = 0; i < 10; i++)
            {
                SpawnCollectible();
            }
        }

        private void StartAnimation()
        {
            StartGameSounds();
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
                    case ElementType.SPOT:
                        {
                            RecyleSpot(x);
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
                    case ElementType.SPOT:
                        {
                            UpdateSpot(x);
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

        #region Spot

        private void SpawnSpot()
        {
            var dirt = new Spot((double)_random.Next(5, 100) * _scale);
            UnderView.Children.Add(dirt);
        }

        private void UpdateSpot(GameObject dirt)
        {
            dirt.SetTop(dirt.GetTop() + _gameSpeed);

            if (dirt.GetTop() > UnderView.Height)
                RecyleSpot(dirt);
        }

        private void RecyleSpot(GameObject dirt)
        {
            RandomizeSpotPosition(dirt);
        }

        private void RandomizeSpotPosition(GameObject dirt)
        {
            dirt.SetPosition(
                left: _random.Next(0, (int)UnderView.Width) - (100 * _scale),
                top: _random.Next(100 * (int)_scale, (int)UnderView.Height) * -1);
        }

        #endregion

        #region Collectible

        private void SpawnCollectible()
        {
            Collectible collectible = new(_scale);
            RandomizeCollectiblePosition(collectible);

            UnderView.Children.Add(collectible);
        }

        private void UpdateCollectible(GameObject Collectible)
        {
            Collectible.SetTop(Collectible.GetTop() + _gameSpeed);

            if (Collectible.GetTop() > UnderView.Height)
                RecyleCollectible(Collectible);
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
