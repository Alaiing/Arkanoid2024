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
        public const string ANIMATION_NAME = "Slow";

        protected override string _animationName => ANIMATION_NAME;

        public SlowBonus(Vector2 position, SpriteSheet spriteSheet, Game game) : base(position, spriteSheet, game)
        {
        }

        public override void Collect(SpaceShip ship, Ball ball)
        {
            ship.Sticky = false;
            ball.SetSpeedY(1);
            ball.SetBrickHitCount(0);
            base.Collect(ship, ball);
        }
    }
}
