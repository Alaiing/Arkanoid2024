using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid2024
{
    public class BreakOutBonus : Bonus
    {
        public static string ANIMATION_NAME = "Breakout";

        protected override string _animationName => ANIMATION_NAME;
        public BreakOutBonus(Vector2 position, SpriteSheet spriteSheet, Game game) : base(position, spriteSheet, game)
        {
        }

        public override void Collect(SpaceShip ship, Ball ball)
        {
            base.Collect(ship, ball);
            EventsManager.FireEvent("Breakout");
        }
    }
}
