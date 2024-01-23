using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Arkanoid2024
{

    public class Arkanoid2024 : OudidonGame
    {
        public const int PLAYGROUND_MIN_X = 42;
        public const int PLAYGROUND_MAX_X = 150;
        public const int PLAYGROUND_MIN_Y = 38;
        public const int PLAYGROUND_MAX_Y = 251;
        public const int DEFAULT_STICK_X = 1;
        public const int GRID_WIDTH = 13;
        public const int GRID_HEIGHT = 18;

        private const string STATE_TITLE = "Title";
        private const string STATE_LEVEL_START = "LevelStart";
        private const string STATE_BALL_OUT = "BallOut";
        private const string STATE_GAME = "Game";
        private const string STATE_GAME_OVER = "GameOver";

        private readonly Vector2[] _spawnPositions = new Vector2[]
        {
            new Vector2(36 + 32, 27 + 10),
            new Vector2(36 + 92, 27 + 10)
        };

        private SpriteSheet _shipSprite;
        private SpaceShip _ship;

        private SpriteSheet _ballSprite;
        private List<Ball> _balls = new();

        private Texture2D _frame;
        private SpriteSheet _hatch;
        private Character[] _hatches;
        private Texture2D _background_1;
        private Texture2D _life;
        private Texture2D _logo;
        private Texture2D _levelStart;
        private Texture2D _gameOver;

        private SpriteSheet _basicBrick;
        private SpriteSheet _silverBrick;
        private SpriteSheet _goldenBrick;

        private SpriteSheet _enemyHat;

        private SpriteSheet _bonusSheet;

        private List<Level> _levels = new();
        private int _currentLevelIndex;
        private int _currentBrickCount;

        private SoundEffect _ping;
        private SoundEffectInstance _pingInstance;

        private SoundEffect _pong;
        private SoundEffectInstance _pongInstance;

        private SoundEffect _pang;
        private SoundEffectInstance _pangInstance;

        private SoundEffect _ballOut;
        private SoundEffectInstance _ballOutInstance;

        private SoundEffect _enemyDie;
        private SoundEffectInstance _enemyDieInstance;

        private SoundEffect _gameLaunchJingle;
        private SoundEffectInstance _gameLaunchJingleInstance;

        private SoundEffect _levelStartJingle;
        private SoundEffectInstance _levelStartJingleInstance;
        
        private SoundEffect _gameOverJingle;
        private SoundEffectInstance _gameOverJingleInstance;

        public const bool CheatInfiniteLives = false;
        public const bool CheatNoBallOut = false;
        public const int CheatStartingLevel = 0;

        private List<Enemy> _enemies = new List<Enemy>();
        private SpriteSheet _explosionSheet;
        private Stack<Character> _explosionsStack = new();

        private int _bonusStack;
        private List<Type> _bonusTypes = new();


        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            EventsManager.ListenTo<Ball>("BallOut", OnBallOut);
            EventsManager.ListenTo("Ping", OnPing);
            EventsManager.ListenTo<Point>("BrickHit", OnBrickHit);
            EventsManager.ListenTo<Brick>("BrickDestroyed", OnBrickDestroyed);
            EventsManager.ListenTo<Enemy>("EnemyHit", OnEnemyHit);
            EventsManager.ListenTo("Multibaaaaall", OnDisrupt);

            _stateMachine.AddState(STATE_TITLE, OnEnter: TitleEnter, OnUpdate: TitleUpdate, OnDraw: TitleDraw);
            _stateMachine.AddState(STATE_LEVEL_START, OnEnter: LevelStartEnter, OnUpdate: LevelStartUpdate, OnDraw: LevelStartDraw);
            _stateMachine.AddState(STATE_GAME, OnEnter: GameEnter, OnUpdate: GameUpdate, OnDraw: GameDraw);
            _stateMachine.AddState(STATE_BALL_OUT, OnEnter: BallOutEnter, OnExit: BallOutExit, OnUpdate: BallOutUpdate, OnDraw: GameDraw);
            _stateMachine.AddState(STATE_GAME_OVER, OnEnter: GameOverEnter, OnUpdate: GameOverUpdate, OnDraw: GameOverDraw);

            _currentLevelIndex = CheatStartingLevel;
            SetState(STATE_TITLE);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _frame = Content.Load<Texture2D>("frame");
            _background_1 = Content.Load<Texture2D>("background1");
            _life = Content.Load<Texture2D>("life");
            _logo = Content.Load<Texture2D>("logo");
            _levelStart = Content.Load<Texture2D>("level_start");
            _gameOver = Content.Load<Texture2D>("game_over");

            _hatches = new Character[2];
            _hatch = new SpriteSheet(Content, "hatch", 20, 9, Point.Zero);
            _hatch.RegisterAnimation("Idle", 0, 0, 1f);
            _hatch.RegisterAnimation("Open", 0, 2, 8f);
            _hatches[0] = new Character(_hatch, this);
            _hatches[0].SetAnimation("Idle");
            _hatches[0].MoveTo(new Vector2(36 + 22, 27));
            Components.Add(_hatches[0]);
            _hatches[0].Deactivate();

            _hatches[1] = new Character(_hatch, this);
            _hatches[1].SetAnimation("Idle");
            _hatches[1].MoveTo(new Vector2(36 + 82, 27));
            Components.Add(_hatches[1]);
            _hatches[1].Deactivate();

            SpriteSheet laserBlast = new SpriteSheet(Content, "laser_blast", 14, 6, Point.Zero);
            laserBlast.RegisterAnimation("Idle", 0, 0, 1f);
            _shipSprite = new SpriteSheet(Content, "spaceship", 22, 8, new Point(11, 0));
            _shipSprite.RegisterAnimation(SpaceShip.DEFAULT, 0, 0, 1f);
            _shipSprite.RegisterAnimation(SpaceShip.EXTENDED, 1, 1, 1f);
            _shipSprite.RegisterAnimation(SpaceShip.LASER, 2, 2, 1f);
            _ship = new SpaceShip(_shipSprite, laserBlast, this);
            Components.Add(_ship);
            _ship.Deactivate();

            _ballSprite = new SpriteSheet(Content, "ball", 4, 4, new Point(2, 2));
            _ballSprite.RegisterAnimation("Idle", 0, 0, 1f);
            AddBall();
            _balls[0].Deactivate();

            _basicBrick = new SpriteSheet(Content, "basic_brick", 8, 8, Point.Zero);
            _basicBrick.RegisterAnimation("Idle", 0, 0, 1f);
            _basicBrick.RegisterAnimation("Hit", 0, 0, 1f);

            _silverBrick = new SpriteSheet(Content, "silver_brick", 8, 8, Point.Zero);
            _silverBrick.RegisterAnimation("Idle", 0, 0, 1f);
            _silverBrick.RegisterAnimation("Hit", 0, 2, 20f);

            _goldenBrick = new SpriteSheet(Content, "silver_brick", 8, 8, Point.Zero);
            _goldenBrick.RegisterAnimation("Idle", 0, 0, 1f);

            _enemyHat = new SpriteSheet(Content, "hat", 9, 14, new Point(4, 0));
            _enemyHat.RegisterAnimation("Idle", 0, 12, 10f);

            _bonusSheet = new SpriteSheet(Content, "bonuses", 8, 7, Point.Zero);
            _bonusSheet.RegisterAnimation(SlowBonus.ANIMATION_NAME, 0, 3, 4f);
            _bonusTypes.Add(typeof(SlowBonus));
            _bonusSheet.RegisterAnimation(CatchBonus.ANIMATION_NAME, 4, 7, 4f);
            _bonusTypes.Add(typeof(CatchBonus));
            _bonusSheet.RegisterAnimation(ExtendedBonus.ANIMATION_NAME, 8, 11, 4f);
            _bonusTypes.Add(typeof(ExtendedBonus));
            _bonusSheet.RegisterAnimation(DisruptBonus.ANIMATION_NAME, 12, 15, 4f);
            _bonusTypes.Add(typeof(DisruptBonus));
            _bonusSheet.RegisterAnimation(LaserBonus.ANIMATION_NAME, 16, 19, 4f);
            _bonusTypes.Add(typeof(LaserBonus));
            _bonusSheet.RegisterAnimation(ExtraLifeBonus.ANIMATION_NAME, 24, 27, 4f);
            _bonusTypes.Add(typeof(ExtraLifeBonus));
            _bonusSheet.RegisterAnimation("Breakout", 20, 23, 4f);

            _levels.Add(new Level("level1.data", _basicBrick, _silverBrick, _goldenBrick, this));
            _levels.Add(new Level("level2.data", _basicBrick, _silverBrick, _goldenBrick, this));

            _explosionSheet = new SpriteSheet(Content, "explosion", 12, 14, Point.Zero);
            _explosionSheet.RegisterAnimation("Explode", 0, 4, 10f);


            _ping = Content.Load<SoundEffect>("ping");
            _pingInstance = _ping.CreateInstance();
            _pong = Content.Load<SoundEffect>("pong");
            _pongInstance = _pong.CreateInstance();
            _pang = Content.Load<SoundEffect>("pang");
            _pangInstance = _pang.CreateInstance();

            _ballOut = Content.Load<SoundEffect>("tchoutchoutchou");
            _ballOutInstance = _ballOut.CreateInstance();

            _enemyDie = Content.Load<SoundEffect>("zboui");
            _enemyDieInstance = _enemyDie.CreateInstance();

            _gameLaunchJingle = Content.Load<SoundEffect>("game_start");
            _gameLaunchJingleInstance = _gameLaunchJingle.CreateInstance();

            _levelStartJingle = Content.Load<SoundEffect>("level_start_jingle");
            _levelStartJingleInstance = _levelStartJingle.CreateInstance();

            _gameOverJingle = Content.Load<SoundEffect>("game_over_jingle");
            _gameOverJingleInstance = _gameOverJingle.CreateInstance();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime); // Updates state machine and components, in that order
        }

        protected override void DrawGameplay(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            base.DrawGameplay(gameTime); // Draws state machine and components, in that order
            _spriteBatch.End();
        }

        protected void DrawBackround()
        {
            _spriteBatch.Draw(_background_1, new Vector2(36, 27), Color.White);
            _spriteBatch.Draw(_frame, new Vector2(36, 27), Color.White);
        }

        protected override void DrawUI(GameTime gameTime)
        {
            // TODO: Draw your overlay UI here
        }

        private void DrawLives()
        {
            for (int i = 0; i < Math.Min(10, _ship.LivesLeft); i++)
            {
                _spriteBatch.Draw(_life, new Vector2(42 + _life.Width * i, 261), Color.White);
            }
        }

        private void SetCurrentLevel(int levelIndex)
        {
            _currentLevelIndex = levelIndex;
            _levels[_currentLevelIndex].Reset();
        }

        private void SpawnExplosion(Vector2 position)
        {
            Character explosion;
            if (_explosionsStack.Count > 0)
            {
                explosion = _explosionsStack.Pop();
            }
            else
            {
                explosion = new Character(_explosionSheet, this);
                explosion.Deactivate();
                explosion.SetAnimation("Explode", onAnimationEnd: () => DisposeExplosion(explosion));
                Components.Add(explosion);
            }

            explosion.SetFrame(0);
            explosion.Activate();
            explosion.MoveTo(position);
        }

        private void DisposeExplosion(Character explosion)
        {
            explosion.Deactivate();
            _explosionsStack.Push(explosion);
        }

        private void AddBall()
        {
            Ball newBall = new Ball(_ballSprite, _ship, this);
            _balls.Add(newBall);
            Components.Add(newBall);
        }

        private void ClearBalls()
        {
            if (_balls.Count > 1)
            {
                _balls.RemoveRange(1, _balls.Count);
            }
        }

        public static bool TestBrick(Level level, Vector2 position, out Point gridPosition)
        {
            int testedGridX = (int)MathF.Floor(position.X - PLAYGROUND_MIN_X - 2) / 8;
            int testedGridY = (int)MathF.Floor(position.Y - PLAYGROUND_MIN_Y) / 8;
            gridPosition = new Point(testedGridX, testedGridY);

            if (testedGridX < GRID_WIDTH && testedGridY < GRID_HEIGHT)
            {
                Brick brick = level.GetBrick(testedGridX, testedGridY);
                if (brick != null && brick.Visible)
                {
                    return true;
                }
            }
            return false;
        }


        #region Events
        private void OnBallOut(Ball ball)
        {
            if (_balls.Count > 1)
            {
                ball.Deactivate();
                Components.Remove(ball);
                _balls.Remove(ball);
                ball.Dispose();
                return;
            }

            _ship.LoseLife();
            SetState(STATE_BALL_OUT);
        }

        private void OnPing()
        {
            _pingInstance.Play();
        }

        private void OnBrickHit(Point position)
        {
            Brick brick = _levels[_currentLevelIndex].GetBrick(position.X, position.Y);
            brick.Hit();
            _pangInstance.Stop();
            _pongInstance.Stop();

            if (brick.IsDestroyed)
            {
                _pongInstance.Play();
            }
            else
            {
                _pangInstance.Play();
            }
        }

        private void OnBrickDestroyed(Brick brick)
        {
            _currentBrickCount++;
            if (_currentBrickCount == _levels[_currentLevelIndex].BrickCount)
            {
                if (_currentLevelIndex < _levels.Count - 1)
                {
                    _levels[_currentLevelIndex].DeActivate();
                    SetCurrentLevel(_currentLevelIndex + 1);
                    _currentBrickCount = 0;
                    SetState(STATE_LEVEL_START);
                }
                else
                {
                    // TODO: end game
                }
                return;
            }

            if (brick.IsRegular && _balls.Count < 2 && Bonus.CurrentFallingBonus == null)
            {
                _bonusStack += CommonRandom.Random.Next(1, 9);
                if (_bonusStack >= 8)
                {
                    SpawnBonus(brick);
                    _bonusStack = 0;
                }
            }
        }

        private void SpawnBonus(Brick brick)
        {
            int index = CommonRandom.Random.Next(0, _bonusTypes.Count);
            ConstructorInfo ctor = _bonusTypes[index].GetConstructor(new Type[] { typeof(Vector2), typeof(SpriteSheet), typeof(Game)});
            ctor.Invoke(new object[] { brick.Position + new Vector2(0, brick.SpriteSheet.BottomMargin), _bonusSheet, this} );
        }

        private void OnEnemyHit(Enemy enemy)
        {
            SpawnExplosion(enemy.Position - enemy.SpriteSheet.DefaultPivot.ToVector2());
            _enemies.Remove(enemy);
            enemy.Kill();

            _enemyDieInstance.Stop();
            _enemyDieInstance.Play();
        }

        private void OnDisrupt()
        {
            AddBall();
            AddBall();

            _balls[0].SetSpeedY(1);
            for (int i = 1; i < _balls.Count; i++)
            {
                _balls[i].MoveTo(_balls[0].Position);
                _balls[i].MoveDirection = _balls[0].MoveDirection;
                _balls[i].SetSpeedY(i + 1);
            }
        }

        #endregion

        #region States
        private void TitleEnter()
        {
            _currentLevelIndex = CheatStartingLevel;
            _currentBrickCount = 0;
            _ship.Reset();
        }

        private void TitleDraw(SpriteBatch batch, GameTime time)
        {
            batch.Draw(_logo, new Vector2(32, 11), Color.White);
        }

        private bool _gameStarting;
        private void TitleUpdate(GameTime time, float arg2)
        {
            SimpleControls.GetStates();
            if (!_gameStarting)
            {
                if (SimpleControls.IsAPressedThisFrame(PlayerIndex.One))
                {
                    _gameLaunchJingleInstance.Play();
                    _gameStarting = true;
                }
            }
            else
            {
                if (_gameLaunchJingleInstance.State != SoundState.Playing)
                {
                    SetState(STATE_LEVEL_START);
                    SetCurrentLevel(CheatStartingLevel);
                    _gameStarting = false;
                }
            }
        }

        private void LevelStartEnter()
        {
            _levelStartJingleInstance.Play();
        }

        private void LevelStartDraw(SpriteBatch batch, GameTime time)
        {
            batch.Draw(_levelStart, new Vector2(68, 136), Color.White);
        }

        private void LevelStartUpdate(GameTime time, float stateTime)
        {
            if (stateTime > _levelStartJingle.Duration.TotalSeconds)
            {
                SetState(STATE_GAME);
            }
        }

        private void GameEnter()
        {
            _levels[_currentLevelIndex].Activate();
            _ship.Activate();
            _ship.ResetPosition();
            _ship.SetType(SpaceShip.SpaceShipType.Default);
            _balls[0].Activate();
            _balls[0].Reset();
            _balls[0].StickToSpaceShip(DEFAULT_STICK_X);
            _hatches[0].Activate();
            _hatches[1].Activate();
            _enemySpawnCooldown = 0;
            _bonusStack = 0;
        }

        private float _enemySpawnCooldown;
        private void GameUpdate(GameTime gameTime, float stateTime)
        {
            if (_enemies.Count < 3)
            {
                _enemySpawnCooldown += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_enemySpawnCooldown >= 2f)
                {
                    int hatchSide = CommonRandom.Random.Next(0, 2);
                    SpawnEnemy(hatchSide);
                    _enemySpawnCooldown = 0;
                }
            }

            if (_balls.Count > 1 || !_balls[0].IsStuck && _balls[0].Enabled)
            {
                foreach (var ball in _balls)
                {
                    ball.TestBrickCollision(_levels[_currentLevelIndex]);
                    ball.TestEnemiesCollision(_enemies);
                    ball.TestBonusCollision();
                }
            }

            for (int i = 0; i < _enemies.Count; i++)
            {
                if (TestCollision(_ship, _enemies[i]))
                {
                    EventsManager.FireEvent("EnemyHit", _enemies[i]);
                }
            }

            if (Bonus.CurrentFallingBonus != null)
            {
                if (TestCollision(_ship, Bonus.CurrentFallingBonus))
                {
                    Bonus.CurrentFallingBonus.Collect(_ship, _balls[0]);
                }
            }

            LaserBlast.TestCollisions(_levels[_currentLevelIndex], _enemies);
        }

        private void GameDraw(SpriteBatch batch, GameTime time)
        {
            DrawBackround();
            DrawLives();
        }

        private void BallOutEnter()
        {
            _ballOutInstance.Play();
            _ship.Enabled = false;
            foreach (var ball in _balls)
            {
                ball.Enabled = false;
            }
            _hatches[0].Enabled = false;
            _hatches[1].Enabled = false;
            if (Bonus.CurrentFallingBonus != null)
                Bonus.CurrentFallingBonus.Enabled = false;
            foreach(Enemy enemy in _enemies)
            {
                enemy.Enabled = false;
            }
        }

        private void BallOutExit()
        {
            _ship.Deactivate();
            ClearBalls();
            _balls[0].Deactivate();
            _levels[_currentLevelIndex].DeActivate();
            _hatches[0].Deactivate();
            _hatches[1].Deactivate();
            RemoveAllEnemies();
            Bonus.ClearBonuses();
        }

        private void BallOutUpdate(GameTime gameTime, float stateTime)
        {
            if (stateTime >= _ballOut.Duration.TotalSeconds)
            {
                if (_ship.LivesLeft >= 0)
                {
                    SetState(STATE_LEVEL_START);
                }
                else
                {
                    SetState(STATE_GAME_OVER);
                }
            }
        }

        private void GameOverEnter()
        {
            _gameOverJingleInstance.Play();
        }

        private void GameOverUpdate(GameTime gameTime, float stateTime)
        {
            if (stateTime > _gameOverJingle.Duration.TotalSeconds)
            {
                SetState(STATE_TITLE);
            }
        }

        private void GameOverDraw(SpriteBatch batch, GameTime time)
        {
            batch.Draw(_gameOver, new Vector2(68, 144), Color.White);
        }



        #endregion

        #region Enemies
        private void SpawnEnemy(int hatchIndex)
        {
            Vector2 position = _spawnPositions[hatchIndex];
            Enemy newEnemy = new Enemy(_enemyHat, _levels[_currentLevelIndex], _enemies, this);
            newEnemy.MoveTo(position);
            newEnemy.SetAnimation("Idle");
            _enemies.Add(newEnemy);
            Components.Add(newEnemy);

            _hatches[hatchIndex].SetAnimation("Open", () => _hatches[hatchIndex].SetAnimation("Idle"));
        }

        private void RemoveAllEnemies()
        {
            foreach (Enemy enemy in _enemies)
            {
                Components.Remove(enemy);
                enemy.Dispose();
            }
            _enemies.Clear();
        }

        public static bool TestCollision(Character character, Character other)
        {
            return OverlapsWith(character.GetBounds(), other.GetBounds());
        }

        public static bool OverlapsWith(Rectangle first, Rectangle second)
        {
            return !(first.Bottom < second.Top
                    || first.Right < second.Left
                    || first.Top > second.Bottom
                    || first.Left > second.Right);
        }
        #endregion
    }
}
