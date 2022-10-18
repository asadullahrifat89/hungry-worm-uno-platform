using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Uno.Extensions;
using Windows.Foundation;
using Windows.System;

namespace HungryWorm
{
    public sealed partial class GamePage : Page
    {
        #region Fields

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly Random _random = new();
        //private int _markNum;

        private double _gameSpeed = 6;
        private readonly double _gameSpeedDefault = 6;

        private int _playerSpeed = 6;
        private int _playerSpeedDefault = 6;

        //private int _powerUpSpawnCounter = 30;
        //private int _powerModeCounter = 500;
        private readonly int _powerModeDelay = 500;

        private int _lives;
        private readonly int _maxLives = 3;

        private int _healthSpawnCounter = 500;

        private double _score;
        private int _foodCollected;

        private bool _isGameOver;
        //private bool _isPowerMode;

        //private bool _isPointerActivated;
        private Point _pointerPosition;

        private double _windowHeight, _windowWidth;
        private double _scale;

        //private PowerUpType _powerUpType;

        private Player _player;
        private Rect _playerHitBox;

        private int _foodSpawnCounter;
        private int _foodSpawnLimit;
        private int _foodCount;

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
        private readonly int _playerTrailLengthLimit = 20;

        private int _playerYummyFaceCounter;

        private int _playerTrailSpawnCounter;
        private int _playerTrailSpawnCounterDefault = 1;

        #endregion

        #region Ctor

        public GamePage()
        {
            InitializeComponent();

            _isGameOver = true;
            ShowInGameTextMessage("TAP_ON_SCREEN_TO_BEGIN");

            _windowHeight = Window.Current.Bounds.Height;
            _windowWidth = Window.Current.Bounds.Width;

            //TODO: remove this to start page
            SoundHelper.LoadGameSounds();

            LoadGameElements();
            PopulateGameViews();

            Loaded += GamePage_Loaded;
            Unloaded += GamePage_Unloaded;
        }

        #endregion

        #region Events

        #region Page

        private async void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            SizeChanged += GamePage_SizeChanged;

            //TODO: remove this to start page
            await LocalizationHelper.LoadLocalizationKeys();

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
                //_isPointerActivated = true;

                PointerPoint point = e.GetCurrentPoint(GameView);
                _pointerPosition = point.Position;

                double left = _player.GetLeft();
                double top = _player.GetTop();

                double playerMiddleX = left + _player.Width / 2;
                double playerMiddleY = top + _player.Height / 2;

                // move right
                if (_player.MovementDirection != MovementDirection.Right && _pointerPosition.X > playerMiddleX + _playerSpeed && left + _player.Width < GameView.Width)
                {
                    //_moveRight = true;
                    UpdateMovementDirection(MovementDirection.Right);
                    return;
                }

                // move up
                if (_player.MovementDirection != MovementDirection.Up && _pointerPosition.Y < playerMiddleY - _playerSpeed)
                {
                    //_moveUp = true;
                    UpdateMovementDirection(MovementDirection.Up);
                    return;
                }

                // move left
                if (_player.MovementDirection != MovementDirection.Left && _pointerPosition.X < playerMiddleX - _playerSpeed && left > 0)
                {
                    //_moveLeft = true;
                    UpdateMovementDirection(MovementDirection.Left);
                    return;
                }

                // move down
                if (_player.MovementDirection != MovementDirection.Down && _pointerPosition.Y > playerMiddleY + _playerSpeed)
                {
                    //_moveDown = true;
                    UpdateMovementDirection(MovementDirection.Down);
                    return;
                }
            }
        }

        private void InputView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            //if (_isPointerActivated)
            //{
            //    PointerPoint point = e.GetCurrentPoint(GameView);
            //    _pointerPosition = point.Position;            
            //}
        }

        private void InputView_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //_isPointerActivated = false;
            _pointerPosition = null;
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            //_isPointerActivated = false;

            switch (e.Key)
            {
                case VirtualKey.Left:
                    {
                        UpdateMovementDirection(MovementDirection.Left);
                    }
                    break;
                case VirtualKey.Up:
                    {
                        UpdateMovementDirection(MovementDirection.Up);
                    }
                    break;
                case VirtualKey.Right:
                    {
                        UpdateMovementDirection(MovementDirection.Right);
                    }
                    break;
                case VirtualKey.Down:
                    {
                        UpdateMovementDirection(MovementDirection.Down);
                    }
                    break;

                default:
                    break;
            }
        }

        private void OnKeyUP(object sender, KeyRoutedEventArgs e)
        {

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
            //NavigateToPage(typeof(StartPage));
        }

        #endregion

        #endregion

        #region Methods

        #region Animation

        #region Game

        private void PopulateGameViews()
        {
            Console.WriteLine("INITIALIZING GAME");

            SetViewSize();

            PopulateUnderView();
            PopulateGameView();
            //PopulateOverView();
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
            _player = new Player(Constants.PLAYER_SIZE * _scale);

            _player.SetPosition(
                left: GameView.Width / 2 - _player.Width / 2,
                top: GameView.Height / 2 - _player.Height / 2);

            _player.SetZ(1);

            GameView.Children.Add(_player);

            //// add 5 collectibles
            //for (int i = 0; i < 3; i++)
            //{
            //    SpawnCollectible(); // on game init
            //}
        }

        private void PopulateOverView()
        {
            // add some clouds above
            for (int i = 0; i < 5; i++)
            {
                //var scaleFactor = _random.Next(1, 4);
                //var scaleReverseFactor = _random.Next(-1, 2);

                //var cloud = new Cloud()
                //{
                //    Width = Constants.CLOUD_WIDTH * _scale,
                //    Height = Constants.CLOUD_HEIGHT * _scale,
                //    RenderTransform = new CompositeTransform()
                //    {
                //        ScaleX = scaleFactor * scaleReverseFactor,
                //        ScaleY = scaleFactor,
                //    }
                //};

                //RandomizeCloudPosition(cloud);
                //OverView.Children.Add(cloud);
            }
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

            _lives = _maxLives;
            //SetLives();

            _playerTrailLength = 2;
            _foodSpawnLimit = 3;
            _foodCount = 0;

            _gameSpeed = _gameSpeedDefault;
            _playerSpeed = _playerSpeedDefault;
            _player.Opacity = 1;

            ResetControls();

            _isGameOver = false;
            //_isPowerMode = false;
            //_powerUpType = 0;
            //_powerModeCounter = _powerModeDelay;

            _score = 0;
            _foodCollected = 0;
            ScoreText.Text = "0";

            PlayerHealthBarPanel.Visibility = Visibility.Visible;

            _playerHealthDepletionPoint = _healthDepletePointDefault;
            _playerHealth = 100;
            _playerHealthRejuvenationPoint = _healthGainPointDefault;
            _playerHealthDepletionCounter = 10;

            _playerTrailSpawnCounter = _playerTrailSpawnCounterDefault;

            UpdateMovementDirection(MovementDirection.Right);
            RemoveGameObjects();
            StartGameSounds();
            RunGame();
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
            //_isPointerActivated = false;
            _pointerPosition = null;
        }

        private void GameViewLoop()
        {
            ScoreText.Text = _score.ToString("#");
            PlayerHealthBar.Value = _playerHealth;

            SpawnGameObjects();
            UpdateGameObjects();
            RemoveGameObjects();

            DepleteHealth();
            ScaleDifficulty();
        }

        private void SpawnGameObjects()
        {
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
            //SeaView.RemoveDestroyableGameObjects();
            //UnderView.RemoveDestroyableGameObjects();
            GameView.RemoveDestroyableGameObjects();
            //OverView.RemoveDestroyableGameObjects();
        }

        private void PauseGame()
        {
            InputView.Focus(FocusState.Programmatic);
            ShowInGameTextMessage("GAME_PAUSED");

            _gameViewTimer?.Dispose();

            ResetControls();

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
            //TODO: this will be done automatically once more pages are developed
            StopGame();

            _isGameOver = true;

            PlayerScoreHelper.PlayerScore = new HungryWormScore()
            {
                Score = Math.Ceiling(_score),
                CollectiblesCollected = _foodCollected
            };

            SoundHelper.PlaySound(SoundType.GAME_OVER);
            //NavigateToPage(typeof(GameOverPage));
        }

        private double DecreaseSpeed(double speed)
        {
            //if (_isPowerMode && _powerUpType == PowerUpType.SLOW_DOWN_TIME)
            //    speed /= 3;

            return speed;
        }

        #endregion

        #region Game Object

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
            //if (CollisionWithSelf())
            //    GameOver();

            //RecycleGameObject(_player);
            SpawnPlayerTrail();

            //_player.SetZ(1);
            _playerHitBox = _player.GetHitBox(_scale);
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
            _playerTrailSpawnCounter--;

            if (_playerTrailSpawnCounter <= 0)
            {
                _playerTrailSpawnCounter = _playerTrailSpawnCounterDefault;

                //double offset = 20 * _scale;

                //double left = _player.GetLeft() + (_player.MovementDirection == MovementDirection.Right ? offset * -1 : (_player.MovementDirection == MovementDirection.Left ? offset : 0));
                //double top = _player.GetTop() + (_player.MovementDirection == MovementDirection.Down ? offset * -1 : (_player.MovementDirection == MovementDirection.Up ? offset : 0));

                double left = _player.GetLeft();
                double top = _player.GetTop();

                PlayerTrail playerTrail = new(Constants.PLAYER_TRAIL_SIZE * _scale);
                playerTrail.SetPosition(left, top);
                playerTrail.SetZ(0);
                playerTrail.UpdateMovementDirection(_player.MovementDirection);

                GameView.Children.Add(playerTrail);
                _playerTrailCount++;
            }
        }

        private void UpdatePlayerTrail(GameObject playerTrail)
        {
            switch (_player.MovementDirection)
            {
                case MovementDirection.Right:
                    playerTrail.SetLeft(playerTrail.GetLeft() - _gameSpeed);
                    break;
                case MovementDirection.Left:
                    playerTrail.SetLeft(playerTrail.GetLeft() + _gameSpeed);
                    break;
                case MovementDirection.Up:
                    playerTrail.SetTop(playerTrail.GetTop() + _gameSpeed);
                    break;
                case MovementDirection.Down:
                    playerTrail.SetTop(playerTrail.GetTop() - _gameSpeed);
                    break;
                default:
                    break;
            }

            if (_playerTrailCount > _playerTrailLength)
            {
                var playerTrails = GameView.GetGameObjects<PlayerTrail>().ToArray();

                GameView.AddDestroyableGameObject(playerTrails[0]);
                _playerTrailCount--;

                // give tail a proper border
                var tail = playerTrails[1];
                tail.BorderThickness = new Thickness(5);
            }

            //if (_length > _maxLength)
            //{
            //    GameView.AddDestroyableGameObject(GameView.GetGameObjects<PlayerTrail>().First());
            //    _length--;
            //}         
        }

        #endregion

        #region Collectible

        private void SpawnCollectible()
        {
            Collectible collectible = new(Constants.COLLECTIBLE_SIZE * _scale);

            collectible.SetContent(_collectibleTemplates[_random.Next(0, _collectibleTemplates.Length)]);

            collectible.SetPosition(
                left: _random.Next(0, (int)_windowWidth),
                top: _random.Next(0, (int)_windowHeight));

            GameView.Children.Add(collectible);
            _foodCount++;
        }

        private void UpdateCollectible(GameObject collectible)
        {
            switch (_player.MovementDirection)
            {
                case MovementDirection.Right:
                    collectible.SetLeft(collectible.GetLeft() - _gameSpeed);
                    break;
                case MovementDirection.Left:
                    collectible.SetLeft(collectible.GetLeft() + _gameSpeed);
                    break;
                case MovementDirection.Up:
                    collectible.SetTop(collectible.GetTop() + _gameSpeed);
                    break;
                case MovementDirection.Down:
                    collectible.SetTop(collectible.GetTop() - _gameSpeed);
                    break;
                default:
                    break;
            }

            // if object goes out of bounds then make it reenter game view
            RecycleGameObject(collectible);

            if (_playerHitBox.IntersectsWith(collectible.GetHitBox(_scale)))
            {
                GameView.AddDestroyableGameObject(collectible);
                Collectible();
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
            //SpawnCollectible();
        }

        #endregion

        #region Dirt

        private void SpawnDirt()
        {
            var dot = new Dirt((double)_random.Next(5, 100) * _scale);

            dot.SetPosition(_random.Next(50, (int)_windowWidth - 50), _random.Next(50, (int)_windowHeight - 50));
            UnderView.Children.Add(dot);
        }

        private void UpdateDirt(GameObject dirt)
        {
            switch (_player.MovementDirection)
            {
                case MovementDirection.Right:
                    dirt.SetLeft(dirt.GetLeft() - _gameSpeed);
                    break;
                case MovementDirection.Left:
                    dirt.SetLeft(dirt.GetLeft() + _gameSpeed);
                    break;
                case MovementDirection.Up:
                    dirt.SetTop(dirt.GetTop() + _gameSpeed);
                    break;
                case MovementDirection.Down:
                    dirt.SetTop(dirt.GetTop() - _gameSpeed);
                    break;
                default:
                    break;
            }

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

        #endregion

        #region Score

        private void AddScore(double score)
        {
            //if (_isPowerMode)
            //{
            //    switch (_powerUpType)
            //    {
            //        case PowerUpType.DOUBLE_SCORE:
            //            score *= 2;
            //            break;
            //        case PowerUpType.QUAD_SCORE:
            //            score *= 4;
            //            break;
            //        default:
            //            break;
            //    }
            //}

            //TODO: decide if length effect to keep or not
            if (_playerTrailLength < _playerTrailLengthLimit)
                _playerTrailLength += 1;

            _score += score;
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

            //SeaView.Width = _windowWidth;
            //SeaView.Height = _windowHeight;

            UnderView.Width = _windowWidth;
            UnderView.Height = _windowHeight;

            GameView.Width = _windowWidth;
            GameView.Height = _windowHeight;

            //OverView.Width = _windowWidth;
            //OverView.Height = _windowHeight;
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
