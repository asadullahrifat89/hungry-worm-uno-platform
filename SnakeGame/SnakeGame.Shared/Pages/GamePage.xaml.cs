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

namespace SnakeGame
{
    public sealed partial class GamePage : Page
    {
        #region Own

        #region Fields

        private PeriodicTimer _gameViewTimer;
        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        private readonly Random _random = new();

        private Rect _playerHitBox;

        private int _gameSpeed;
        private readonly int _defaultGameSpeed = 5;

        private int _playerSpeed = 5;
        private int _defaultPlayerSpeed = 5;
        private int _markNum;

        private int _powerUpSpawnCounter = 30;

        private int _powerModeCounter = 500;
        private readonly int _powerModeDelay = 500;

        private int _lives;
        private readonly int _maxLives = 3;
        private int _healthSpawnCounter = 500;
        private int _damageRecoveryOpacityFrameSkip;

        private int _collectibleSpawnCounter = 200;

        private double _score;
        private int _collectiblesCollected;

        private int _islandSpawnCounter;

        private bool _isGameOver;
        private bool _isPowerMode;

        private bool _isRecoveringFromDamage;
        private bool _isPointerActivated;

        private int _accelerationCounter;

        private int _damageRecoveryCounter = 100;
        private readonly int _damageRecoveryDelay = 500;

        private double _windowHeight, _windowWidth;
        private double _scale;
        private Point _pointerPosition;

        //private PowerUpType _powerUpType;

        private Player _player;
        private int _length;
        private int _maxLength = 50;

        private Uri[] _playerTemplates;
        private Uri[] _collectibleTemplates;
        private int _collectiblesFaceCounter;

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
                InputView.Focus(FocusState.Programmatic);
                StartGame();
            }
            else
            {
                _isPointerActivated = true;

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
            if (_isPointerActivated)
            {
                PointerPoint point = e.GetCurrentPoint(GameView);
                _pointerPosition = point.Position;
            }
        }

        private void InputView_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //_isPointerActivated = false;
            //_pointerPosition = null;
        }

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            //_moveRight = false;
            //_moveLeft = false;
            //_moveDown = false;
            //_moveUp = false;

            _isPointerActivated = false;

            switch (e.Key)
            {
                case VirtualKey.Left:
                    {
                        //_moveLeft = true;
                        //_moveRight = false;
                        UpdateMovementDirection(MovementDirection.Left);
                    }
                    break;
                case VirtualKey.Up:
                    {
                        //_moveUp = true;
                        //_moveDown = false;
                        UpdateMovementDirection(MovementDirection.Up);
                    }
                    break;
                case VirtualKey.Right:
                    {
                        //_moveRight = true;
                        //_moveLeft = false;
                        UpdateMovementDirection(MovementDirection.Right);
                    }
                    break;
                case VirtualKey.Down:
                    {
                        //_moveDown = true;
                        //_moveUp = false;
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
            // add some cars underneath
            for (int i = 0; i < 50; i++)
            {
                SpawnDirt();
            }

            //// add some clouds underneath
            //for (int i = 0; i < 15; i++)
            //{
            //    //var scaleFactor = _random.Next(1, 4);
            //    //var scaleReverseFactor = _random.Next(-1, 2);

            //    //var cloud = new Cloud()
            //    //{
            //    //    Width = Constants.CLOUD_WIDTH * _scale,
            //    //    Height = Constants.CLOUD_HEIGHT * _scale,
            //    //    RenderTransform = new CompositeTransform()
            //    //    {
            //    //        ScaleX = scaleFactor * scaleReverseFactor,
            //    //        ScaleY = scaleFactor,
            //    //    }
            //    //};

            //    //RandomizeCloudPosition(cloud);
            //    //UnderView.Children.Add(cloud);
            //}
        }

        private void PopulateGameView()
        {
            // add 5 collectibles
            for (int i = 0; i < 5; i++)
            {
                SpawnCollectible();
            }

            // add player
            _player = new Player(Constants.PLAYER_SIZE * _scale);

            _player.SetPosition(
                left: GameView.Width / 2,
                top: GameView.Height / 2);

            GameView.Children.Add(_player);
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

            _gameSpeed = _defaultGameSpeed;
            _playerSpeed = _defaultPlayerSpeed;
            _player.Opacity = 1;

            ResetControls();

            _isGameOver = false;
            _isPowerMode = false;
            //_powerUpType = 0;
            _powerModeCounter = _powerModeDelay;

            _isRecoveringFromDamage = false;
            _damageRecoveryCounter = _damageRecoveryDelay;

            _score = 0;
            _collectiblesCollected = 0;
            scoreText.Text = "0";

            RemoveGameObjects();

            //SpawnCollectible();

            StartGameSounds();
            RunGame();
            UpdateMovementDirection(MovementDirection.Right);
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
            _isPointerActivated = false;
        }

        private void GameViewLoop()
        {
            //AddScore(0.05d); // increase the score by .5 each tick of the timer
            scoreText.Text = _score.ToString("#");

            SpawnGameObjects();
            UpdateGameObjects();
            RemoveGameObjects();

            if (_isGameOver)
                return;

            // as you progress in the game you will score higher and game speed will go up
            //ScaleDifficulty();
        }

        private void SpawnGameObjects()
        {
            if (GameView.Children.OfType<Collectible>().Count() < 10)
            {
                _collectibleSpawnCounter--;

                if (_collectibleSpawnCounter < 1)
                {
                    SpawnCollectible();
                    _collectibleSpawnCounter = _random.Next(30, 80);
                }
            }
        }

        private void UpdateGameObjects()
        {
            foreach (GameObject x in GameView.Children.OfType<GameObject>())
            {
                switch ((ElementType)x.Tag)
                {
                    case ElementType.PLAYER:
                        {
                            if (_player.MovementDirection != MovementDirection.None)
                                UpdatePlayer();

                            if (_collectiblesFaceCounter > 0)
                            {
                                _collectiblesFaceCounter--;

                                if (_collectiblesFaceCounter <= 0)
                                    _player.SetContent(_playerTemplates.First());
                            }
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

            foreach (GameObject x in UnderView.Children.OfType<GameObject>())
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
            SoundHelper.ResumeSound(SoundType.CAR_ENGINE);

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

            PlayerScoreHelper.PlayerScore = new SnakeGameScore()
            {
                Score = Math.Ceiling(_score),
                CollectiblesCollected = _collectiblesCollected
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
                gameObject.SetLeft(0);

            if (gameObject.GetLeft() < 0)
                gameObject.SetLeft(_windowWidth);

            if (gameObject.GetTop() > _windowHeight)
                gameObject.SetTop(0);

            if (gameObject.GetTop() < 0)
                gameObject.SetTop(_windowHeight);
        }

        #endregion

        #region Player

        private void UpdatePlayer()
        {
            if (CollisionWithSelf())
            {
                StopGame();
            }

            var effectiveSpeed = _playerSpeed;

            double left = _player.GetLeft();
            double top = _player.GetTop();

            switch (_player.MovementDirection)
            {
                case MovementDirection.Right:
                    _player.SetLeft(left + effectiveSpeed);
                    break;
                case MovementDirection.Left:
                    _player.SetLeft(left - effectiveSpeed);
                    break;
                case MovementDirection.Up:
                    _player.SetTop(top - effectiveSpeed);
                    break;
                case MovementDirection.Down:
                    _player.SetTop(top + effectiveSpeed);
                    break;
                default:
                    break;
            }

            RecycleGameObject(_player);

            //if (_player.GetLeft() > _windowWidth)
            //    _player.SetLeft(0);

            //if (_player.GetLeft() < 0)
            //    _player.SetLeft(_windowWidth);

            //if (_player.GetTop() > _windowHeight)
            //    _player.SetTop(0);

            //if (_player.GetTop() < 0)
            //    _player.SetTop(_windowHeight);

            SpawnPlayerTrail(left, top);

            _player.SetZ(1);
            _playerHitBox = _player.GetHitBox(_scale);
        }

        private void SpawnPlayerTrail(double left, double top)
        {
            PlayerTrail playerTrail = new(Constants.PLAYER_TRAIL_SIZE * _scale);
            playerTrail.SetPosition(left, top);
            playerTrail.SetZ(0);
            playerTrail.UpdateMovementDirection(_player.MovementDirection);

            GameView.Children.Add(playerTrail);
            _length++;

            if (_length > _maxLength)
            {
                GameView.Children.Remove(GameView.Children.OfType<PlayerTrail>().First());
                _length--;
            }
        }

        public bool CollisionWithSelf()
        {

            if (_player != null)
            {
                foreach (var target in GameView.Children.OfType<PlayerTrail>())
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
            if (_player != null)
                _player.UpdateMovementDirection(movementDirection);
        }

        #endregion

        #region Collectible

        private void SpawnCollectible()
        {
            var speed = _random.Next(2, 5);

            Collectible collectible = new(Constants.COLLECTIBLE_SIZE * _scale)
            {
                Speed = speed
            };

            collectible.SetContent(_collectibleTemplates[_random.Next(0, _collectibleTemplates.Length)]);
            collectible.SetPosition(_random.Next(50, (int)_windowWidth - 50), _random.Next(50, (int)_windowHeight - 50));

            GameView.Children.Add(collectible);
        }

        private void UpdateCollectible(GameObject collectible)
        {
            var speed = collectible.Speed;

            switch (_player.MovementDirection)
            {
                case MovementDirection.Right:
                    collectible.SetLeft(collectible.GetLeft() - speed);
                    break;
                case MovementDirection.Left:
                    collectible.SetLeft(collectible.GetLeft() + speed);
                    break;
                case MovementDirection.Up:
                    collectible.SetTop(collectible.GetTop() + speed);
                    break;
                case MovementDirection.Down:
                    collectible.SetTop(collectible.GetTop() - speed);
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
            AddScore(10);
            SoundHelper.PlaySound(SoundType.COLLECTIBLE_COLLECTED);
            //SpawnCollectible();

            _collectiblesCollected++;
            _collectiblesFaceCounter = 50;

            _player.SetContent(_playerTemplates[_random.Next(0, _playerTemplates.Length)]);
        }

        #endregion

        #region Dirt

        private void SpawnDirt()
        {
            var dot = new Dirt((double)_random.Next(5, 100) * _scale)
            {
                Speed = 4
            };

            dot.SetPosition(_random.Next(50, (int)_windowWidth - 50), _random.Next(50, (int)_windowHeight - 50));
            UnderView.Children.Add(dot);
        }

        private void UpdateDirt(GameObject dirt)
        {
            var speed = dirt.Speed;

            switch (_player.MovementDirection)
            {
                case MovementDirection.Right:
                    dirt.SetLeft(dirt.GetLeft() - speed);
                    break;
                case MovementDirection.Left:
                    dirt.SetLeft(dirt.GetLeft() + speed);
                    break;
                case MovementDirection.Up:
                    dirt.SetTop(dirt.GetTop() + speed);
                    break;
                case MovementDirection.Down:
                    dirt.SetTop(dirt.GetTop() - speed);
                    break;
                default:
                    break;
            }

            // if object goes out of bounds then make it reenter game view
            RecycleGameObject(dirt);
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
            //_maxLength += 5;
            _score += score;
        }

        #endregion

        #region Difficulty

        private void ScaleDifficulty()
        {
            if (_score >= 10 && _score < 20)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 1;
                _playerSpeed = _defaultPlayerSpeed + (1 / 2);
            }
            if (_score >= 20 && _score < 30)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 2;
                _playerSpeed = _defaultPlayerSpeed + (2 / 2);
            }
            if (_score >= 30 && _score < 40)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 3;
                _playerSpeed = _defaultPlayerSpeed + (3 / 2);
            }
            if (_score >= 40 && _score < 50)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 4;
                _playerSpeed = _defaultPlayerSpeed + (4 / 2);
            }
            if (_score >= 50 && _score < 80)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 5;
                _playerSpeed = _defaultPlayerSpeed + (5 / 2);
            }
            if (_score >= 80 && _score < 100)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 6;
                _playerSpeed = _defaultPlayerSpeed + (6 / 2);
            }
            if (_score >= 100 && _score < 130)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 7;
                _playerSpeed = _defaultPlayerSpeed + (7 / 2);
            }
            if (_score >= 130 && _score < 150)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 8;
                _playerSpeed = _defaultPlayerSpeed + (8 / 2);
            }
            if (_score >= 150 && _score < 180)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 9;
                _playerSpeed = _defaultPlayerSpeed + (9 / 2);
            }
            if (_score >= 180 && _score < 200)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 10;
                _playerSpeed = _defaultPlayerSpeed + (10 / 2);
            }
            if (_score >= 200 && _score < 220)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 11;
                _playerSpeed = _defaultPlayerSpeed + (11 / 2);
            }
            if (_score >= 220 && _score < 250)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 12;
                _playerSpeed = _defaultPlayerSpeed + (12 / 2);
            }
            if (_score >= 250 && _score < 300)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 13;
                _playerSpeed = _defaultPlayerSpeed + (13 / 2);
            }
            if (_score >= 300 && _score < 350)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 14;
                _playerSpeed = _defaultPlayerSpeed + (14 / 2);
            }
            if (_score >= 350 && _score < 400)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 15;
                _playerSpeed = _defaultPlayerSpeed + (15 / 2);
            }
            if (_score >= 400 && _score < 500)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 16;
                _playerSpeed = _defaultPlayerSpeed + (16 / 2);
            }
            if (_score >= 500 && _score < 600)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 17;
                _playerSpeed = _defaultPlayerSpeed + (17 / 2);
            }
            if (_score >= 600 && _score < 700)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 18;
                _playerSpeed = _defaultPlayerSpeed + (18 / 2);
            }
            if (_score >= 700 && _score < 800)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 19;
                _playerSpeed = _defaultPlayerSpeed + (19 / 2);
            }
            if (_score >= 800 && _score < 900)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 20;
                _playerSpeed = _defaultPlayerSpeed + (20 / 2);
            }
            if (_score >= 900 && _score < 1000)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 21;
                _playerSpeed = _defaultPlayerSpeed + (21 / 2);
            }
            if (_score >= 1000 && _score < 1200)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 22;
                _playerSpeed = _defaultPlayerSpeed + (22 / 2);
            }
            if (_score >= 1200 && _score < 1400)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 23;
                _playerSpeed = _defaultPlayerSpeed + (23 / 2);
            }
            if (_score >= 1400 && _score < 1600)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 24;
                _playerSpeed = _defaultPlayerSpeed + (24 / 2);
            }
            if (_score >= 1600 && _score < 1800)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 25;
                _playerSpeed = _defaultPlayerSpeed + (25 / 2);
            }
            if (_score >= 1800 && _score < 2000)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 26;
                _playerSpeed = _defaultPlayerSpeed + (26 / 2);
            }
            if (_score >= 2000 && _score < 2200)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 27;
                _playerSpeed = _defaultPlayerSpeed + (27 / 2);
            }
            if (_score >= 2200 && _score < 2400)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 28;
                _playerSpeed = _defaultPlayerSpeed + (28 / 2);
            }
            if (_score >= 2400 && _score < 2600)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 29;
                _playerSpeed = _defaultPlayerSpeed + (29 / 2);
            }
            if (_score >= 2600 && _score < 2800)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 30;
                _playerSpeed = _defaultPlayerSpeed + (30 / 2);
            }
            if (_score >= 2800 && _score < 3000)
            {
                _gameSpeed = _defaultGameSpeed + 1 * 31;
                _playerSpeed = _defaultPlayerSpeed + (31 / 2);
            }
        }

        #endregion

        #region Sound

        private async void StartGameSounds()
        {
            SoundHelper.PlaySound(SoundType.CAR_START);

            await Task.Delay(TimeSpan.FromSeconds(1));

            SoundHelper.PlaySound(SoundType.CAR_ENGINE);

            SoundHelper.RandomizeBackgroundSound();
            SoundHelper.PlaySound(SoundType.BACKGROUND);
        }

        private void StopGameSounds()
        {
            SoundHelper.StopSound(SoundType.BACKGROUND);
            SoundHelper.StopSound(SoundType.CAR_ENGINE);
        }

        private void PauseGameSounds()
        {
            SoundHelper.PauseSound(SoundType.BACKGROUND);
            SoundHelper.PauseSound(SoundType.CAR_ENGINE);
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

        #endregion

        #region Contextual

        //        #region Fields

        //        private PeriodicTimer _gameViewTimer;
        //        private readonly TimeSpan _frameTime = TimeSpan.FromMilliseconds(Constants.DEFAULT_FRAME_TIME);

        //        private readonly Random _random = new();

        //        private bool _isGameOver;
        //        private bool _isPointerActivated;

        //        private double _windowHeight, _windowWidth;
        //        private double _scale;
        //        private Point _pointerPosition;

        //        int apples, score, level;

        //        #endregion

        //        #region Properties

        //        public double ElementSize { get; set; }

        //        public Collectible Collectible { get; set; }

        //        public Snake Snake { get; set; }

        //        #endregion

        //        #region Ctor

        //        public GamePage()
        //        {
        //            InitializeComponent();

        //            _isGameOver = true;
        //            ShowInGameTextMessage("TAP_ON_SCREEN_TO_BEGIN");

        //            SoundHelper.LoadGameSounds();

        //            _windowHeight = Window.Current.Bounds.Height;
        //            _windowWidth = Window.Current.Bounds.Width;

        //            Loaded += GamePage_Loaded;
        //            Unloaded += GamePage_Unloaded;
        //        }

        //        #endregion

        //        #region Events

        //        #region Page

        //        private void GamePage_Loaded(object sender, RoutedEventArgs e)
        //        {
        //            SizeChanged += GamePage_SizeChanged;
        //        }

        //        private void GamePage_Unloaded(object sender, RoutedEventArgs e)
        //        {
        //            SizeChanged -= GamePage_SizeChanged;
        //            StopGame();
        //        }

        //        private void GamePage_SizeChanged(object sender, SizeChangedEventArgs args)
        //        {
        //            _windowWidth = args.NewSize.Width;
        //            _windowHeight = args.NewSize.Height;

        //            SetViewSize();

        //            Console.WriteLine($"WINDOWS SIZE: {_windowWidth}x{_windowHeight}");
        //        }

        //        #endregion

        //        #region Input

        //        private void InputView_PointerPressed(object sender, PointerRoutedEventArgs e)
        //        {
        //            if (_isGameOver)
        //            {
        //                InputView.Focus(FocusState.Programmatic);
        //                StartGame();
        //            }
        //            else
        //            {
        //                _isPointerActivated = true;
        //            }
        //        }

        //        private void InputView_PointerMoved(object sender, PointerRoutedEventArgs e)
        //        {
        //            if (_isPointerActivated)
        //            {
        //                PointerPoint point = e.GetCurrentPoint(GameView);
        //                _pointerPosition = point.Position;
        //            }
        //        }

        //        private void InputView_PointerReleased(object sender, PointerRoutedEventArgs e)
        //        {
        //            _isPointerActivated = false;
        //            _pointerPosition = null;
        //        }

        //        private void OnKeyUP(object sender, KeyRoutedEventArgs e)
        //        {
        //            switch (e.Key)
        //            {
        //                case VirtualKey.Left:
        //                    UpdateMovementDirection(MovementDirection.Left);
        //                    break;
        //                case VirtualKey.Up:
        //                    UpdateMovementDirection(MovementDirection.Up);
        //                    break;
        //                case VirtualKey.Right:
        //                    UpdateMovementDirection(MovementDirection.Right);
        //                    break;
        //                case VirtualKey.Down:
        //                    UpdateMovementDirection(MovementDirection.Down);
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }

        //        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        //        {

        //        }

        //        #endregion

        //        #region Button

        //        private void QuitGameButton_Checked(object sender, RoutedEventArgs e)
        //        {
        //            PauseGame();
        //        }

        //        private void QuitGameButton_Unchecked(object sender, RoutedEventArgs e)
        //        {
        //            ResumeGame();
        //        }

        //        private void ConfirmQuitGameButton_Click(object sender, RoutedEventArgs e)
        //        {
        //            //NavigateToPage(typeof(StartPage));
        //        }

        //        #endregion

        //        #endregion

        //        #region Methods

        //        #region Animation

        //        #region Game

        //        private void StartGame()
        //        {
        //#if DEBUG
        //            Console.WriteLine("GAME STARTED");
        //#endif
        //            HideInGameTextMessage();
        //            //SoundHelper.PlaySound(SoundType.MENU_SELECT);

        //            ResetControls();

        //            _isGameOver = false;

        //            score = 0;
        //            scoreText.Text = "0";

        //            InitializeSnake();
        //            StartGameSounds();
        //            RunGame();
        //        }

        //        private void InitializeSnake()
        //        {
        //            Snake = new Snake(ElementSize);
        //            Snake.PositionFirstElement(_random.Next(100, (int)_windowWidth), _random.Next(100, (int)_windowHeight), MovementDirection.Right);
        //        }

        //        private void RemoveGameObjects()
        //        {
        //            GameView.RemoveDestroyableGameObjects();
        //        }

        //        private async void RunGame()
        //        {
        //            _gameViewTimer = new PeriodicTimer(_frameTime);

        //            while (await _gameViewTimer.WaitForNextTickAsync())
        //            {
        //                GameViewLoop();
        //            }
        //        }

        //        private void GameViewLoop()
        //        {
        //            Snake.MoveSnake();
        //            CheckCollision();
        //            CreateApple();
        //            Draw();
        //        }

        //        private void Draw()
        //        {
        //            DrawSnake();
        //            DrawApple();
        //        }

        //        private void DrawSnake()
        //        {
        //            foreach (var PlayerTrail in Snake.Elements)
        //            {
        //                if (!GameView.Children.Contains(PlayerTrail))
        //                    GameView.Children.Add(PlayerTrail);

        //                Canvas.SetLeft(PlayerTrail, PlayerTrail.X);
        //                Canvas.SetTop(PlayerTrail, PlayerTrail.Y);
        //                Canvas.SetZIndex(PlayerTrail, 1);

        //                if (PlayerTrail.IsHead)
        //                    Canvas.SetZIndex(PlayerTrail, Snake.Elements.Count + 1);
        //            }
        //        }

        //        private void DrawApple()
        //        {
        //            if (!GameView.Children.Contains(Collectible))
        //                GameView.Children.Add(Collectible);

        //            Canvas.SetLeft(Collectible, Collectible.X);
        //            Canvas.SetTop(Collectible, Collectible.Y);
        //        }

        //        private void CheckCollision()
        //        {
        //            if (CollisionWithApple())
        //                ProcessCollisionWithApple();

        //            if (Snake.CollisionWithSelf() || CollisionWithWorldBounds())
        //                StopGame();
        //        }

        //        private void ProcessCollisionWithApple()
        //        {
        //            Console.WriteLine("APPLE REMOVED");

        //            IncrementScore();
        //            GameView.Children.Remove(Collectible);
        //            Collectible = null;
        //            Snake.Grow();
        //            //IncreaseGameSpeed();
        //        }

        //        private void IncreaseGameSpeed()
        //        {
        //            //_gameSpeed++;
        //        }

        //        internal void IncrementScore()
        //        {
        //            apples += 1;
        //            if (apples % 3 == 0)
        //                level += 1;
        //            score += 10;
        //            UpdateScore();
        //        }

        //        internal void UpdateScore()
        //        {
        //            //ApplesLbl.Content = $"Apples: {apples}";
        //            scoreText.Text = $"{score}";
        //            //LevelLbl.Content = $"Level: {level}";
        //        }

        //        private void CreateApple()
        //        {
        //            if (Collectible != null)
        //                return;

        //            Collectible = new Collectible(ElementSize)
        //            {
        //                X = _random.Next(100, (int)_windowWidth - 100),
        //                Y = _random.Next(100, (int)_windowHeight - 100),
        //            };
        //        }

        //        private bool CollisionWithApple()
        //        {
        //            if (Collectible == null || Snake == null || Snake.Head == null)
        //                return false;

        //            PlayerTrail source = Snake.Head;
        //            var target = Collectible;

        //            //return (head.X == Collectible.X && head.Y == Collectible.Y);

        //            if (source.Width >= 0.0 && source.Width >= 0.0
        //               && target.X <= source.X + source.Width && target.X + target.Width >= source.X
        //               && target.Y <= source.Y + source.Height)
        //            {
        //                return target.Y + target.Height >= source.Y;
        //            }

        //            return false;
        //        }

        //        private bool CollisionWithWorldBounds()
        //        {
        //            if (Snake == null || Snake.Head == null)
        //                return false;

        //            var snakeHead = Snake.Head;

        //            //return (snakeHead.X > _windowWidth - ElementSize ||
        //            //    snakeHead.Y > _windowHeight - ElementSize ||
        //            //    snakeHead.X < 0 || snakeHead.Y < 0);

        //            if (snakeHead.X > _windowWidth)
        //            {
        //                snakeHead.X = 0;
        //            }

        //            if (snakeHead.X < 0)
        //            {
        //                snakeHead.X = _windowWidth;
        //            }

        //            if (snakeHead.Y > _windowHeight)
        //            {
        //                snakeHead.Y = 0;
        //            }

        //            if (snakeHead.Y < 0)
        //            {
        //                snakeHead.Y = _windowHeight;
        //            }

        //            return false;
        //        }

        //        private void PauseGame()
        //        {
        //            InputView.Focus(FocusState.Programmatic);
        //            ShowInGameTextMessage("GAME_PAUSED");

        //            _gameViewTimer?.Dispose();

        //            ResetControls();

        //            //SoundHelper.PlaySound(SoundType.MENU_SELECT);
        //            PauseGameSounds();
        //        }

        //        private void ResumeGame()
        //        {
        //            InputView.Focus(FocusState.Programmatic);
        //            HideInGameTextMessage();

        //            //SoundHelper.PlaySound(SoundType.MENU_SELECT);
        //            //SoundHelper.ResumeSound(SoundType.BACKGROUND);
        //            //SoundHelper.ResumeSound(SoundType.CAR_ENGINE);

        //            RunGame();
        //        }

        //        private void StopGame()
        //        {
        //            _gameViewTimer?.Dispose();
        //            StopGameSounds();
        //        }

        //        private void ResetControls()
        //        {
        //            _isPointerActivated = false;
        //        }

        //        internal void UpdateMovementDirection(MovementDirection movementDirection)
        //        {
        //            if (Snake != null)
        //                Snake.UpdateMovementDirection(movementDirection);
        //        }

        //        #endregion

        //        #endregion

        //        #region Sound

        //        private async void StartGameSounds()
        //        {
        //            //SoundHelper.PlaySound(SoundType.CAR_START);

        //            //await Task.Delay(TimeSpan.FromSeconds(1));

        //            //SoundHelper.PlaySound(SoundType.CAR_ENGINE);

        //            //SoundHelper.RandomizeBackgroundSound();
        //            //SoundHelper.PlaySound(SoundType.BACKGROUND);
        //        }

        //        private void StopGameSounds()
        //        {
        //            //SoundHelper.StopSound(SoundType.BACKGROUND);
        //            //SoundHelper.StopSound(SoundType.CAR_ENGINE);
        //        }

        //        private void PauseGameSounds()
        //        {
        //            //SoundHelper.PauseSound(SoundType.BACKGROUND);
        //            //SoundHelper.PauseSound(SoundType.CAR_ENGINE);
        //        }

        //        #endregion

        //        #region Page

        //        private void SetViewSize()
        //        {
        //            _scale = ScalingHelper.GetGameObjectScale(_windowWidth);

        //            GameView.Width = _windowWidth;
        //            GameView.Height = _windowHeight;

        //            ElementSize = Constants.PLAYER_SIZE * _scale;
        //        }

        //        private void NavigateToPage(Type pageType)
        //        {
        //            //SoundHelper.PlaySound(SoundType.MENU_SELECT);
        //            App.NavigateToPage(pageType);
        //        }

        //        #endregion

        //        #region In Game Message

        //        private void ShowInGameTextMessage(string resourceKey)
        //        {
        //            InGameMessageText.Text = LocalizationHelper.GetLocalizedResource(resourceKey);
        //            InGameMessagePanel.Visibility = Visibility.Visible;
        //        }

        //        private void HideInGameTextMessage()
        //        {
        //            InGameMessageText.Text = "";
        //            InGameMessagePanel.Visibility = Visibility.Collapsed;
        //        }

        //        #endregion

        //        #endregion

        #endregion

        #region Functional

        //#region Fields

        //private bool _isGameOver;
        //private bool _isPointerActivated;

        //// This list describes the Bonus Red pieces of Food on the Canvas
        //private readonly List<Point> _applePoints = new List<Point>();

        //// This list describes the body of the snake on the Canvas
        //private readonly List<Point> _snakePoints = new List<Point>();

        //private readonly Brush _snakeColor = new SolidColorBrush(Colors.Green);

        //private readonly Point _startingPoint = new Point(100, 100);
        //private Point _currentPosition = new Point();

        //// Movement direction initialisation
        //private int _direction = 0;

        ///* Placeholder for the previous movement direction
        // * The snake needs this to avoid its own body.  */
        //private int _previousDirection = 0;

        ///* Here user can change the size of the snake. 
        // * Possible sizes are THIN, NORMAL and THICK */
        //private readonly int _headSize = (int)SnakeSize.Thick;

        //private int _length = 100;
        //private int _score = 0;
        //private readonly Random _rnd = new Random();

        //PeriodicTimer _timer;
        //private double _windowHeight, _windowWidth;
        //private double _scale;

        //private Point _pointerPosition;
        //private double _gameSpeed = 4;

        //#endregion

        //#region Ctor

        //public GamePage()
        //{
        //    InitializeComponent();

        //    _isGameOver = true;

        //    _windowHeight = Window.Current.Bounds.Height;
        //    _windowWidth = Window.Current.Bounds.Width;

        //    Loaded += GamePage_Loaded;
        //    Unloaded += GamePage_Unloaded;
        //}

        //#endregion

        //#region Events

        //private void GamePage_Loaded(object sender, RoutedEventArgs e)
        //{
        //    SizeChanged += GamePage_SizeChanged;
        //}

        //private void GamePage_Unloaded(object sender, RoutedEventArgs e)
        //{
        //    SizeChanged -= GamePage_SizeChanged;
        //    StopGame();
        //}

        //private void GamePage_SizeChanged(object sender, SizeChangedEventArgs args)
        //{
        //    _windowWidth = args.NewSize.Width;
        //    _windowHeight = args.NewSize.Height;

        //    SetViewSize();

        //    Console.WriteLine($"WINDOWS SIZE: {_windowWidth}x{_windowHeight}");
        //}

        //private void OnKeyUP(object sender, KeyRoutedEventArgs e)
        //{
        //    Console.WriteLine("KEY DOWN");
        //    switch (e.Key)
        //    {
        //        case VirtualKey.Down:
        //            if (_previousDirection != (int)Movingdirection.Upwards)
        //                _direction = (int)Movingdirection.Downwards;
        //            break;
        //        case VirtualKey.Up:
        //            if (_previousDirection != (int)Movingdirection.Downwards)
        //                _direction = (int)Movingdirection.Upwards;
        //            break;
        //        case VirtualKey.Left:
        //            if (_previousDirection != (int)Movingdirection.Toright)
        //                _direction = (int)Movingdirection.Toleft;
        //            break;
        //        case VirtualKey.Right:
        //            if (_previousDirection != (int)Movingdirection.Toleft)
        //                _direction = (int)Movingdirection.Toright;
        //            break;

        //    }

        //    _previousDirection = _direction;
        //}

        //private void InputView_PointerPressed(object sender, PointerRoutedEventArgs e)
        //{
        //    if (_isGameOver)
        //    {
        //        InputView.Focus(FocusState.Programmatic);
        //        StartGame();
        //    }
        //    else
        //    {
        //        _isPointerActivated = true;
        //    }
        //}

        //private void InputView_PointerMoved(object sender, PointerRoutedEventArgs e)
        //{
        //    if (_isPointerActivated)
        //    {
        //        PointerPoint point = e.GetCurrentPoint(GameView);
        //        _pointerPosition = point.Position;
        //    }
        //}

        //private void InputView_PointerReleased(object sender, PointerRoutedEventArgs e)
        //{
        //    _isPointerActivated = false;
        //    _pointerPosition = null;
        //}

        //#endregion

        //#region Methods

        //private void SetViewSize()
        //{
        //    _scale = ScalingHelper.GetGameObjectScale(_windowWidth);

        //    GameView.Width = _windowWidth;
        //    GameView.Height = _windowHeight;
        //}

        //private async void StartGame()
        //{
        //    Console.WriteLine("START GAME");
        //    _isGameOver = false;

        //    PaintSnake(_startingPoint);
        //    _currentPosition = _startingPoint;

        //    if (_applePoints.Count == 0)
        //    {
        //        // Instantiate Food Objects
        //        for (var n = 0; n < 10; n++)
        //        {
        //            PaintApple(n);
        //        }
        //    }

        //    _timer = new PeriodicTimer(TimeSpan.FromMilliseconds((double)GameSpeed.Moderate));

        //    while (await _timer.WaitForNextTickAsync())
        //    {
        //        GameLoop();
        //    }

        //    //_timer.Tick += Timer_Tick;

        //    ///* Here user can change the speed of the snake. 
        //    // * Possible speeds are FAST, MODERATE, SLOW and DAMNSLOW */
        //    //_timer.Interval = TimeSpan.FromMilliseconds((double)GameSpeed.Moderate);
        //    //_timer.Start();
        //}

        //private void GameLoop(/*object sender, object e*/)
        //{
        //    // Expand the body of the snake to the direction of movement

        //    switch (_direction)
        //    {
        //        case (int)Movingdirection.Downwards:
        //            _currentPosition.Y += _gameSpeed;
        //            PaintSnake(_currentPosition);
        //            break;
        //        case (int)Movingdirection.Upwards:
        //            _currentPosition.Y -= _gameSpeed;
        //            PaintSnake(_currentPosition);
        //            break;
        //        case (int)Movingdirection.Toleft:
        //            _currentPosition.X -= _gameSpeed;
        //            PaintSnake(_currentPosition);
        //            break;
        //        case (int)Movingdirection.Toright:
        //            _currentPosition.X += _gameSpeed;
        //            PaintSnake(_currentPosition);
        //            break;
        //    }

        //    // Restrict to boundaries of the Canvas
        //    if ((_currentPosition.X < 0) || (_currentPosition.X > _windowWidth) ||
        //        (_currentPosition.Y < 0) || (_currentPosition.Y > _windowHeight))
        //        StopGame();

        //    // Hitting a bonus Point causes the lengthen-Snake Effect
        //    int n = 0;
        //    foreach (Point point in _applePoints)
        //    {
        //        if ((Math.Abs(point.X - _currentPosition.X) < _headSize) &&
        //            (Math.Abs(point.Y - _currentPosition.Y) < _headSize))
        //        {
        //            _length += 10;
        //            _score += 10;

        //            scoreText.Text = _score.ToString();

        //            // In the case of food consumption, erase the food object
        //            // from the list of bonuses as well as from the canvas
        //            _applePoints.RemoveAt(n);
        //            GameView.Children.RemoveAt(n);
        //            PaintApple(n);
        //            break;
        //        }
        //        n++;
        //    }

        //    // Restrict hits to body of Snake
        //    for (int q = 0; q < (_snakePoints.Count - _headSize * 2); q++)
        //    {
        //        Point point = new Point(_snakePoints[q].X, _snakePoints[q].Y);
        //        if ((Math.Abs(point.X - _currentPosition.X) < (_headSize)) &&
        //             (Math.Abs(point.Y - _currentPosition.Y) < (_headSize)))
        //        {
        //            Console.WriteLine("BODY PART COLLISION");
        //            StopGame();
        //            break;
        //        }
        //    }
        //}

        //private void PaintSnake(Point currentposition)
        //{
        //    // This method is used to paint a frame of the snake´s body each time it is called.

        //    PlayerTrail newEllipse = new(_headSize);

        //    Canvas.SetTop(newEllipse, currentposition.Y);
        //    Canvas.SetLeft(newEllipse, currentposition.X);

        //    int count = GameView.Children.Count;
        //    GameView.Children.Add(newEllipse);
        //    _snakePoints.Add(currentposition);

        //    // Restrict the tail of the snake
        //    if (count > _length)
        //    {
        //        GameView.Children.RemoveAt(count - _length + 9);
        //        _snakePoints.RemoveAt(count - _length);
        //    }
        //}

        //private void PaintApple(int index)
        //{
        //    Point bonusPoint = new Point(_rnd.Next(5, (int)_windowWidth), _rnd.Next(5, (int)_windowHeight));

        //    Border newEllipse = new()
        //    {
        //        Background = new SolidColorBrush(Colors.Crimson),
        //        CornerRadius = new Microsoft.UI.Xaml.CornerRadius(50),
        //        Width = _headSize,
        //        Height = _headSize
        //    };

        //    Canvas.SetTop(newEllipse, bonusPoint.Y);
        //    Canvas.SetLeft(newEllipse, bonusPoint.X);

        //    GameView.Children.Insert(index, newEllipse);
        //    _applePoints.Insert(index, bonusPoint);
        //}

        //private void StopGame()
        //{
        //    //_timer.Stop();
        //    _timer.Dispose();

        //    Console.WriteLine("GAME OVER");

        //    //MessageBox.Show($@"You Lose! Your score is {_score}", "Game Over", MessageBoxButton.OK, MessageBoxImage.Hand);
        //    //this.Close();
        //}

        //#endregion

        #endregion
    }
}
