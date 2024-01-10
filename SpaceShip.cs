using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid2024
{
    public class SpaceShip : Character
    {
        private Vector2 _defaultPosition;
        private int _livesLeft;
        public int LivesLeft => _livesLeft;
        private int _startingLives;

        public SpaceShip(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)        
        {        
            _defaultPosition = new Vector2(68, Arkanoid2024.PLAYGROUND_MAX_Y);
            _startingLives = ConfigManager.GetConfig("STARTING_LIVES", 4);
            SetBaseSpeed(100f);
        }

        public override void Reset()
        {
            base.Reset();
            _livesLeft = _startingLives;
            ResetPosition();
        }

        public void ResetPosition()
        {
            MoveTo(_defaultPosition);
        }

        public void LoseLife()
        {
            if (!Arkanoid2024.CheatInfiniteLives)
            {
                _livesLeft--;
            }
        }

        public override void Update(GameTime gameTime)
        {
            SimpleControls.GetStates();
            if (SimpleControls.IsLeftDown(PlayerIndex.One))
            {
                MoveDirection = new Vector2(-1, 0);
                SetSpeedMultiplier(1f);
            }
            else if (SimpleControls.IsRightDown(PlayerIndex.One)) 
            {
                MoveDirection = new Vector2(1, 0);
                SetSpeedMultiplier(1f);
            }
            else
            {
                SetSpeedMultiplier(0f);
            }
            base.Update(gameTime);

            if (Position.X < Arkanoid2024.PLAYGROUND_MIN_X - 2)
            {
                MoveTo(new Vector2(Arkanoid2024.PLAYGROUND_MIN_X - 2, Position.Y));
            }
            else if (Position.X > Arkanoid2024.PLAYGROUND_MAX_X - _spriteSheet.FrameWidth)
            {
                MoveTo(new Vector2(Arkanoid2024.PLAYGROUND_MAX_X - _spriteSheet.FrameWidth, Position.Y));
            }
        }
    }
}
