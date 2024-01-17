using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid2024
{
    public class SlowBonus : Bonus
    {
        public const string SLOW_BONUS = "Slow";

        protected override string _animationName => SLOW_BONUS;

        public SlowBonus(Vector2 position, SpriteSheet spriteSheet, Game game) : base(position, spriteSheet, game)
        {
        }

        public override void Collect(SpaceShip ship, Ball ball)
        {
            ship.Sticky = false;
            ball.SetSpeedY(1);
            base.Collect(ship, ball);
        }
    }
}
