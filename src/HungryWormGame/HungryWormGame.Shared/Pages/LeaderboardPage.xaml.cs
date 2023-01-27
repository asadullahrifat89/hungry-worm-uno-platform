using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Uno.Extensions;
using System.Threading;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace HungryWormGame
{
    public sealed partial class LeaderboardPage : Page
    {
        #region Fields

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly Random _random = new();

        private double _windowHeight, _windowWidth;
        private double _scale;

        private readonly int _gameSpeed = 8;

        private int _markNum;

        private Uri[] _collectibles;

        private readonly IBackendService _backendService;

        #endregion

        #region Properties

        public ObservableCollection<GameProfile> GameProfilesCollection { get; set; } = new ObservableCollection<GameProfile>();

        public ObservableCollection<GameScore> GameScoresCollection { get; set; } = new ObservableCollection<GameScore>();

        #endregion

        #region Ctor

        public LeaderboardPage()
        {
            this.InitializeComponent();
            _backendService = (Application.Current as App).Host.Services.GetRequiredService<IBackendService>();

            GameProfilesList.ItemsSource = GameProfilesCollection;
            GameScoresList.ItemsSource = GameScoresCollection;

            _windowHeight = Window.Current.Bounds.Height;
            _windowWidth = Window.Current.Bounds.Width;

            LoadGameElements();
            PopulateGameViews();

            this.Loaded += LeaderboardPage_Loaded;
            this.Unloaded += LeaderboardPage_Unloaded;
        }

        #endregion

        #region Events

        #region Page

        private async void LeaderboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetLocalization();

            this.RunProgressBar();

            if (await GetGameProfile())
                ShowUserName();

            SeasonToggle.IsChecked = true;

            this.StopProgressBar();

            SizeChanged += GamePlayPage_SizeChanged;
            StartAnimation();
        }

        private void LeaderboardPage_Unloaded(object sender, RoutedEventArgs e)
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

        private async void PlayAgainButton_Click(object sender, RoutedEventArgs e)
        {
            if (GameProfileHelper.HasUserLoggedIn() ? await GenerateSession() : true)
                NavigateToPage(typeof(GamePlayPage));
        }

        private async void SeasonToggle_Click(object sender, RoutedEventArgs e)
        {
            SoundHelper.PlaySound(SoundType.MENU_SELECT);

            this.RunProgressBar();

            UncheckScoreboardChoiceToggles(sender);

            await GetGameSeason();

            this.StopProgressBar();
        }

        private async void AllTimeScoreboardToggle_Click(object sender, RoutedEventArgs e)
        {
            SoundHelper.PlaySound(SoundType.MENU_SELECT);

            this.RunProgressBar();

            UncheckScoreboardChoiceToggles(sender);

            await GetGameProfiles();

            this.StopProgressBar();
        }

        private async void DailyScoreboardToggle_Click(object sender, RoutedEventArgs e)
        {
            SoundHelper.PlaySound(SoundType.MENU_SELECT);

            this.RunProgressBar();

            UncheckScoreboardChoiceToggles(sender);

            await GetGameScores();

            this.StopProgressBar();
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(StartPage));
        }

        #endregion

        #endregion

        #region Methods

        #region Logic

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

        private async Task<bool> GetGameProfile()
        {
            (bool IsSuccess, string Message, _) = await _backendService.GetUserGameProfile();

            if (!IsSuccess)
            {
                var error = Message;
                this.ShowError(error);
                return false;
            }

            SetGameScores(
                personalBestScore: GameProfileHelper.GameProfile.PersonalBestScore,
                lastGameScore: GameProfileHelper.GameProfile.LastGameScore);

            return true;
        }

        private async Task<bool> GetGameProfiles()
        {
            GameProfilesCollection.Clear();
            SetListViewMessage(LocalizationHelper.GetLocalizedResource("LOADING_DATA"));

            (bool IsSuccess, string Message, GameProfile[] GameProfiles) = await _backendService.GetUserGameProfiles(pageIndex: 0, pageSize: 10);

            if (!IsSuccess)
            {
                SetListViewMessage();
                var error = Message;
                this.ShowError(error);
                return false;
            }

            if (GameProfiles is not null && GameProfiles.Length > 0)
            {
                SetListViewMessage();
                GameProfilesCollection.AddRange(GameProfiles);
                SetLeaderboardPlacements(GameProfilesCollection);
                IndicateCurrentPlayer(GameProfilesCollection.Cast<LeaderboardPlacement>().ToObservableCollection());
            }
            else
            {
                SetListViewMessage(LocalizationHelper.GetLocalizedResource("NO_DATA_AVAILABLE"));
            }

            return true;
        }

        private async Task<bool> GetGameScores()
        {
            GameScoresCollection.Clear();
            SetListViewMessage(LocalizationHelper.GetLocalizedResource("LOADING_DATA"));

            (bool IsSuccess, string Message, GameScore[] GameScores) = await _backendService.GetUserGameScores(pageIndex: 0, pageSize: 10);

            if (!IsSuccess)
            {
                SetListViewMessage();
                var error = Message;
                this.ShowError(error);
                return false;
            }

            if (GameScores is not null && GameScores.Length > 0)
            {
                SetListViewMessage();
                GameScoresCollection.AddRange(GameScores);
                SetLeaderboardPlacements(GameScoresCollection);
                IndicateCurrentPlayer(GameScoresCollection.Cast<LeaderboardPlacement>().ToObservableCollection());
            }
            else
            {
                SetListViewMessage(LocalizationHelper.GetLocalizedResource("NO_DATA_AVAILABLE"));
            }

            return true;
        }

        private async Task<bool> GetGameSeason()
        {
            SetListViewMessage(LocalizationHelper.GetLocalizedResource("LOADING_DATA"));

            (bool IsSuccess, string Message, Season Season) = await _backendService.GetGameSeason();

            if (!IsSuccess)
            {
                SetListViewMessage();
                var error = Message;
                this.ShowError(error);
                return false;
            }

            if (Season is not null && Season.PrizeDescriptions is not null && Season.PrizeDescriptions.Length > 0)
            {
                SetListViewMessage();
                SeasonPrizeDescriptionText.Text = Season.PrizeDescriptions.FirstOrDefault(x => x.Culture == LocalizationHelper.CurrentCulture).Value;
                await GetGamePrize();
            }
            else
            {
                SeasonPrizeContainer.Visibility = Visibility.Collapsed;
                SetListViewMessage(LocalizationHelper.GetLocalizedResource("NO_DATA_AVAILABLE"));
            }

            return true;
        }

        private async Task<bool> GetGamePrize()
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
                SetListViewMessage();
                WinningCriteriaDescriptionText.Text = GamePrize.WinningCriteria.CriteriaDescriptions.FirstOrDefault(x => x.Culture == LocalizationHelper.CurrentCulture).Value;
                GamePrizeDescriptionText.Text = GamePrize.PrizeDescriptions.FirstOrDefault(x => x.Culture == LocalizationHelper.CurrentCulture).Value;
            }
            else
            {
                DailyPrizeContainer.Visibility = Visibility.Collapsed;
            }

            return true;
        }

        private void SetLeaderboardPlacements(dynamic leaderboardPlacements)
        {
            if (leaderboardPlacements.Count > 0)
            {
                // king of the ring
                if (leaderboardPlacements[0] is LeaderboardPlacement firstPlacement)
                {
                    firstPlacement.MedalEmoji = "🥇";
                    firstPlacement.Emoji = "🏆";
                }

                if (leaderboardPlacements.Count > 1)
                {
                    if (leaderboardPlacements[1] is LeaderboardPlacement secondPlacement)
                    {
                        secondPlacement.MedalEmoji = "🥈";
                    }
                }

                if (leaderboardPlacements.Count > 2)
                {
                    if (leaderboardPlacements[2] is LeaderboardPlacement thirdPlacement)
                    {
                        thirdPlacement.MedalEmoji = "🥉";
                    }
                }
            }
        }

        private void IndicateCurrentPlayer(ObservableCollection<LeaderboardPlacement> leaderboardPlacements)
        {
            if (leaderboardPlacements is not null)
            {
                if (leaderboardPlacements.FirstOrDefault(x => x.User.UserId == GameProfileHelper.GameProfile.User.UserId) is LeaderboardPlacement placement)
                {
                    placement.Emoji = "👨‍🚀";
                }
            }
        }

        private void SetListViewMessage(string message = null)
        {
            ListViewMessage.Text = message;
            ListViewMessage.Visibility = message.IsNullOrBlank() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SetGameScores(double personalBestScore, double lastGameScore)
        {
            PersonalBestScoreText.Text = LocalizationHelper.GetLocalizedResource("PersonalBestScoreText") + ": " + personalBestScore;
            LastGameScoreText.Text = LocalizationHelper.GetLocalizedResource("LastGameScoreText") + ": " + lastGameScore;
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

            OverView.Width = _windowWidth;
            OverView.Height = _windowHeight;
        }

        private void NavigateToPage(Type pageType)
        {
            if (pageType == typeof(GamePlayPage))
                SoundHelper.StopSound(SoundType.INTRO);

            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            App.NavigateToPage(pageType);
        }

        private void UncheckScoreboardChoiceToggles(object sender)
        {
            foreach (var toggleButton in ScoreboardChoice.Children.OfType<ToggleButton>().Where(x => x.Name != ((ToggleButton)sender).Name))
            {
                toggleButton.IsChecked = false;
            }
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

        #endregion

        #endregion
    }
}
