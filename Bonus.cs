using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid2024
{
    public abstract class Bonus : Character
    {
        protected virtual string _animationName { get; }
        protected static Bonus _currentFallingBonus;
        public static Bonus CurrentFallingBonus => _currentFallingBonus;

        public static void ClearBonuses()
        {
                _currentFallingBonus?.Kill();
        }

        public Bonus(Vector2 position, SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            MoveDirection = new Vector2(0, 1);
            SetAnimation(_animationName);
            SetBaseSpeed(ConfigManager.GetConfig("BONUS_SPEED", 20f));
            MoveTo(position);
            Game.Components.Add(this);
            _currentFallingBonus = this;
        }

        public virtual void Collect(SpaceShip ship, Ball ball)
        {
            Kill();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Position.Y > Arkanoid2024.PLAYGROUND_MAX_Y + 10)
            {
                Kill();
            }
        }

        public void Kill()
        {
            _currentFallingBonus = null;
            Game.Components.Remove(this);
            Deactivate();
            Dispose();
        }

    }
}
