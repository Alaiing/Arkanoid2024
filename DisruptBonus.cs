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
        public const string ANIMATION_NAME = "Disrupt";

        protected override string _animationName => ANIMATION_NAME;
        
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
            ship.SetType(SpaceShip.SpaceShipType.Default);

            EventsManager.FireEvent("Multibaaaaall");
            base.Collect(ship, ball);
        }
    }
}
