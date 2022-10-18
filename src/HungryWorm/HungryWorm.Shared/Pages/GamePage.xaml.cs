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

        private Rect _playerHitBox;

        private double _gameSpeed = 6;
        private readonly double _defaultGameSpeed = 6;

        private int _playerSpeed = 6;
        private int _defaultPlayerSpeed = 6;
        private int _markNum;

        private int _powerUpSpawnCounter = 30;

        private int _powerModeCounter = 500;
        private readonly int _powerModeDelay = 500;

        private int _lives;
        private readonly int _maxLives = 3;
        private int _healthSpawnCounter = 500;
        private int _damageRecoveryOpacityFrameSkip;

        private int _foodSpawnCounter = 200;

        private double _score;
        private int _foodCollected;

        private bool _isGameOver;
        private bool _isPowerMode;

        private bool _isPointerActivated;

        private double _windowHeight, _windowWidth;
        private double _scale;
        private Point _pointerPosition;

        //private PowerUpType _powerUpType;

        private Player _player;
        private int _length;
        private int _maxLength;
        private int _foodSpawnLimit;
        private int _foodCount;

        private Uri[] _playerTemplates;
        private Uri[] _collectibleTemplates;
        private int _yummyFaceCounter;

        private double _health;
        private int _healthDepleteCounter;
        private double _healthDepletePoint;
        private double _healthGainPoint;
        private readonly double _defaultHealthDepletePoint = 0.5;
        private readonly double _defaultHealthGainPoint = 10;

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
            _isPointerActivated = false;

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
            // add some cars underneath
            for (int i = 0; i < 35; i++)
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
            // add player
            _player = new Player(Constants.PLAYER_SIZE * _scale);

            _player.SetPosition(
                left: GameView.Width / 2 - _player.Width / 2,
                top: GameView.Height / 2 - _player.Height / 2);

            _player.SetZ(1);

            GameView.Children.Add(_player);

            // add 5 collectibles
            for (int i = 0; i < 5; i++)
            {
                SpawnCollectible(); // on game init
            }
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

            _maxLength = 40;
            _foodSpawnLimit = 10;
            _foodCount = 0;

            _gameSpeed = _defaultGameSpeed;
            _playerSpeed = _defaultPlayerSpeed;
            _player.Opacity = 1;

            ResetControls();

            _isGameOver = false;
            _isPowerMode = false;
            //_powerUpType = 0;
            _powerModeCounter = _powerModeDelay;

            _score = 0;
            _foodCollected = 0;
            ScoreText.Text = "0";

            PlayerHealthBarPanel.Visibility = Visibility.Visible;

            _healthDepletePoint = _defaultHealthDepletePoint;
            _health = 100;
            _healthGainPoint = _defaultHealthGainPoint;
            _healthDepleteCounter = 10;

            RemoveGameObjects();
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
            ScoreText.Text = _score.ToString("#");
            PlayerHealthBar.Value = _health;

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

                if (_foodSpawnCounter < 1)
                {
                    SpawnCollectible();
                    _foodSpawnCounter = _random.Next(30, 80);
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
            _yummyFaceCounter = 50;
            _player.SetContent(_playerTemplates[_random.Next(0, _playerTemplates.Length)]);
        }

        private void YummyFaceCoolDown()
        {
            if (_yummyFaceCounter > 0)
            {
                _yummyFaceCounter--;

                if (_yummyFaceCounter <= 0)
                    _player.SetContent(_playerTemplates.First());
            }
        }

        #endregion

        #region PlayerTrail

        private void SpawnPlayerTrail()
        {
            double left = _player.GetLeft();
            double top = _player.GetTop();

            PlayerTrail playerTrail = new(Constants.PLAYER_TRAIL_SIZE * _scale);
            playerTrail.SetPosition(left, top);
            playerTrail.SetZ(0);
            playerTrail.UpdateMovementDirection(_player.MovementDirection);

            GameView.Children.Add(playerTrail);
            _length++;
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

            var playerTrails = GameView.GetGameObjects<PlayerTrail>().ToArray();

            if (_length > _maxLength)
            {
                GameView.AddDestroyableGameObject(playerTrails[0]);
                _length--;
            }

            // give tail a proper border
            var tail = playerTrails[1];
            tail.BorderThickness = new Thickness(5);

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
            collectible.SetPosition(_random.Next(0, (int)_windowWidth), _random.Next(0, (int)_windowHeight));

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
            AddHealth(_healthGainPoint);

            _foodCount--;
            _foodCollected++;

            SetYummyFace();
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
            if (_health < 100)
            {
                if (_health + health > 100)
                    health = _health + health - 100;

                _health += health;
            }
        }

        private void DepleteHealth()
        {
            _healthDepleteCounter--;

            if (_healthDepleteCounter <= 0)
            {
                _health -= _healthDepletePoint;
                _healthDepleteCounter = 10;
            }

            if (_health <= 0)
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
            //_maxLength += 1;
            _score += score;
        }

        #endregion

        #region Difficulty

        private void ScaleDifficulty()
        {
            if (_score >= 10 && _score < 20)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 1;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 1;
                _gameSpeed = _defaultGameSpeed + 0.2 * 1;
            }
            if (_score >= 20 && _score < 30)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 2;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 2;
                _gameSpeed = _defaultGameSpeed + 0.2 * 2;
            }
            if (_score >= 30 && _score < 40)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 3;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 3;
                _gameSpeed = _defaultGameSpeed + 0.2 * 3;
            }
            if (_score >= 40 && _score < 50)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 4;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 4;
                _gameSpeed = _defaultGameSpeed + 0.2 * 4;
            }
            if (_score >= 50 && _score < 80)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 5;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 5;
                _gameSpeed = _defaultGameSpeed + 0.2 * 5;
            }
            if (_score >= 80 && _score < 100)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 6;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 6;
                _gameSpeed = _defaultGameSpeed + 0.2 * 6;
            }
            if (_score >= 100 && _score < 130)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 7;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 7;
                _gameSpeed = _defaultGameSpeed + 0.2 * 7;
            }
            if (_score >= 130 && _score < 150)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 8;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 8;
                _gameSpeed = _defaultGameSpeed + 0.2 * 8;
            }
            if (_score >= 150 && _score < 180)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 9;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 9;
                _gameSpeed = _defaultGameSpeed + 0.2 * 9;
            }
            if (_score >= 180 && _score < 200)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 10;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 10;
                _gameSpeed = _defaultGameSpeed + 0.2 * 10;
            }
            if (_score >= 200 && _score < 220)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 11;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 11;
                _gameSpeed = _defaultGameSpeed + 0.2 * 11;
            }
            if (_score >= 220 && _score < 250)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 12;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 12;
                _gameSpeed = _defaultGameSpeed + 0.2 * 12;
            }
            if (_score >= 250 && _score < 300)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 13;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 13;
                _gameSpeed = _defaultGameSpeed + 0.2 * 13;
            }
            if (_score >= 300 && _score < 350)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 14;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 14;
                _gameSpeed = _defaultGameSpeed + 0.2 * 14;
            }
            if (_score >= 350 && _score < 400)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 15;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 15;
                _gameSpeed = _defaultGameSpeed + 0.2 * 15;
            }
            if (_score >= 400 && _score < 500)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 16;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 16;
                _gameSpeed = _defaultGameSpeed + 0.2 * 16;
            }
            if (_score >= 500 && _score < 600)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 17;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 17;
                _gameSpeed = _defaultGameSpeed + 0.2 * 17;
            }
            if (_score >= 600 && _score < 700)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 18;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 18;
                _gameSpeed = _defaultGameSpeed + 0.2 * 18;
            }
            if (_score >= 700 && _score < 800)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 19;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 19;
                _gameSpeed = _defaultGameSpeed + 0.2 * 19;
            }
            if (_score >= 800 && _score < 900)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 20;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 20;
                _gameSpeed = _defaultGameSpeed + 0.2 * 20;
            }
            if (_score >= 900 && _score < 1000)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 21;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 21;
                _gameSpeed = _defaultGameSpeed + 0.2 * 21;
            }
            if (_score >= 1000 && _score < 1200)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 22;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 22;
                _gameSpeed = _defaultGameSpeed + 0.2 * 22;
            }
            if (_score >= 1200 && _score < 1400)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 23;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 23;
                _gameSpeed = _defaultGameSpeed + 0.2 * 23;
            }
            if (_score >= 1400 && _score < 1600)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 24;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 24;
                _gameSpeed = _defaultGameSpeed + 0.2 * 24;
            }
            if (_score >= 1600 && _score < 1800)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 25;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 25;
                _gameSpeed = _defaultGameSpeed + 0.2 * 25;
            }
            if (_score >= 1800 && _score < 2000)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 26;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 26;
                _gameSpeed = _defaultGameSpeed + 0.2 * 26;
            }
            if (_score >= 2000 && _score < 2200)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 27;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 27;
                _gameSpeed = _defaultGameSpeed + 0.2 * 27;
            }
            if (_score >= 2200 && _score < 2400)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 28;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 28;
                _gameSpeed = _defaultGameSpeed + 0.2 * 28;
            }
            if (_score >= 2400 && _score < 2600)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 29;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 29;
                _gameSpeed = _defaultGameSpeed + 0.2 * 29;
            }
            if (_score >= 2600 && _score < 2800)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 30;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 30;
                _gameSpeed = _defaultGameSpeed + 0.2 * 30;
            }
            if (_score >= 2800 && _score < 3000)
            {
                _healthGainPoint = _defaultHealthGainPoint + 0.1 * 31;
                _healthDepletePoint = _defaultHealthDepletePoint + 0.1 * 31;
                _gameSpeed = _defaultGameSpeed + 0.2 * 31;
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

    #region Experimental

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
    //            ScoreText.Text = "0";

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
    //            ScoreText.Text = $"{score}";
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

    //            ScoreText.Text = _score.ToString();

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

    #endregion
}
