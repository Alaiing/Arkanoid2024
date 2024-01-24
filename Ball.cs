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
        private const int BRICK_COUNT_THRESHOLD = 8;

        private SpaceShip _spaceShip;
        private bool _stuck;
        public bool IsStuck => _stuck;
        private float _stuckX;

        private Vector2 _previousPosition;
        public Vector2 PreviousPosition => _previousPosition;

        private int _speedX;
        private int _speedY;

        private int _defaultSpeedY;

        private int _brickHitCount;
        public int BrickHitCount => _brickHitCount;

        public Ball(SpriteSheet spriteSheet, SpaceShip spaceShip, Game game) : base(spriteSheet, game)
        {
            _spaceShip = spaceShip;
            SetBaseSpeed(50f);
            _speedX = 1;
            _defaultSpeedY = _speedY = ConfigManager.GetConfig("BALL_DEFAULT_SPEED_Y", 3);
            DrawOrder = 1;
            SetAnimation("Idle");
    }

    public void SetBrickHitCount(int brickHitCount)
        {
            _brickHitCount = brickHitCount;
            UpdateSpeed();
        }

        public override void Reset()
        {
            base.Reset();
            MoveTo(new Vector2(96, 177));
            _speedY = _defaultSpeedY;
            MoveDirection = new Vector2(_speedX, -_speedY);
            SetSpeedMultiplier(1f);
            _brickHitCount = 0;
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
            SetSpeedMultiplier(1f);
            EventsManager.FireEvent("Ping");
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
                    MoveTo(_spaceShip.Position + new Vector2(_stuckX, -SpriteSheet.BottomMargin));
                    return;
                }
            }

            if (Position.X > Arkanoid2024.PLAYGROUND_MAX_X - _spriteSheet.RightMargin - 2)
            {
                MoveDirection = new Vector2(-MoveDirection.X, MoveDirection.Y);
                MoveTo(new Vector2(Arkanoid2024.PLAYGROUND_MAX_X - SpriteSheet.RightMargin - 2, Position.Y));
            }
            else if (Position.X < Arkanoid2024.PLAYGROUND_MIN_X + SpriteSheet.LeftMargin)
            {
                MoveDirection = new Vector2(-MoveDirection.X, MoveDirection.Y);
                MoveTo(new Vector2(Arkanoid2024.PLAYGROUND_MIN_X + SpriteSheet.LeftMargin, Position.Y));
            }

            if (MoveDirection.Y > 0 && Position.Y + SpriteSheet.BottomMargin > Arkanoid2024.PLAYGROUND_MAX_Y)
            {
                if (Position.X + SpriteSheet.RightMargin >= _spaceShip.Position.X - _spaceShip.Size / 2
                    && Position.X - SpriteSheet.LeftMargin <= _spaceShip.Position.X + _spaceShip.Size / 2)
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
            float x = Position.X + (MoveDirection.X > 0 ? SpriteSheet.RightMargin + 1 : -SpriteSheet.LeftMargin - 1);
            float y = Position.Y + (MoveDirection.Y > 0 ? SpriteSheet.BottomMargin + 1 : -SpriteSheet.TopMargin - 1);

            Vector2 testedPosition = new Vector2(PixelPositionX, y);

            bool brickHit = false;

            if (Arkanoid2024.TestBrick(level, testedPosition, out Point gridPosition))
            {
                MoveDirection = new Vector2(MoveDirection.X, -MoveDirection.Y);
                brickHit = true;
                EventsManager.FireEvent("BrickHit", gridPosition);
            }

            testedPosition = new Vector2(x, PixelPositionY);

            if (Arkanoid2024.TestBrick(level, testedPosition, out gridPosition))
            {
                MoveDirection = new Vector2(-MoveDirection.X, MoveDirection.Y);
                if (!brickHit)
                {
                    EventsManager.FireEvent("BrickHit", gridPosition);
                    brickHit = true;
                }
            }

            if (brickHit)
            {
                _brickHitCount++;
                UpdateSpeed();
            }
        }

        public void TestEnemiesCollision(List<Enemy> enemies)
        {
            float x = Position.X + (MoveDirection.X > 0 ? SpriteSheet.RightMargin + 1 : -SpriteSheet.LeftMargin - 1);
            float y = Position.Y + (MoveDirection.Y > 0 ? SpriteSheet.BottomMargin + 1 : -SpriteSheet.TopMargin - 1);

            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i];
                if (enemy.Visible)
                {
                    float testedX = PixelPositionX;
                    float testedY = y;

                    if (TestEnemyCollision(enemy, new Vector2(testedX, testedY)))
                    {
                        MoveDirection = new Vector2(MoveDirection.X, -MoveDirection.Y);
                        EventsManager.FireEvent("EnemyHit", enemy);
                    }

                    testedX = x;
                    testedY = PixelPositionY;

                    if (TestEnemyCollision(enemy, new Vector2(testedX, testedY)))
                    {
                        MoveDirection = new Vector2(-MoveDirection.X, MoveDirection.Y);
                        EventsManager.FireEvent("EnemyHit", enemy);
                    }
                }
            }
        }

        private bool TestEnemyCollision(Enemy enemy, Vector2 position)
        {
            Rectangle bounds = enemy.GetBounds();
            return bounds.Contains(position);
        }

        public bool TestBonusCollision()
        {
            if (Bonus.CurrentFallingBonus == null)
                return false;

            float x = Position.X + (MoveDirection.X > 0 ? SpriteSheet.RightMargin + 1 : -SpriteSheet.LeftMargin - 1);
            float y = Position.Y + (MoveDirection.Y > 0 ? SpriteSheet.BottomMargin + 1 : -SpriteSheet.TopMargin - 1);

            Vector2 testedPosition = new Vector2(PixelPositionX, y);

            //if (Bonus.CurrentFallingBonus.GetBounds().Contains(testedPosition))
            //{
            //    MoveDirection = new Vector2(MoveDirection.X, -MoveDirection.Y);
            //}

            testedPosition = new Vector2(x, PixelPositionY);

            if (Bonus.CurrentFallingBonus.GetBounds().Contains(testedPosition))
            {
                MoveDirection = new Vector2(-MoveDirection.X, MoveDirection.Y);
            }

            return false;
        }

        private void UpdateSpeed()
        {
            SetSpeedMultiplier(1f + MathF.Min(1f, (_brickHitCount / BRICK_COUNT_THRESHOLD) * 0.1f));
        }
    }
}
