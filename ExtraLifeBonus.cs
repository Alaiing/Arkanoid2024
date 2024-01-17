using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid2024
{
    public class ExtraLifeBonus : Bonus
    {
        public const string EXTRA_LIFE_BONUS = "Player";

        protected override string _animationName => EXTRA_LIFE_BONUS;
        public ExtraLifeBonus(Vector2 position, SpriteSheet spriteSheet, Game game) : base(position, spriteSheet, game)
        {
        }

        public override void Collect(SpaceShip ship, Ball ball)
        {
            ship.IncreaseLife();
            base.Collect(ship, ball);
        }
    }
}
