using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid2024
{
    public class Level
    {
        private Color[] _brickColors = new Color[]
        {
            Color.Red,
            Color.Yellow,
            new Color(0, 128, 255),
            new Color(128, 0, 128),
            new Color(0, 255, 0),
            new Color(255, 128, 0),
            Color.White
        };

        private Brick[,] _bricks;
        private SpriteSheet _basicBrickSheet;
        private SpriteSheet _silverBrickSheet;
        private SpriteSheet _goldenBrickSheet;
        private OudidonGame _game;

        private int _brickCount;
        public int BrickCount => _brickCount;

        public Level(string levelAsset, SpriteSheet basicBrick, SpriteSheet silverBrick, SpriteSheet goldenBrick, OudidonGame game)
        {
            _bricks = new Brick[Arkanoid2024.GRID_WIDTH, Arkanoid2024.GRID_HEIGHT];
            _game = game;
            _basicBrickSheet = basicBrick;
            _silverBrickSheet = silverBrick;
            _goldenBrickSheet = goldenBrick;
            Load(levelAsset);
        }

        public Brick GetBrick(int x, int y)
        {
            return _bricks[x, y];
        }

        public void Activate()
        {
            foreach (Brick brick in _bricks)
            {
                if (brick != null)
                {
                    _game.Components.Add(brick);
                }
            }
        }

        public void DeActivate()
        {
            foreach (Brick brick in _bricks)
            {
                if (brick != null)
                {
                    _game.Components.Remove(brick);
                }
            }
        }

        public void Reset()
        {
            foreach (Brick brick in _bricks)
            {
                brick?.Reset();
            }
        }

        private void Load(string asset)
        {
            if (System.IO.File.Exists(asset))
            {
                string[] lines = System.IO.File.ReadAllLines(asset);
                _brickCount = 0;
                for (int y = 0; y < lines.Length; y++)
                {
                    string line = lines[y];
                    for (int x = 0; x < line.Length; x++)
                    {
                        if (line[x] != '0')
                        {
                            Brick newBrick;
                            Color color = Color.White;
                            int brickType = int.Parse(line[x].ToString());
                            switch (brickType)
                            {
                                case Brick.BRICK_SILVER:
                                    newBrick = new Brick(_silverBrickSheet, _game, maxHealth: 2, points: 50);
                                    break;
                                case Brick.BRICK_GOLDEN:
                                    newBrick = new Brick(_goldenBrickSheet, _game, maxHealth: 0, points: 50);
                                    break;
                                default:
                                    newBrick = new Brick(_basicBrickSheet, _game, maxHealth: 1, points: 50);
                                    color = _brickColors[brickType - 3];
                                    break;
                            }

                            newBrick.Reset();
                            newBrick.SetColors(new Color[] { color });
                            newBrick.MoveTo(new Vector2(x * 8 + Arkanoid2024.PLAYGROUND_MIN_X + 2, y * 8 + Arkanoid2024.PLAYGROUND_MIN_Y));

                            _bricks[x, y] = newBrick;
                            _brickCount++;
                        }
                        else
                        {
                            _bricks[x, y] = null;
                        }
                    }
                }
            }
        }
    }
}
