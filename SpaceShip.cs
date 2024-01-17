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
        public const string DEFAULT = "Default";
        public const string EXTENDED = "Extended";
        public const string LASER = "Laser";

        public enum SpaceShipType { Default, Extended, Laser }

        private Vector2 _defaultPosition;
        private int _livesLeft;
        public int LivesLeft => _livesLeft;
        private int _startingLives;

        private SpaceShipType _type;

        public bool Sticky;

        private int _size;
        public int Size => _size;

        private SpriteSheet _laserBlastSprite;

        public SpaceShip(SpriteSheet spriteSheet, SpriteSheet laserBlastSprite, Game game) : base(spriteSheet, game)        
        {        
            _laserBlastSprite = laserBlastSprite;
            _defaultPosition = new Vector2(68, Arkanoid2024.PLAYGROUND_MAX_Y);
            _startingLives = ConfigManager.GetConfig("STARTING_LIVES", 4);
            SetBaseSpeed(200f);
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

        public void IncreaseLife()
        {
            _livesLeft = Math.Min(10, _livesLeft + 1);
        }

        public void SetType(SpaceShipType spaceShipType)
        {
            _type = spaceShipType;
            switch (spaceShipType)
            {
                case SpaceShipType.Default:
                    _size = 18;
                    SetAnimation(DEFAULT);
                    break;
                case SpaceShipType.Extended:
                    _size = 22;
                    SetAnimation(EXTENDED);
                    break;
                case SpaceShipType.Laser:
                    _size = 18;
                    SetAnimation(LASER);
                    break;
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

            if (_type == SpaceShipType.Laser && SimpleControls.IsAPressedThisFrame(PlayerIndex.One))
            {
                new LaserBlast(Position + new Vector2(-_size / 2 + 2, - _laserBlastSprite.BottomMargin), _laserBlastSprite, Game);
            }

            base.Update(gameTime);

            if (Position.X  - _size / 2 < Arkanoid2024.PLAYGROUND_MIN_X - 2)
            {
                MoveTo(new Vector2(Arkanoid2024.PLAYGROUND_MIN_X - 2 + _size / 2, Position.Y));
            }
            else if (Position.X + _size / 2 > Arkanoid2024.PLAYGROUND_MAX_X)
            {
                MoveTo(new Vector2(Arkanoid2024.PLAYGROUND_MAX_X - _size / 2 , Position.Y));
            }
        }
    }
}
