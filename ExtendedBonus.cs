using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid2024
{
    public class ExtendedBonus : Bonus
    {
        public const string EXTENDED_BONUS = "Extended";

        protected override string _animationName => EXTENDED_BONUS;

        public ExtendedBonus(Vector2 position, SpriteSheet spriteSheet, Game game) : base(position, spriteSheet, game)
        {
        }

        public override void Collect(SpaceShip ship, Ball ball)
        {
            ship.Sticky = false;
            ship.SetType(SpaceShip.SpaceShipType.Extended);
            base.Collect(ship, ball);
        }
    }
}
