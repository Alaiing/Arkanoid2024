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
        public new const string ANIMATION_NAME = "Player";

        protected override string _animationName => ANIMATION_NAME;
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
