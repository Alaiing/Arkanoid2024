using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Oudidon;
using System;
using System.Collections.Generic;

namespace Arkanoid2024
{

    public class Arkanoid2024 : OudidonGame
    {
        public const int PLAYGROUND_MIN_X = 42;
        public const int PLAYGROUND_MAX_X = 150;
        public const int PLAYGROUND_MIN_Y = 38;
        public const int PLAYGROUND_MAX_Y = 251;
        public const int DEFAULT_STICK_X = 8;
        public const int GRID_WIDTH = 13;
        public const int GRID_HEIGHT = 18;

        private const string STATE_TITLE = "Title";
        private const string STATE_LEVEL_START = "LevelStart";
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
        private Ball _ball;

        private Texture2D _frame;
        private SpriteSheet _hatch;
        private Character[] _hatches;
        private Texture2D _background_1;
        private Texture2D _life;
        private Texture2D _logo;
        private Texture2D _levelStart;

        private SpriteSheet _basicBrick;
        private SpriteSheet _silverBrick;
        private SpriteSheet _goldenBrick;

        private SpriteSheet _enemyHat;

        private List<Level> _levels = new();
        private int _currentLevelIndex;
        private int _currentBrickCount;

        private SoundEffect _ping;
        private SoundEffectInstance _pingInstance;

        private SoundEffect _pong;
        private SoundEffectInstance _pongInstance;

        private SoundEffect _pang;
        private SoundEffectInstance _pangInstance;

        public const bool CheatInfiniteLives = false;
        public const bool CheatNoBallOut = false;
        public const int CheatStartingLevel = 0;

        private List<Enemy> _enemies = new List<Enemy>();

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            EventsManager.ListenTo("BallOut", OnBallOut);
            EventsManager.ListenTo("Ping", OnPing);
            EventsManager.ListenTo<Point>("BrickHit", OnBrickHit);
            EventsManager.ListenTo<Brick>("BrickDestroyed", OnBrickDestroyed);

            _stateMachine.AddState(STATE_TITLE, OnEnter: TitleEnter, OnUpdate: TitleUpdate, OnDraw: TitleDraw);
            _stateMachine.AddState(STATE_LEVEL_START, OnUpdate: LevelStartUpdate, OnDraw: LevelStartDraw);
            _stateMachine.AddState(STATE_GAME, OnEnter: GameEnter, OnExit: GameExit, OnUpdate: GameUpdate, OnDraw: GameDraw);
            _stateMachine.AddState(STATE_GAME_OVER);

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

            _shipSprite = new SpriteSheet(Content, "spaceship", 18, 6, Point.Zero);
            _shipSprite.RegisterAnimation("Idle", 0, 0, 1f);
            _ship = new SpaceShip(_shipSprite, this);
            _ship.SetAnimation("Idle");
            Components.Add(_ship);
            _ship.Deactivate();

            _ballSprite = new SpriteSheet(Content, "ball", 4, 4, new Point(2, 2));
            _ballSprite.RegisterAnimation("Idle", 0, 0, 1f);
            _ball = new Ball(_ballSprite, _ship, this);
            _ball.SetAnimation("Idle");
            Components.Add(_ball);
            _ball.Deactivate();

            _basicBrick = new SpriteSheet(Content, "basic_brick", 8, 8, Point.Zero);
            _basicBrick.RegisterAnimation("Idle", 0, 0, 1f);
            _basicBrick.RegisterAnimation("Hit", 0, 0, 1f);

            _silverBrick = new SpriteSheet(Content, "silver_brick", 8, 8, Point.Zero);
            _silverBrick.RegisterAnimation("Idle", 0, 0, 1f);
            _silverBrick.RegisterAnimation("Hit", 0, 2, 20f);

            _goldenBrick = new SpriteSheet(Content, "silver_brick", 8, 8, Point.Zero);
            _goldenBrick.RegisterAnimation("Idle", 0, 0, 1f);

            _enemyHat = new SpriteSheet(Content, "hat", 9, 14, new Point(4,0));
            _enemyHat.RegisterAnimation("Idle", 0, 12, 10f);

            _levels.Add(new Level("test_level.data", _basicBrick, _silverBrick, _goldenBrick, this));
            _levels.Add(new Level("level2.data", _basicBrick, _silverBrick, _goldenBrick, this));

            _ping = Content.Load<SoundEffect>("ping");
            _pingInstance = _ping.CreateInstance();
            _pong = Content.Load<SoundEffect>("pong");
            _pongInstance = _pong.CreateInstance();
            _pang = Content.Load<SoundEffect>("pang");
            _pangInstance = _pang.CreateInstance();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime); // Updates state machine and components, in that order

            if (!_ball.IsStuck && _ball.Enabled)
            {
                _ball.TestBrickCollision(_levels[_currentLevelIndex]);
            }
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
            for (int i = 0; i < _ship.LivesLeft; i++)
            {
                _spriteBatch.Draw(_life, new Vector2(42 + _life.Width * i, 261), Color.White);
            }
        }

        private void SetCurrentLevel(int levelIndex)
        {
            _currentLevelIndex = levelIndex;
            _levels[_currentLevelIndex].Reset();
        }

        #region Events
        private void OnBallOut()
        {
            _ball.StickToSpaceShip(DEFAULT_STICK_X);
            _ship.LoseLife();
            if (_ship.LivesLeft >= 0)
            {
                SetState(STATE_LEVEL_START);
            }
            else
            {
                SetState(STATE_TITLE);
            }
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

        private void TitleUpdate(GameTime time, float arg2)
        {
            SimpleControls.GetStates();
            if (SimpleControls.IsADown(PlayerIndex.One))
            {
                SetState(STATE_LEVEL_START);
                SetCurrentLevel(CheatStartingLevel);
            }
        }

        private void LevelStartDraw(SpriteBatch batch, GameTime time)
        {
            batch.Draw(_levelStart, new Vector2(68, 136), Color.White);
        }

        private void LevelStartUpdate(GameTime time, float stateTime)
        {
            if (stateTime > 1f)
            {
                SetState(STATE_GAME);
            }
        }

        private void GameEnter()
        {
            _levels[_currentLevelIndex].Activate();
            _ship.Activate();
            _ship.ResetPosition();
            _ball.Activate();
            _ball.Reset();
            _ball.StickToSpaceShip(DEFAULT_STICK_X);
            _hatches[0].Activate();
            _hatches[1].Activate();
            _enemySpawnCooldown = 0;
        }

        private void GameExit()
        {
            _ship.Deactivate();
            _ball.Deactivate();
            _levels[_currentLevelIndex].DeActivate();
            _hatches[0].Deactivate();
            _hatches[1].Deactivate();
            RemoveAllEnemies();
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
        }

        private void GameDraw(SpriteBatch batch, GameTime time)
        {
            DrawBackround();
            DrawLives();
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
            foreach(Enemy enemy in _enemies) 
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
