using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid2024
{
    public class DisruptBonus : Bonus
    {
        public const string DISRUPT_BONUS = "Disrupt";

        protected override string _animationName => DISRUPT_BONUS;
        
        public DisruptBonus(Vector2 position, SpriteSheet spriteSheet, Game game) : base(position, spriteSheet, game)
        {
        }

        public override void Collect(SpaceShip ship, Ball ball)
        {
            ship.Sticky = false;
            if (ball.IsStuck)
            {
                ball.Unstick();
            }

            EventsManager.FireEvent("Multibaaaaall");
            base.Collect(ship, ball);
        }
    }
}
