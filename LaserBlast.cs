using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid2024
{
    public class LaserBlast : Character
    {
        protected static List<LaserBlast> _currentLaserBlasts = new List<LaserBlast>();
        public static List<LaserBlast> CurrentLaserBlasts => _currentLaserBlasts;

        public static void ClearBonuses()
        {
            for (int i = _currentLaserBlasts.Count - 1; i >= 0; i--)
            {
                _currentLaserBlasts[i].Kill();
            }
        }

        public LaserBlast(Vector2 position, SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            MoveDirection = new Vector2(0, -1);
            SetAnimation("Idle");
            SetBaseSpeed(300f);
            MoveTo(position);
            game.Components.Add(this);
            _currentLaserBlasts.Add(this);
        }

        public static void TestCollisions(Level level, List<Enemy> enemies)
        {
            for (int i = _currentLaserBlasts.Count -1; i>= 0; i--)
            {
                _currentLaserBlasts[i].TestCollision(level, enemies);
            }
        }

        public void TestCollision(Level level, List<Enemy> enemies)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i];
                if (enemy.GetBounds().Contains(Position))
                {
                    EventsManager.FireEvent("EnemyHit", enemy);
                    Kill();
                    return;
                }
                if (enemy.GetBounds().Contains(Position + new Vector2(SpriteSheet.FrameWidth, 0)))
                {
                    EventsManager.FireEvent("EnemyHit", enemy);
                    Kill();
                    return;
                }
            }
        }

        public void Kill()
        {
            _currentLaserBlasts.Remove(this);
            Game.Components.Remove(this);
            Deactivate();
            Dispose();
        }

    }
}
