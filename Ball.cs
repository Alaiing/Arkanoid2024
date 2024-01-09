using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid2023
{
    public class Ball : Character
    {
        private SpaceShip _spaceShip;
        private bool _stuck;
        public bool IsStuck => _stuck;
        private float _stuckX;

        private Vector2 _previousPosition;
        public Vector2 PreviousPosition => _previousPosition;

        public Ball(SpriteSheet spriteSheet, SpaceShip spaceShip, Game game) : base(spriteSheet, game)
        {
            _spaceShip = spaceShip;
            SetBaseSpeed(80f);
            DrawOrder = 1;
        }

        public override void Reset()
        {
            base.Reset();
            MoveTo(new Vector2(96, 177));
            MoveDirection = new Vector2(1, -2);
            SetSpeedMultiplier(1f);
        }

        public void StickToSpaceShip(int offset)
        {
            _stuck = true;
            _stuckX = offset;
            SetSpeedMultiplier(0);
        }

        public void Unstick()
        {
            _stuck = false;
            MoveDirection = new Vector2(_stuckX + _spriteSheet.FrameWidth /2 < _spaceShip.SpriteSheet.FrameWidth / 2 ? -1 : 1, -2);
            SetSpeedMultiplier(1f);
        }

        public override void Update(GameTime gameTime)
        {
            _previousPosition = _position;

            base.Update(gameTime);

            if (_stuck && SimpleControls.IsADown(PlayerIndex.One))
            {
                Unstick();
            }

            if (_stuck)
            {
                MoveTo(_spaceShip.Position + new Vector2(_stuckX, -_spriteSheet.FrameHeight));
                return;
            }

            if (Position.X > Arkanoid2024.PLAYGROUND_MAX_X - _spriteSheet.RightMargin)
            {
                MoveDirection = new Vector2(-MoveDirection.X, MoveDirection.Y);
                MoveTo(new Vector2(Arkanoid2024.PLAYGROUND_MAX_X - _spriteSheet.RightMargin, Position.Y));
            } 
            else if (Position.X < Arkanoid2024.PLAYGROUND_MIN_X)
            {
                MoveDirection = new Vector2(-MoveDirection.X, MoveDirection.Y);
                MoveTo(new Vector2(Arkanoid2024.PLAYGROUND_MIN_X, Position.Y));
            }

            if (Position.Y > Arkanoid2024.PLAYGROUND_MAX_Y - _spriteSheet.BottomMargin)
            {
                if (Position.X + SpriteSheet.FrameWidth / 2 >= _spaceShip.Position.X && Position.X + SpriteSheet.FrameWidth / 2 <= _spaceShip.Position.X + _spaceShip.SpriteSheet.FrameWidth)
                {
                    int offset = PixelPositionX - _spaceShip.PixelPositionX;
                    if (MoveDirection.X > 0 && offset < _spaceShip.SpriteSheet.FrameWidth / 2
                        || MoveDirection.X < 0 && offset > _spaceShip.SpriteSheet.FrameWidth / 2)
                    {
                        MoveDirection = new Vector2(-MoveDirection.X, -MoveDirection.Y);
                    }
                    else
                    {
                        MoveDirection = new Vector2(MoveDirection.X, -MoveDirection.Y);
                    }
                    MoveTo(new Vector2(Position.X, Arkanoid2024.PLAYGROUND_MAX_Y - _spriteSheet.BottomMargin));
                }
                else
                {
                    EventsManager.FireEvent("BallOut");
                }
            }
            else if (Position.Y < Arkanoid2024.PLAYGROUND_MIN_Y)
            {
                MoveDirection = new Vector2(MoveDirection.X, -MoveDirection.Y);
                MoveTo(new Vector2(Position.X, Arkanoid2024.PLAYGROUND_MIN_Y));
            }
        }

    }
}
