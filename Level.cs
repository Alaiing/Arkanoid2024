using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid2023
{
    public class Level : DrawableGameComponent
    {
        private Color[] _brickColors = new Color[] 
        { 
            Color.Red,
            Color.Yellow,
            new Color(0, 128, 255),
            new Color(128, 0, 128),
            new Color(0, 255, 0)
        };
        public class Brick
        {
            public Color color;
            public int points;
            public int health;
            public bool destroyed;
        }

        private Brick[,] _bricks;
        private Texture2D _brickTexture;

        public Level(string levelAsset, Texture2D brickTexture, Game game) : base(game)
        {
            _bricks = new Brick[Arkanoid2024.GRID_WIDTH, Arkanoid2024.GRID_HEIGHT];
            Load(levelAsset);
            _brickTexture = brickTexture;
            DrawOrder = 0;
        }

        public Brick GetBrick(int x, int y)
        {
            return _bricks[x, y];
        }

        private void Load(string asset)
        {
            if (System.IO.File.Exists(asset))
            {
                string[] lines = System.IO.File.ReadAllLines(asset);
                for (int y = 0; y < lines.Length; y++)
                {
                    string line = lines[y];
                    for (int x = 0; x < line.Length; x++)
                    {
                        if (line[x] != '0')
                        {
                            int brickType = int.Parse(line[x].ToString());
                            Color color = brickType < 6 ? _brickColors[brickType - 1] : Color.White;
                            // TODO: different brick type
                            _bricks[x, y] = new Brick { color = color, points = 50, health = 1, destroyed = false };
                        }
                        else
                        {
                            _bricks[x, y] = null;
                        }
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            for (int x = 0; x < Arkanoid2024.GRID_WIDTH; x++)
            {
                for (int y = 0; y < Arkanoid2024.GRID_HEIGHT; y++)
                {
                    if (_bricks[x, y] != null && !_bricks[x, y].destroyed)
                    {
                        (Game as OudidonGame).SpriteBatch.Draw(_brickTexture, new Vector2(44 + x * _brickTexture.Width, 38 + y * _brickTexture.Height), _bricks[x, y].color);
                    }
                }
            }
        }
    }
}
