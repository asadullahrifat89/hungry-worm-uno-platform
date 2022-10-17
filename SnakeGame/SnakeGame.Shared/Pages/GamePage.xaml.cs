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
using Uno.Extensions;
using Windows.Foundation;
using Windows.System;


namespace SnakeGame
{
    public sealed partial class GamePage : Page
    {
        #region First

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

        //        public int ElementSize { get; set; } = 80;

        //        public Apple Apple { get; set; }

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
        //            foreach (var snakeElement in Snake.Elements)
        //            {
        //                if (!GameView.Children.Contains(snakeElement))
        //                    GameView.Children.Add(snakeElement);

        //                Canvas.SetLeft(snakeElement, snakeElement.X);
        //                Canvas.SetTop(snakeElement, snakeElement.Y);
        //                Canvas.SetZIndex(snakeElement, 1);

        //                if (snakeElement.IsHead)
        //                    Canvas.SetZIndex(snakeElement, Snake.Elements.Count + 1);
        //            }
        //        }

        //        private void DrawApple()
        //        {
        //            if (!GameView.Children.Contains(Apple))
        //                GameView.Children.Add(Apple);

        //            Canvas.SetLeft(Apple, Apple.X);
        //            Canvas.SetTop(Apple, Apple.Y);
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
        //            GameView.Children.Remove(Apple);
        //            Apple = null;
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
        //            score += 10 * level;
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
        //            if (Apple != null)
        //                return;

        //            Apple = new Apple(ElementSize)
        //            {
        //                X = _random.Next(100, (int)_windowWidth - 100),
        //                Y = _random.Next(100, (int)_windowHeight - 100),
        //            };
        //        }

        //        private bool CollisionWithApple()
        //        {
        //            if (Apple == null || Snake == null || Snake.Head == null)
        //                return false;

        //            SnakeElement source = Snake.Head;
        //            var target = Apple;

        //            //return (head.X == Apple.X && head.Y == Apple.Y);

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

        #region Second

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

        //    SnakeElement newEllipse = new(_headSize);

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

        #region Third

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

        private bool _moveLeft;
        private bool _moveRight;
        private bool _moveUp;
        private bool _moveDown;
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
        private List<SnakeElement> _playerTrails;
        private int _length;

        #endregion

        #region Ctor

        public GamePage()
        {
            InitializeComponent();

            _isGameOver = true;
            ShowInGameTextMessage("TAP_ON_SCREEN_TO_BEGIN");

            _windowHeight = Window.Current.Bounds.Height;
            _windowWidth = Window.Current.Bounds.Width;

            SoundHelper.LoadGameSounds();
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
                InputView.Focus(FocusState.Programmatic);
                StartGame();
            }
            else
            {
                _isPointerActivated = true;

                PointerPoint point = e.GetCurrentPoint(GameView);
                _pointerPosition = point.Position;

                _moveRight = false;
                _moveLeft = false;
                _moveDown = false;
                _moveUp = false;
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
            //_pointerPosition = null;
        }

        private int _isMoveUp;
        private int _isMoveDown;
        private int _isMoveLeft;
        private int _isMoveRight;

        private void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Left:
                    {
                        _moveLeft = true;
                        _moveRight = false;

                        //_isMoveLeft = 2;
                        //_isMoveUp--;
                        //_isMoveDown--;

                        //if (_isMoveUp <= 0)
                        //    _moveUp = false;
                        //if (_isMoveDown <= 0)
                        //    _moveDown = false;
                    }
                    break;
                case VirtualKey.Up:
                    {
                        _moveUp = true;
                        _moveDown = false;

                        //_isMoveUp = 2;
                        //_isMoveLeft--;
                        //_isMoveRight--;

                        //if (_isMoveLeft <= 0)
                        //    _moveLeft = false;
                        //if (_isMoveRight <= 0)
                        //    _moveRight = false;
                    }
                    break;
                case VirtualKey.Right:
                    {
                        _moveRight = true;
                        _moveLeft = false;

                        //_isMoveRight = 2;
                        //_isMoveUp--;
                        //_isMoveDown--;

                        //if (_isMoveUp <= 0)
                        //    _moveUp = false;
                        //if (_isMoveDown <= 0)
                        //    _moveDown = false;
                    }
                    break;
                case VirtualKey.Down:
                    {
                        _moveDown = true;
                        _moveUp = false;

                        //_isMoveDown = 2;
                        //_isMoveLeft--;
                        //_isMoveRight--;

                        //if (_isMoveLeft <= 0)
                        //    _moveLeft = false;
                        //if (_isMoveRight <= 0)
                        //    _moveRight = false;
                    }
                    break;

                default:
                    break;
            }
        }

        private void OnKeyUP(object sender, KeyRoutedEventArgs e)
        {
            // when the player releases the left or right key it will set the designated boolean to false
            //if (e.Key == VirtualKey.Left)
            //{
            //    _moveLeft = false;
            //}
            //if (e.Key == VirtualKey.Right)
            //{
            //    _moveRight = false;
            //}
            //if (e.Key == VirtualKey.Up)
            //{
            //    _moveUp = false;
            //}
            //if (e.Key == VirtualKey.Down)
            //{
            //    _moveDown = false;
            //}

            //if (!_moveLeft && !_moveRight && !_moveUp && !_moveDown)
            //    _accelerationCounter = 0;

            // in this case we will listen for the enter key aswell but for this to execute we will need the game over boolean to be true
            //if (e.Key == VirtualKey.Enter && _isGameOver == true)
            //{
            //    StartGame();
            //}
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

            //PopulateUnderView();
            PopulateGameView();
            //PopulateOverView();
        }

        private void PopulateUnderView()
        {
            // add some cars underneath
            for (int i = 0; i < 10; i++)
            {
                //var car = new Car()
                //{
                //    Width = Constants.CAR_WIDTH * _scale,
                //    Height = Constants.CAR_HEIGHT * _scale,
                //    IsCollidable = false,
                //    RenderTransform = new CompositeTransform()
                //    {
                //        ScaleX = 0.5,
                //        ScaleY = 0.5,
                //    }
                //};

                //RandomizeCarPosition(car);
                //UnderView.Children.Add(car);
            }

            // add some clouds underneath
            for (int i = 0; i < 15; i++)
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
                //UnderView.Children.Add(cloud);
            }
        }

        private void PopulateGameView()
        {
            //// add 5 cars
            //for (int i = 0; i < 5; i++)
            //{
            //    var car = new Car()
            //    {
            //        Width = Constants.CAR_WIDTH * _scale,
            //        Height = Constants.CAR_HEIGHT * _scale,
            //        IsCollidable = true,
            //    };

            //    RandomizeCarPosition(car);
            //    GameView.Children.Add(car);
            //}

            _playerTrails = new List<SnakeElement>();

            // add player
            _player = new Player()
            {
                Width = Constants.PLAYER_WIDTH * _scale,
                Height = Constants.PLAYER_HEIGHT * _scale,
            };

            _player.SetPosition(
                left: GameView.Width / 2 - _player.Width / 2,
                top: GameView.Height - _player.Height - (50 * _scale));

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
            //_cars = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.CAR).Select(x => x.Value).ToArray();
            //_islands = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.ISLAND).Select(x => x.Value).ToArray();
            //_clouds = Constants.ELEMENT_TEMPLATES.Where(x => x.Key == ElementType.CLOUD).Select(x => x.Value).ToArray();
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

            //foreach (GameObject x in SeaView.Children.OfType<GameObject>())
            //{
            //    SeaView.AddDestroyableGameObject(x);
            //}

            RecycleGameObjects();
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

        private void RecycleGameObjects()
        {

        }

        private void ResetControls()
        {
            _moveLeft = false;
            _moveRight = false;
            _moveUp = false;
            _moveDown = false;
            _isPointerActivated = false;
        }

        private void GameViewLoop()
        {
            //AddScore(0.05d); // increase the score by .5 each tick of the timer
            //scoreText.Text = _score.ToString("#");

            _playerHitBox = _player.GetHitBox(_scale);

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

        }

        private void UpdateGameObjects()
        {
            foreach (GameObject x in GameView.Children.OfType<GameObject>())
            {
                switch ((ElementType)x.Tag)
                {
                    case ElementType.PLAYER:
                        {
                            //if (_moveLeft || _moveRight || _moveUp || _moveDown || _isPointerActivated)
                            //{
                            UpdatePlayer();
                            //}
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

            //PlayerScoreHelper.PlayerScore = new SkyRacerGameScore()
            //{
            //    Score = Math.Ceiling(_score),
            //    CollectiblesCollected = _collectiblesCollected
            //};

            //SoundHelper.PlaySound(SoundType.GAME_OVER);
            //NavigateToPage(typeof(GameOverPage));
        }

        private double DecreaseSpeed(double speed)
        {
            //if (_isPowerMode && _powerUpType == PowerUpType.SLOW_DOWN_TIME)
            //    speed /= 3;

            return speed;
        }

        #region Player

        private void UpdatePlayer()
        {
            double effectiveSpeed = _accelerationCounter >= _playerSpeed ? _playerSpeed : _accelerationCounter / 1.3;

            // increase acceleration and stop when player speed is reached
            if (_accelerationCounter <= _playerSpeed)
                _accelerationCounter++;

            double left = _player.GetLeft();
            double top = _player.GetTop();

            double playerMiddleX = left + _player.Width / 2;
            double playerMiddleY = top + _player.Height / 2;

            if (_isPointerActivated)
            {
                // move up
                if (_pointerPosition.Y < playerMiddleY - _playerSpeed)
                    _player.SetTop(top - effectiveSpeed);

                // move left
                if (_pointerPosition.X < playerMiddleX - _playerSpeed && left > 0)
                    _player.SetLeft(left - effectiveSpeed);

                // move down
                if (_pointerPosition.Y > playerMiddleY + _playerSpeed)
                    _player.SetTop(top + effectiveSpeed);

                // move right
                if (_pointerPosition.X > playerMiddleX + _playerSpeed && left + _player.Width < GameView.Width)
                    _player.SetLeft(left + effectiveSpeed);
            }
            else
            {
                if (_moveLeft && left > 0)
                    _player.SetLeft(left - effectiveSpeed);

                if (_moveRight && left + _player.Width < GameView.Width)
                    _player.SetLeft(left + effectiveSpeed);

                if (_moveUp && top > 0 + (50 * _scale))
                    _player.SetTop(top - effectiveSpeed);

                if (_moveDown && top < GameView.Height - (100 * _scale))
                    _player.SetTop(top + effectiveSpeed);

            }

            //if (GameView.Children.OfType<SnakeElement>().Count() == 20)
            //{
            //    GameView.Children.RemoveAt(20);
            //}

            SnakeElement playerTrail = new(_player.Height);
            playerTrail.SetTop(top);
            playerTrail.SetLeft(left);

            GameView.Children.Add(playerTrail);
            _length++;

            if (_length > 50)
            {
                GameView.Children.Remove(GameView.Children.OfType<SnakeElement>().First());
                _length--;
            }
        }

        #endregion

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

            //UnderView.Width = _windowWidth;
            //UnderView.Height = _windowHeight;

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
    }

    //public enum SnakeSize
    //{
    //    Thin = 15,
    //    Normal = 25,
    //    Thick = 50
    //};

    //public enum Movingdirection
    //{
    //    Upwards = 8,
    //    Downwards = 2,
    //    Toleft = 4,
    //    Toright = 6
    //};

    ////TimeSpan values
    //public enum GameSpeed
    //{
    //    Fast = 10,
    //    Moderate = 18,
    //    Slow = 50,
    //    DamnSlow = 100
    //};
}
