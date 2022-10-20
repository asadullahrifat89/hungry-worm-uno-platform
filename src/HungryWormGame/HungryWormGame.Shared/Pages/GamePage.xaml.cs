using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Linq;
using System.Threading;
using Uno.Extensions;
using Windows.Foundation;
using Windows.System;

namespace HungryWormGame
{
    public sealed partial class GamePage : Page
    {
        #region Fields

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly Random _random = new();

        private double _gameSpeed = 6;
        private readonly double _gameSpeedDefault = 6;
        private readonly double _gameSpeedfactor = 0.05;

        private int _powerUpCount;
        private readonly int _powerUpSpawnLimit = 1;
        private int _powerUpSpawnCounter = 800;
        private int _powerModeDurationCounter;
        private readonly int _powerModeDuration = 800;

        private double _score;

        private bool _isGameOver;
        private bool _isPowerMode;

        private bool _isPointerActivated;
        private Point _pointerPosition;

        private double _windowHeight, _windowWidth;
        private double _scale;

        private Player _player;
        private Rect _playerHitBox;

        private int _foodSpawnCounter;
        private int _foodSpawnLimit;
        private int _foodCount;
        private int _foodCollected;

        private Uri[] _playerTemplates;
        private Uri[] _collectibleTemplates;

        private double _playerHealth;
        private int _playerHealthDepletionCounter;
        private double _playerHealthDepletionPoint;
        private double _playerHealthRejuvenationPoint;

        private readonly double _healthDepletePointDefault = 0.5;
        private readonly double _healthGainPointDefault = 10;

        private int _playerTrailCount;
        private int _playerTrailLength;
        private readonly int _playerTrailLengthLimit = 10;

        private int _playerYummyFaceCounter;

        #endregion

        #region Ctor

        public GamePage()
        {
            InitializeComponent();

            _isGameOver = true;
            ShowInGameTextMessage("TAP_ON_SCREEN_TO_BEGIN");

            _windowHeight = Window.Current.Bounds.Height;
            _windowWidth = Window.Current.Bounds.Width;

            LoadGameElements();
            PopulateGameViews();

            Loaded += GamePage_Loaded;
            Unloaded += GamePage_Unloaded;
        }

        #endregion

        #region Events

        #region Page

        private void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += GamePage_SizeChanged;
        }

        private void GamePage_Unloaded(object sender, RoutedEventArgs e)
        {
            SizeChanged -= GamePage_SizeChanged;
            StopGame();
        }

        private void GamePage_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            _windowWidth = args.NewSize.Width;
            _windowHeight = args.NewSize.Height;

            SetViewSize();

            Console.WriteLine($"WINDOWS SIZE: {_windowWidth}x{_windowHeight}");
        }

        #endregion

        #region Input

        private void InputView_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_isGameOver)
            {
                App.EnterFullScreen(true);

                InputView.Focus(FocusState.Programmatic);
                StartGame();
            }
            else
            {
                _isPointerActivated = true;

                PointerPoint point = e.GetCurrentPoint(GameView);
                _pointerPosition = point.Position;
            }
        }

        private void InputView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isPointerActivated)
            {
                PointerPoint point = e.GetCurrentPoint(GameView);
                _pointerPosition = point.Position;
            }
        }

        private void InputView_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isPointerActivated = false;
        }

        #endregion

        #region Button

        private void QuitGameButton_Checked(object sender, RoutedEventArgs e)
        {
            PauseGame();
        }

        private void QuitGameButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ResumeGame();
        }

        private void ConfirmQuitGameButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(typeof(StartPage));
        }

        #endregion

        #endregion

        #region Methods

        #region Animation

        #region Game

        private void PopulateGameViews()
        {
#if DEBUG
            Console.WriteLine("INITIALIZING GAME");
#endif
            SetViewSize();

            PopulateUnderView();
            PopulateGameView();
        }

        private void PopulateUnderView()
        {
            // add some dirt underneath
            for (int i = 0; i < 15; i++)
            {
                SpawnDirt();
            }
        }

        private void PopulateGameView()
        {
            // add player
            _player = new Player(_scale);

            _player.SetPosition(
                left: GameView.Width / 2 - _player.Width / 2,
                top: GameView.Height / 2 - _player.Height / 2);

            _player.SetZ(1);

            GameView.Children.Add(_player);

            _playerHitBox = _player.GetHitBox();
        }

        private void LoadGameElements()
        {
            _playerTemplates = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.PLAYER).Select(x => x.Value).ToArray();
            _collectibleTemplates = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.COLLECTIBLE).Select(x => x.Value).ToArray();
        }

        private void StartGame()
        {
#if DEBUG
            Console.WriteLine("GAME STARTED");
#endif
            HideInGameTextMessage();
            SoundHelper.PlaySound(SoundType.MENU_SELECT);

            _playerTrailCount = 0;
            _playerTrailLength = 2;
            _foodSpawnLimit = 3;
            _foodCount = 0;

            _gameSpeed = _gameSpeedDefault;
            _player.Opacity = 1;

            ResetControls();

            _isGameOver = false;
            _isPowerMode = false;
            //_powerUpType = 0;
            _powerModeDurationCounter = _powerModeDuration;
            _powerUpCount = 0;

            _score = 0;
            _foodCollected = 0;
            ScoreText.Text = "0";

            PlayerHealthBarPanel.Visibility = Visibility.Visible;

            _playerHealthDepletionPoint = _healthDepletePointDefault;
            _playerHealth = 100;
            _playerHealthRejuvenationPoint = _healthGainPointDefault;
            _playerHealthDepletionCounter = 10;

            //_playerTrailSpawnCounter = _playerTrailSpawnCounterDefault;

            foreach (GameObject x in GameView.GetGameObjects<PlayerTrail>())
            {
                GameView.AddDestroyableGameObject(x);
            }

            foreach (GameObject x in GameView.GetGameObjects<Collectible>())
            {
                GameView.AddDestroyableGameObject(x);
            }

            foreach (GameObject x in GameView.GetGameObjects<PowerUp>())
            {
                GameView.AddDestroyableGameObject(x);
            }

            RemoveGameObjects();
            StartGameSounds();

            var directions = Enum.GetNames<MovementDirection>();
            UpdateMovementDirection((MovementDirection)_random.Next(1, directions.Length));

            RunGame();

            _player.SetSize(width: Constants.PLAYER_SIZE * _scale, height: Constants.PLAYER_SIZE * _scale);

            foreach (PlayerTrail trail in GameView.GetGameObjects<PlayerTrail>())
            {
                trail.SetRoundness(_scale);
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

        private void ResetControls()
        {
            _player.MovementDirection = MovementDirection.None;
            _pointerPosition = null;
        }

        private void GameViewLoop()
        {
            ScoreText.Text = _score.ToString("#");
            PlayerHealthBar.Value = _playerHealth;

            SpawnGameObjects();
            UpdateGameObjects();
            RemoveGameObjects();

            if (_isPowerMode)
            {
                PowerUpCoolDown();
                if (_powerModeDurationCounter <= 0)
                    PowerDown();
            }

            DepleteHealth();
            ScaleDifficulty();
        }

        private void SpawnGameObjects()
        {
            if (_powerUpCount < _powerUpSpawnLimit)
            {
                _powerUpSpawnCounter--;

                if (_powerUpSpawnCounter < 1)
                {
                    SpawnPowerUp();
                    _powerUpSpawnCounter = _random.Next(1000, 1200);
                }
            }

            if (_foodCount < _foodSpawnLimit)
            {
                _foodSpawnCounter--;

                if (_foodSpawnCounter <= 0)
                {
                    SpawnCollectible();
                    _foodSpawnCounter = 10; // regular spawn
                }
            }
        }

        private void UpdateGameObjects()
        {
            foreach (GameObject x in GameView.GetGameObjects<GameObject>())
            {
                switch ((ElementType)x.Tag)
                {
                    case ElementType.PLAYER:
                        {
                            if (_player.MovementDirection != MovementDirection.None)
                                UpdatePlayer();

                            YummyFaceCoolDown();
                        }
                        break;
                    case ElementType.COLLECTIBLE:
                        {
                            UpdateCollectible(x);
                        }
                        break;
                    case ElementType.PLAYER_TRAIL:
                        {
                            UpdatePlayerTrail(x);
                        }
                        break;
                    case ElementType.POWERUP:
                        {
                            UpdatePowerUp(x);
                        }
                        break;
                    default:
                        break;
                }
            }

            foreach (GameObject x in UnderView.GetGameObjects<GameObject>())
            {
                switch ((ElementType)x.Tag)
                {
                    case ElementType.DIRT:
                        {
                            UpdateDirt(x);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void RemoveGameObjects()
        {
            GameView.RemoveDestroyableGameObjects();
        }

        private void PauseGame()
        {
            InputView.Focus(FocusState.Programmatic);
            ShowInGameTextMessage("GAME_PAUSED");

            _gameViewTimer?.Dispose();

            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            PauseGameSounds();
        }

        private void ResumeGame()
        {
            InputView.Focus(FocusState.Programmatic);
            HideInGameTextMessage();

            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            SoundHelper.ResumeSound(SoundType.BACKGROUND);

            RunGame();
        }

        private void StopGame()
        {
            _gameViewTimer?.Dispose();
            StopGameSounds();
        }

        private void GameOver()
        {
            _isGameOver = true;

            PlayerScoreHelper.PlayerScore = new HungryWormGameScore()
            {
                Score = Math.Ceiling(_score),
                CollectiblesCollected = _foodCollected
            };

            SoundHelper.PlaySound(SoundType.GAME_OVER);
            NavigateToPage(typeof(GameOverPage));
        }

        #endregion

        #region GameObject

        private void MoveGameObject(GameObject gameObject)
        {
            if (_pointerPosition.X > _playerHitBox.Right)
            {
                var distance = ((_pointerPosition.X - _playerHitBox.Right) / _gameSpeed) * _gameSpeedfactor;
                double speed = AdjustSpeed(distance);
                gameObject.SetLeft(gameObject.GetLeft() - speed);
            }

            if (_pointerPosition.X < _playerHitBox.Left)
            {
                var distance = ((_playerHitBox.Left - _pointerPosition.X) / _gameSpeed) * _gameSpeedfactor;
                double speed = AdjustSpeed(distance);
                gameObject.SetLeft(gameObject.GetLeft() + speed);
            }

            if (_pointerPosition.Y < _playerHitBox.Top)
            {
                var distance = ((_playerHitBox.Top - _pointerPosition.Y) / _gameSpeed) * _gameSpeedfactor;
                double speed = AdjustSpeed(distance);
                gameObject.SetTop(gameObject.GetTop() + speed);
            }

            if (_pointerPosition.Y > _playerHitBox.Bottom)
            {
                var distance = ((_pointerPosition.Y - _playerHitBox.Bottom) / _gameSpeed) * _gameSpeedfactor;
                double speed = AdjustSpeed(distance);
                gameObject.SetTop(gameObject.GetTop() - speed);
            }
        }

        private double AdjustSpeed(double distance)
        {
            var speed = _gameSpeed * distance;
            speed = speed < _gameSpeedDefault ? _gameSpeedDefault : speed;
            return speed;
        }

        private void RecycleGameObject(GameObject gameObject)
        {
            if (gameObject.GetLeft() > _windowWidth)
                gameObject.SetLeft(0 - gameObject.Width);

            if (gameObject.GetLeft() + gameObject.Width < 0)
                gameObject.SetLeft(_windowWidth);

            if (gameObject.GetTop() > _windowHeight)
                gameObject.SetTop(0 - gameObject.Height);

            if (gameObject.GetTop() + gameObject.Height < 0)
                gameObject.SetTop(_windowHeight);
        }

        #endregion

        #region Player

        private void UpdatePlayer()
        {
            SpawnPlayerTrail();
        }

        public bool CollisionWithSelf()
        {
            if (_player != null)
            {
                foreach (var target in GameView.GetGameObjects<PlayerTrail>())
                {
                    if (target.GetLeft() == _player.GetLeft() && target.GetTop() == _player.GetTop())
                    {
                        Console.WriteLine("COLLIDED WITH SELF");
                        return true;
                    }
                }
            }

            return false;
        }

        public void UpdateMovementDirection(MovementDirection movementDirection)
        {
            if (_player != null && !_isGameOver)
                _player.UpdateMovementDirection(movementDirection);
        }

        private void SetYummyFace()
        {
            _playerYummyFaceCounter = 50;
            _player.SetContent(_playerTemplates[_random.Next(0, _playerTemplates.Length)]);
        }

        private void YummyFaceCoolDown()
        {
            if (_playerYummyFaceCounter > 0)
            {
                _playerYummyFaceCounter--;

                if (_playerYummyFaceCounter <= 0)
                    _player.SetContent(_playerTemplates.First());
            }
        }

        #endregion

        #region PlayerTrail

        private void SpawnPlayerTrail()
        {
            double left = _playerHitBox.X;
            double top = _playerHitBox.Y;

            PlayerTrail playerTrail = new(_scale);
            playerTrail.SetPosition(left, top);
            playerTrail.SetZ(0);

            GameView.Children.Add(playerTrail);
            _playerTrailCount++;
        }

        private void UpdatePlayerTrail(GameObject playerTrail)
        {
            MoveGameObject(playerTrail);

            if (_playerTrailCount > _playerTrailLength)
            {
                GameView.AddDestroyableGameObject(playerTrail);
                _playerTrailCount--;
            }
        }

        #endregion

        #region Collectible

        private void SpawnCollectible()
        {
            Collectible collectible = new(_scale);

            collectible.SetContent(_collectibleTemplates[_random.Next(0, _collectibleTemplates.Length)]);

            collectible.SetPosition(
                left: _random.Next(0, (int)_windowWidth),
                top: _random.Next(0, (int)_windowHeight));

            GameView.Children.Add(collectible);
            _foodCount++;
        }

        private void UpdateCollectible(GameObject collectible)
        {
            MoveGameObject(collectible);

            // if object goes out of bounds then make it reenter game view
            RecycleGameObject(collectible);

            if (collectible.IsFlaggedForShrinking)
            {
                collectible.Shrink();

                if (collectible.HasShrinked)
                    GameView.AddDestroyableGameObject(collectible);
            }
            else
            {
                if (_playerHitBox.IntersectsWith(collectible.GetHitBox()))
                {
                    collectible.IsFlaggedForShrinking = true;
                    Collectible();
                }

                // in power mode draw the collectible closer
                if (_isPowerMode)
                {
                    var playerHitBoxDistant = _player.GetDistantHitBox();
                    var collectibleHitBoxDistant = collectible.GetDistantHitBox();

                    if (playerHitBoxDistant.IntersectsWith(collectibleHitBoxDistant))
                    {
                        var collectibleHitBox = collectible.GetHitBox();

                        if (_playerHitBox.Left < collectibleHitBox.Left)
                            collectible.SetLeft(collectible.GetLeft() - _gameSpeed * 1.5);

                        if (collectibleHitBox.Right < _playerHitBox.Left)
                            collectible.SetLeft(collectible.GetLeft() + _gameSpeed * 1.5);

                        if (collectibleHitBox.Top > _playerHitBox.Bottom)
                            collectible.SetTop(collectible.GetTop() - _gameSpeed * 1.5);

                        if (collectibleHitBox.Bottom < _playerHitBox.Top)
                            collectible.SetTop(collectible.GetTop() + _gameSpeed * 1.5);
                    }
                }
            }
        }

        private void Collectible()
        {
            SoundHelper.PlayRandomSound(SoundType.ATE_FOOD);

            AddScore(10);
            AddHealth(_playerHealthRejuvenationPoint);

            _foodCount--;
            _foodCollected++;

            SetYummyFace();
        }

        #endregion

        #region Dirt

        private void SpawnDirt()
        {
            var dirt = new Dirt((double)_random.Next(5, 100) * _scale);

            dirt.SetPosition(_random.Next(50, (int)_windowWidth - 50), _random.Next(50, (int)_windowHeight - 50));
            UnderView.Children.Add(dirt);
        }

        private void UpdateDirt(GameObject dirt)
        {
            MoveGameObject(dirt);

            // if object goes out of bounds then make it reenter game view
            RecycleGameObject(dirt);
        }

        #endregion

        #region Health

        private void AddHealth(double health)
        {
            if (_playerHealth < 100)
            {
                if (_playerHealth + health > 100)
                    health = _playerHealth + health - 100;

                _playerHealth += health;
            }
        }

        private void DepleteHealth()
        {
            _playerHealthDepletionCounter--;

            if (_playerHealthDepletionCounter <= 0)
            {
                _playerHealth -= _playerHealthDepletionPoint;
                _playerHealthDepletionCounter = 10;
            }

            if (_playerHealth <= 0)
                GameOver();
        }

        #endregion

        #region PowerUp

        private void SpawnPowerUp()
        {
            PowerUp powerUp = new(_scale);

            powerUp.SetPosition(
                left: _random.Next(0, (int)(GameView.Width - 55)),
                top: _random.Next(100, (int)GameView.Height) * -1);

            GameView.Children.Add(powerUp);
        }

        private void UpdatePowerUp(GameObject powerUp)
        {
            MoveGameObject(powerUp);

            // if object goes out of bounds then make it reenter game view
            RecycleGameObject(powerUp);

            if (_playerHitBox.IntersectsWith(powerUp.GetHitBox()))
            {
                GameView.AddDestroyableGameObject(powerUp);
                PowerUp(powerUp);
            }
        }

        private void PowerUp(GameObject powerUp)
        {
            _isPowerMode = true;
            _powerModeDurationCounter = _powerModeDuration;
            _powerUpCount++;

            powerUpText.Visibility = Visibility.Visible;
            SoundHelper.PlaySound(SoundType.POWER_UP);
        }

        private void PowerUpCoolDown()
        {
            _powerModeDurationCounter -= 1;
            double remainingPow = (double)_powerModeDurationCounter / (double)_powerModeDuration * 4;

            powerUpText.Text = "";

            for (int i = 0; i < remainingPow; i++)
            {
                powerUpText.Text += "⚡";
            }
        }

        private void PowerDown()
        {
            _isPowerMode = false;
            _powerUpCount--;

            powerUpText.Visibility = Visibility.Collapsed;
            SoundHelper.PlaySound(SoundType.POWER_DOWN);
        }

        #endregion

        #endregion

        #region Score

        private void AddScore(double score)
        {
            _score += score;

            if (_playerTrailLength < _playerTrailLengthLimit)
                _playerTrailLength += 1;
        }

        #endregion

        #region Difficulty

        private void ScaleDifficulty()
        {
            if (_score >= 10 && _score < 20)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 1;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 1;
                _gameSpeed = _gameSpeedDefault + 0.2 * 1;
            }
            if (_score >= 20 && _score < 30)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 2;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 2;
                _gameSpeed = _gameSpeedDefault + 0.2 * 2;
            }
            if (_score >= 30 && _score < 40)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 3;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 3;
                _gameSpeed = _gameSpeedDefault + 0.2 * 3;
            }
            if (_score >= 40 && _score < 50)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 4;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 4;
                _gameSpeed = _gameSpeedDefault + 0.2 * 4;
            }
            if (_score >= 50 && _score < 80)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 5;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 5;
                _gameSpeed = _gameSpeedDefault + 0.2 * 5;
            }
            if (_score >= 80 && _score < 100)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 6;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 6;
                _gameSpeed = _gameSpeedDefault + 0.2 * 6;
            }
            if (_score >= 100 && _score < 130)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 7;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 7;
                _gameSpeed = _gameSpeedDefault + 0.2 * 7;
            }
            if (_score >= 130 && _score < 150)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 8;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 8;
                _gameSpeed = _gameSpeedDefault + 0.2 * 8;
            }
            if (_score >= 150 && _score < 180)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 9;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 9;
                _gameSpeed = _gameSpeedDefault + 0.2 * 9;
            }
            if (_score >= 180 && _score < 200)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 10;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 10;
                _gameSpeed = _gameSpeedDefault + 0.2 * 10;
            }
            if (_score >= 200 && _score < 220)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 11;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 11;
                _gameSpeed = _gameSpeedDefault + 0.2 * 11;
            }
            if (_score >= 220 && _score < 250)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 12;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 12;
                _gameSpeed = _gameSpeedDefault + 0.2 * 12;
            }
            if (_score >= 250 && _score < 300)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 13;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 13;
                _gameSpeed = _gameSpeedDefault + 0.2 * 13;
            }
            if (_score >= 300 && _score < 350)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 14;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 14;
                _gameSpeed = _gameSpeedDefault + 0.2 * 14;
            }
            if (_score >= 350 && _score < 400)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 15;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 15;
                _gameSpeed = _gameSpeedDefault + 0.2 * 15;
            }
            if (_score >= 400 && _score < 500)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 16;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 16;
                _gameSpeed = _gameSpeedDefault + 0.2 * 16;
            }
            if (_score >= 500 && _score < 600)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 17;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 17;
                _gameSpeed = _gameSpeedDefault + 0.2 * 17;
            }
            if (_score >= 600 && _score < 700)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 18;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 18;
                _gameSpeed = _gameSpeedDefault + 0.2 * 18;
            }
            if (_score >= 700 && _score < 800)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 19;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 19;
                _gameSpeed = _gameSpeedDefault + 0.2 * 19;
            }
            if (_score >= 800 && _score < 900)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 20;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 20;
                _gameSpeed = _gameSpeedDefault + 0.2 * 20;
            }
            if (_score >= 900 && _score < 1000)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 21;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 21;
                _gameSpeed = _gameSpeedDefault + 0.2 * 21;
            }
            if (_score >= 1000 && _score < 1200)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 22;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 22;
                _gameSpeed = _gameSpeedDefault + 0.2 * 22;
            }
            if (_score >= 1200 && _score < 1400)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 23;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 23;
                _gameSpeed = _gameSpeedDefault + 0.2 * 23;
            }
            if (_score >= 1400 && _score < 1600)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 24;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 24;
                _gameSpeed = _gameSpeedDefault + 0.2 * 24;
            }
            if (_score >= 1600 && _score < 1800)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 25;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 25;
                _gameSpeed = _gameSpeedDefault + 0.2 * 25;
            }
            if (_score >= 1800 && _score < 2000)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 26;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 26;
                _gameSpeed = _gameSpeedDefault + 0.2 * 26;
            }
            if (_score >= 2000 && _score < 2200)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 27;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 27;
                _gameSpeed = _gameSpeedDefault + 0.2 * 27;
            }
            if (_score >= 2200 && _score < 2400)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 28;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 28;
                _gameSpeed = _gameSpeedDefault + 0.2 * 28;
            }
            if (_score >= 2400 && _score < 2600)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 29;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 29;
                _gameSpeed = _gameSpeedDefault + 0.2 * 29;
            }
            if (_score >= 2600 && _score < 2800)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 30;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 30;
                _gameSpeed = _gameSpeedDefault + 0.2 * 30;
            }
            if (_score >= 2800 && _score < 3000)
            {
                _playerHealthRejuvenationPoint = _healthGainPointDefault + 0.1 * 31;
                _playerHealthDepletionPoint = _healthDepletePointDefault + 0.1 * 31;
                _gameSpeed = _gameSpeedDefault + 0.2 * 31;
            }
        }

        #endregion

        #region Sound

        private void StartGameSounds()
        {
            SoundHelper.RandomizeSound(SoundType.BACKGROUND);
            SoundHelper.PlaySound(SoundType.BACKGROUND);
        }

        private void StopGameSounds()
        {
            SoundHelper.StopSound(SoundType.BACKGROUND);
        }

        private void PauseGameSounds()
        {
            SoundHelper.PauseSound(SoundType.BACKGROUND);
        }

        #endregion

        #region Page

        private void SetViewSize()
        {
            _scale = ScalingHelper.GetGameObjectScale(_windowWidth);

            UnderView.Width = _windowWidth;
            UnderView.Height = _windowHeight;

            GameView.Width = _windowWidth;
            GameView.Height = _windowHeight;

            if (_player is not null)
            {
                _player.SetSize(width: Constants.PLAYER_SIZE * _scale, height: Constants.PLAYER_SIZE * _scale);

                foreach (PlayerTrail trail in GameView.GetGameObjects<PlayerTrail>())
                {
                    trail.SetRoundness(_scale);
                }
            }
        }

        private void NavigateToPage(Type pageType)
        {
            SoundHelper.PlaySound(SoundType.MENU_SELECT);
            App.NavigateToPage(pageType);
        }

        #endregion

        #region In Game Message

        private void ShowInGameTextMessage(string resourceKey)
        {
            InGameMessageText.Text = LocalizationHelper.GetLocalizedResource(resourceKey);
            InGameMessagePanel.Visibility = Visibility.Visible;
        }

        private void HideInGameTextMessage()
        {
            InGameMessageText.Text = "";
            InGameMessagePanel.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion
    }
}
