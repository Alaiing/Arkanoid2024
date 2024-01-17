using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid2024
{
    public class Ball : Character
    {
        private SpaceShip _spaceShip;
        private bool _stuck;
        public bool IsStuck => _stuck;
        private float _stuckX;

        private Vector2 _previousPosition;
        public Vector2 PreviousPosition => _previousPosition;

        private int _speedX;
        private int _speedY;

        private int _defaultSpeedY;

        public Ball(SpriteSheet spriteSheet, SpaceShip spaceShip, Game game) : base(spriteSheet, game)
        {
            _spaceShip = spaceShip;
            SetBaseSpeed(50f);
            _speedX = 1;
            _defaultSpeedY = _speedY = ConfigManager.GetConfig("BALL_DEFAULT_SPEED_Y", 3);
            DrawOrder = 1;
            SetAnimation("Idle");
        }

        public override void Reset()
        {
            base.Reset();
            MoveTo(new Vector2(96, 177));
            _speedY = _defaultSpeedY;
            MoveDirection = new Vector2(_speedX, -_speedY);
            SetSpeedMultiplier(1f);
        }

        private float _stuckTimer;
        public void StickToSpaceShip(int offset)
        {
            _stuck = true;
            _stuckX = offset;
            _stuckTimer = 0;
            SetSpeedMultiplier(0);
        }

        public void Unstick()
        {
            _stuck = false;
            MoveDirection = new Vector2((_stuckX + _spriteSheet.FrameWidth / 2 < _spaceShip.SpriteSheet.FrameWidth / 2 ? -1 : 1) * _speedX, -_speedY);
            SetSpeedMultiplier(1f);
        }

        public void SetSpeedY(int speedY)
        {
            _speedY = speedY;
            MoveDirection = new Vector2(MoveDirection.X, MathF.Sign(MoveDirection.Y) * _speedY);
        }

        public override void Update(GameTime gameTime)
        {
            _previousPosition = _position;

            base.Update(gameTime);

            if (_stuck)
            {
                _stuckTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (SimpleControls.IsADown(PlayerIndex.One) || _stuckTimer > 5f)
                {
                    Unstick();
                }
                else
                {
                    MoveTo(_spaceShip.Position + new Vector2(_stuckX + SpriteSheet.LeftMargin - _spaceShip.Size / 2, -SpriteSheet.BottomMargin));
                    return;
                }
            }

            if (Position.X > Arkanoid2024.PLAYGROUND_MAX_X - _spriteSheet.RightMargin - 2)
            {
                MoveDirection = new Vector2(-MoveDirection.X, MoveDirection.Y);
                MoveTo(new Vector2(Arkanoid2024.PLAYGROUND_MAX_X - SpriteSheet.RightMargin - 2 , Position.Y));
            }
            else if (Position.X < Arkanoid2024.PLAYGROUND_MIN_X + SpriteSheet.LeftMargin)
            {
                MoveDirection = new Vector2(-MoveDirection.X, MoveDirection.Y);
                MoveTo(new Vector2(Arkanoid2024.PLAYGROUND_MIN_X + SpriteSheet.LeftMargin, Position.Y));
            }

            if (MoveDirection.Y > 0 && Position.Y + SpriteSheet.BottomMargin > Arkanoid2024.PLAYGROUND_MAX_Y)
            {
                if (Position.X + SpriteSheet.RightMargin >= _spaceShip.Position.X - _spaceShip.Size / 2 
                    && Position.X - SpriteSheet.LeftMargin <= _spaceShip.Position.X + _spaceShip.Size /2)
                {
                    int offset = PixelPositionX - _spaceShip.PixelPositionX;
                    if (MoveDirection.X > 0 && offset < 0
                        || MoveDirection.X < 0 && offset > 0)
                    {
                        MoveDirection = new Vector2(-MoveDirection.X, -MoveDirection.Y);
                    }
                    else
                    {
                        MoveDirection = new Vector2(MoveDirection.X, -MoveDirection.Y);
                    }
                    MoveTo(new Vector2(Position.X, Arkanoid2024.PLAYGROUND_MAX_Y - SpriteSheet.BottomMargin));
                    EventsManager.FireEvent("Ping");
                    if (_spaceShip.Sticky)
                    {
                        StickToSpaceShip(offset);
                    }
                }
                else
                {
                    if (Arkanoid2024.CheatNoBallOut)
                    {
                        MoveDirection = new Vector2(MoveDirection.X, -MoveDirection.Y);
                    }
                    else
                    {
                        EventsManager.FireEvent("BallOut", this);
                    }
                }
            }
            else if (Position.Y - SpriteSheet.TopMargin < Arkanoid2024.PLAYGROUND_MIN_Y)
            {
                MoveDirection = new Vector2(MoveDirection.X, -MoveDirection.Y);
                MoveTo(new Vector2(Position.X, Arkanoid2024.PLAYGROUND_MIN_Y + SpriteSheet.TopMargin));
            }
        }


        public void TestBrickCollision(Level level)
        {
            int x = PixelPositionX - Arkanoid2024.PLAYGROUND_MIN_X + (MoveDirection.X > 0 ? SpriteSheet.RightMargin + 1 : -SpriteSheet.LeftMargin - 1);
            int y = PixelPositionY - Arkanoid2024.PLAYGROUND_MIN_Y + (MoveDirection.Y > 0 ? SpriteSheet.BottomMargin + 1 : -SpriteSheet.TopMargin - 1);

            int testedGridX = (PixelPositionX - Arkanoid2024.PLAYGROUND_MIN_X) / 8;
            int testedGridY = y / 8;

            bool brickHit = false;

            if (TestBrick(level, testedGridX, testedGridY))
            {
                MoveDirection = new Vector2(MoveDirection.X, -MoveDirection.Y);
                brickHit = true;
                EventsManager.FireEvent("BrickHit", new Point(testedGridX, testedGridY));
            }

            testedGridX = x / 8;
            testedGridY = (PixelPositionY - Arkanoid2024.PLAYGROUND_MIN_Y) / 8;

            if (TestBrick(level, testedGridX, testedGridY))
            {
                MoveDirection = new Vector2(-MoveDirection.X, MoveDirection.Y);
                if (!brickHit)
                {
                    EventsManager.FireEvent("BrickHit", new Point(testedGridX, testedGridY));
                }
            }
        }

        public void TestEnemiesCollision(List<Enemy> enemies)
        {
            int x = PixelPositionX + (MoveDirection.X > 0 ? SpriteSheet.RightMargin + 1 : -SpriteSheet.LeftMargin - 1);
            int y = PixelPositionY + (MoveDirection.Y > 0 ? SpriteSheet.BottomMargin + 1 : -SpriteSheet.TopMargin - 1);

            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i];
                if (enemy.Visible)
                {
                    int testedX = PixelPositionX;
                    int testedY = y;

                    if (TestEnemyCollision(enemy, testedX, testedY))
                    {
                        MoveDirection = new Vector2(MoveDirection.X, -MoveDirection.Y);
                        EventsManager.FireEvent("EnemyHit", enemy);
                    }

                    testedX = x;
                    testedY = PixelPositionY;

                    if (TestEnemyCollision(enemy, testedX, testedY))
                    {
                        MoveDirection = new Vector2(-MoveDirection.X, MoveDirection.Y);
                        EventsManager.FireEvent("EnemyHit", enemy);
                    }
                }
            }
        }

        private bool TestBrick(Level level, int x, int y)
        {
            if (x < Arkanoid2024.GRID_WIDTH && y < Arkanoid2024.GRID_HEIGHT)
            {
                Brick brick = level.GetBrick(x, y);
                if (brick != null && brick.Visible)
                {
                    return true;
                }
            }
            return false;
        }

        private bool TestEnemyCollision(Enemy enemy, int x, int y)
        {
            Rectangle bounds = enemy.GetBounds();
            return bounds.Contains(x, y);
        }
    }
}
