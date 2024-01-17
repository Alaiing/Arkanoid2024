using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid2024
{
    public class Enemy : Character
    {
        private const string STATE_GOING_DOWN = "GoingDown";
        private const string STATE_GOING_LEFTRIGHT = "GoingLeftRight";
        private const string STATE_STRAIGHT = "Crazy";
        private const string STATE_ROUND = "Round";
        private const string STATE_DYING = "Dying";

        private Level _level;
        private List<Enemy> _allEnemies;
        private SimpleStateMachine _stateMachine;

        public Enemy(SpriteSheet spriteSheet, Level level, List<Enemy> allEnemies, Game game) : base(spriteSheet, game)
        {
            _level = level;
            _allEnemies = allEnemies;
            _stateMachine = new SimpleStateMachine();
            SetBaseSpeed(50f);
            _bump = 1;
        }

        public override void Initialize()
        {
            base.Initialize();

            _stateMachine.AddState(STATE_GOING_DOWN, OnEnter: GoingDownEnter, OnUpdate: GoingDownUpdate);
            _stateMachine.AddState(STATE_GOING_LEFTRIGHT, OnEnter: GoingLeftRightEnter, OnUpdate: GoingLeftRightUpdate);
            _stateMachine.AddState(STATE_STRAIGHT, OnEnter: StraightEnter, OnUpdate: StraightUpdate);
            _stateMachine.AddState(STATE_ROUND, OnEnter: RoundEnter, OnUpdate: RoundUpdate);
            _stateMachine.AddState(STATE_DYING);

            _stateMachine.SetState(STATE_GOING_DOWN);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _stateMachine.Update(gameTime);

            if (Position.X + SpriteSheet.RightMargin >= Arkanoid2024.PLAYGROUND_MAX_X)
            {
                MoveDirection = new Vector2(-MoveDirection.X, MoveDirection.Y);
                MoveTo(new Vector2(Arkanoid2024.PLAYGROUND_MAX_X - SpriteSheet.RightMargin, Position.Y));
                _bump = -_bump;
            }
            else if (Position.X - SpriteSheet.LeftMargin <= Arkanoid2024.PLAYGROUND_MIN_X)
            {
                MoveDirection = new Vector2(-MoveDirection.X, MoveDirection.Y);
                MoveTo(new Vector2(Arkanoid2024.PLAYGROUND_MIN_X + SpriteSheet.LeftMargin, Position.Y));
                _bump = -_bump;
            }

            if (Position.Y > Arkanoid2024.PLAYGROUND_MAX_Y)
            {
                _allEnemies.Remove(this);
                Kill();
            }
        }

        public void Kill()
        {
            Game.Components.Remove(this);
            Deactivate();
            Dispose();
        }

        private void StraightEnter()
        {
            MoveDirection = new Vector2(1, 1);
        }

        private void StraightUpdate(GameTime gameTime, float stateTime)
        {
            if (stateTime > 1.25f)
                _stateMachine.SetState(STATE_ROUND);
        }

        private void RoundEnter()
        {
            MoveDirection = new Vector2(-1, 1);
        }

        private int _bump;
        private float _changeDirectionTime = 0.25f;
        private int[] _directionY = new int[6] { 1, 0, -1, -1, 0, 1 };
        private void RoundUpdate(GameTime gameTime, float stateTime)
        {
            if (stateTime > 6 * _changeDirectionTime)
            {
                _stateMachine.SetState(STATE_STRAIGHT);
                return;
            }
            int indexY = (int)MathF.Floor(stateTime / _changeDirectionTime);
            int directionX = (int)MathF.Floor(stateTime / (_changeDirectionTime * 3)) * 2 - 1;
            MoveDirection = new Vector2(directionX * _bump, _directionY[indexY]);
        }

        private void GoingDownEnter()
        {
            MoveDirection = new Vector2(0, 1);
        }

        private void GoingDownUpdate(GameTime gameTime, float stateTime)
        {
            if (Position.Y > (Arkanoid2024.PLAYGROUND_MAX_Y + Arkanoid2024.PLAYGROUND_MIN_Y) / 2)
            {
                _stateMachine.SetState(STATE_ROUND);
                return;
            }

            if (!CanGoDown())
            {
                int positionInGridY = (PixelPositionY + 1 - Arkanoid2024.PLAYGROUND_MIN_Y + SpriteSheet.BottomMargin) / 8;
                MoveTo(new Vector2(Position.X, Arkanoid2024.PLAYGROUND_MIN_Y + positionInGridY * 8 - 1 - SpriteSheet.BottomMargin));
                _stateMachine.SetState(STATE_GOING_LEFTRIGHT);
            }
        }

        private void GoingLeftRightEnter()
        {
            MoveDirection = new Vector2(CommonRandom.Random.Next(0, 2) * 2 - 1, 0);
        }

        private void GoingLeftRightUpdate(GameTime time, float arg2)
        {
            if (CanGoDown())
            {
                _stateMachine.SetState(STATE_GOING_DOWN);
                return;
            }

            foreach (Enemy enemy in _allEnemies)
            {
                if (enemy != this && Arkanoid2024.TestCollision(this, enemy))
                {
                    MoveDirection = -MoveDirection;
                    return;
                }
            }

            int margin = MoveDirection.X > 0 ? SpriteSheet.RightMargin : -SpriteSheet.LeftMargin;
            int positionInGridX = (PixelPositionX + margin - Arkanoid2024.PLAYGROUND_MIN_X) / 8;
            int positionInGridY = (PixelPositionY - Arkanoid2024.PLAYGROUND_MIN_Y + SpriteSheet.BottomMargin) / 8;

            Brick brick = _level.GetBrick(positionInGridX, positionInGridY);
            if (brick != null && brick.Visible)
            {
                MoveDirection = -MoveDirection;
            }
        }

        private bool CanGoDown()
        {
            if (CollidesWithOther())
                return false;

            int positionInGridX = (PixelPositionX - SpriteSheet.LeftMargin - Arkanoid2024.PLAYGROUND_MIN_X) / 8;
            int positionInGridY = (PixelPositionY + 1 - Arkanoid2024.PLAYGROUND_MIN_Y + SpriteSheet.BottomMargin) / 8;
            Brick brickLeft = _level.GetBrick(positionInGridX, positionInGridY);
            positionInGridX = (PixelPositionX + SpriteSheet.RightMargin - Arkanoid2024.PLAYGROUND_MIN_X) / 8;
            Brick brickRight = _level.GetBrick(positionInGridX, positionInGridY);
            if ((brickLeft == null || !brickLeft.Visible) && (brickRight == null || !brickRight.Visible))
            {
                _stateMachine.SetState(STATE_GOING_DOWN);
                return true;
            }

            return false;
        }

        private bool CollidesWithOther()
        {
            foreach (Enemy enemy in _allEnemies)
            {
                if (enemy != this && Arkanoid2024.TestCollision(this, enemy))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
