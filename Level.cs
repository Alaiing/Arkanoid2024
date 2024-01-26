using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

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
            Color.White,
            Color.Blue
        };

        private int[] _brickPoints = new int[]
        {
            90,
            120,
            70,
            110,
            80,
            60,
            50,
            100
        };

        private Brick[,] _bricks;
        private int _width, _height;
        public int Width => _width;
        public int Height => _height;

        private SpriteSheet _basicBrickSheet;
        private SpriteSheet _silverBrickSheet;
        private SpriteSheet _goldenBrickSheet;
        private OudidonGame _game;

        private int _brickCount;
        public int BrickCount => _brickCount;

        private int _enemySpriteSheetIndex;
        public int EnemySpriteSheetIndex => _enemySpriteSheetIndex;

        public Level(string levelAsset, int levelIndex, SpriteSheet basicBrick, SpriteSheet silverBrick, SpriteSheet goldenBrick, OudidonGame game)
        {
            _width = Arkanoid2024.GRID_WIDTH;
            _height = Arkanoid2024.GRID_HEIGHT;
            _bricks = new Brick[_width, _height];
            _game = game;
            _basicBrickSheet = basicBrick;
            _silverBrickSheet = silverBrick;
            _goldenBrickSheet = goldenBrick;
            Load(levelAsset, levelIndex);
        }

        public Brick GetBrick(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _height)
                return null;

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

        private void Load(string asset, int levelIndex)
        {
            if (System.IO.File.Exists(asset))
            {
                string[] lines = System.IO.File.ReadAllLines(asset);
                _brickCount = 0;
                _enemySpriteSheetIndex = int.Parse(lines[0]);
                for (int y = 1; y < lines.Length; y++)
                {
                    string line = lines[y];
                    for (int x = 0; x < line.Length; x++)
                    {
                        if (line[x] != '0')
                        {
                            Brick newBrick;
                            Color color = Color.White;
                            int brickType = int.Parse(line[x].ToString(),System.Globalization.NumberStyles.HexNumber);
                            switch (brickType)
                            {
                                case Brick.BRICK_SILVER:
                                    newBrick = new Brick(_silverBrickSheet, _game, maxHealth: 2 + (levelIndex / 8), points: 50);
                                    _brickCount++;
                                    break;
                                case Brick.BRICK_GOLDEN:
                                    newBrick = new Brick(_goldenBrickSheet, _game, maxHealth: 0, points: 0);
                                    break;
                                default:
                                    newBrick = new Brick(_basicBrickSheet, _game, maxHealth: 1, points: _brickPoints[brickType - 3]);
                                    color = _brickColors[brickType - 3];
                                    _brickCount++;
                                    break;
                            }

                            newBrick.Reset();
                            newBrick.SetColors(new Color[] { color });
                            newBrick.MoveTo(new Vector2(x * 8 + Arkanoid2024.PLAYGROUND_MIN_X + 2, (y - 1) * 8 + Arkanoid2024.PLAYGROUND_MIN_Y));

                            _bricks[x, y - 1] = newBrick;
                        }
                        else
                        {
                            _bricks[x, y - 1] = null;
                        }
                    }
                }
            }
        }

        public int GetRemainingPoints()
        {
            int points = 0;

            foreach (Brick brick in _bricks)
            {
                if (brick != null && brick.Visible)
                {
                    points += brick.Points;
                }
            }

            return points;
        }
    }
}
