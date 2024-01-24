using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid2024
{
    internal class Breakout : Character
    {
        public Breakout(SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            DrawOrder = 99;
        }

        public override void Draw(GameTime gameTime)
        {
            Game.SpriteBatch.Draw(SpriteSheet.GetTexture(), Position, new Rectangle(0, 30 - (int)(gameTime.TotalGameTime.TotalSeconds * 62) % 30, SpriteSheet.FrameWidth, 31), Color.White);
        }
    }
}
