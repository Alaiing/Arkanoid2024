using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Oudidon;
using System;

namespace Arkanoid2023
{

    public class Arkanoid2024 : OudidonGame
    {
        public const int PLAYGROUND_MIN_X = 42;
        public const int PLAYGROUND_MAX_X = 148;
        public const int PLAYGROUND_MIN_Y = 38;
        public const int PLAYGROUND_MAX_Y = 251;
        public const int DEFAULT_STICK_X = 8;
        public const int GRID_WIDTH = 13;
        public const int GRID_HEIGHT = 18;


        private SpriteSheet _shipSprite;
        private SpaceShip _ship;

        private SpriteSheet _ballSprite;
        private Ball _ball;

        private Texture2D _frame;
        private Texture2D _background_1;
        private Texture2D _life;
        private Texture2D _basicBrick;

        private Level _level1;

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            EventsManager.ListenTo("BallOut", OnBallOut);
            EventsManager.ListenTo("GameOver", OnGameOver);

            StartGame();
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            _frame = Content.Load<Texture2D>("frame");
            _background_1 = Content.Load<Texture2D>("background1");
            _life = Content.Load<Texture2D>("life");
            _basicBrick = Content.Load<Texture2D>("basic_brick");

            _shipSprite = new SpriteSheet(Content, "spaceship", 18, 6, Point.Zero);
            _shipSprite.RegisterAnimation("Idle", 0, 0, 1f);
            _ship = new SpaceShip(_shipSprite, this);
            _ship.SetAnimation("Idle");
            Components.Add(_ship);

            _ballSprite = new SpriteSheet(Content, "ball", 4, 4, Point.Zero);
            _ballSprite.RegisterAnimation("Idle", 0, 0, 1f);
            _ball = new Ball(_ballSprite, _ship, this);
            _ball.SetAnimation("Idle");
            Components.Add(_ball);
            
            _level1 = new Level("level1.data", _basicBrick, this);
            Components.Add(_level1);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime); // Updates state machine and components, in that order

            if (!_ball.IsStuck)
            {
                int previousX = (int)MathF.Floor(_ball.PreviousPosition.X - 44) / 8;
                int previousY = (int)MathF.Floor(_ball.PreviousPosition.Y - 38) / 8;
                int x = (_ball.PixelPositionX - 44) / 8;
                int y = (_ball.PixelPositionY - 38) / 8;
                if (x < GRID_WIDTH && y < GRID_HEIGHT)
                {
                    Level.Brick brick = _level1.GetBrick(x, y);
                    if (brick != null)
                    {
                        if (x - previousX != 0)
                        {
                            _ball.MoveDirection = new Vector2(-_ball.MoveDirection.X, _ball.MoveDirection.Y);
                        }

                        if (y - previousY != 0)
                        {
                            _ball.MoveDirection = new Vector2(_ball.MoveDirection.X, -_ball.MoveDirection.Y);
                        }
                    }
                }
            }
        }

        protected override void DrawGameplay(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            DrawBackround();
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
            DrawLives();
        }

        private void DrawLives()
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
            for (int i=0; i < _ship.LivesLeft; i++)
            {
                _spriteBatch.Draw(_life, new Vector2(42 + _life.Width * i, 261), Color.White);
            }
            _spriteBatch.End();
        }

        private void StartGame()
        {
            _ship.Reset();
            _ball.StickToSpaceShip(DEFAULT_STICK_X);
        }

        private void OnBallOut()
        {
            _ball.StickToSpaceShip(DEFAULT_STICK_X);
            _ship.LoseLife();
        }

        private void OnGameOver()
        {
            StartGame();
        }

    }
}
