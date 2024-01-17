using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid2024
{
    public class CatchBonus : Bonus
    {
        public const string CATCH_BONUS = "Catch";
        protected override string _animationName => CATCH_BONUS;

        public CatchBonus(Vector2 position, SpriteSheet spriteSheet, Game game) : base(position, spriteSheet, game) { }

        public override void Collect(SpaceShip ship, Ball ball)
        {
            ship.Sticky = true;
            ship.SetType(SpaceShip.SpaceShipType.Default);
            base.Collect(ship, ball);
        }
    }
}
