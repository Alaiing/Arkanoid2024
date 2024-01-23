using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid2024
{
    public class LaserBonus : Bonus
    {
        public new const string ANIMATION_NAME = "Laser";

        protected override string _animationName => ANIMATION_NAME;

        public LaserBonus(Vector2 position, SpriteSheet spriteSheet, Game game) : base(position, spriteSheet, game)
        {
        }

        public override void Collect(SpaceShip ship, Ball ball)
        {
            ship.Sticky = false;
            ship.SetType(SpaceShip.SpaceShipType.Laser);
            base.Collect(ship, ball);
        }
    }
}
