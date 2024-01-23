using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
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

        private SoundEffectInstance _soundInstance;

        public static void ClearBonuses()
        {
            for (int i = _currentLaserBlasts.Count - 1; i >= 0; i--)
            {
                _currentLaserBlasts[i].Kill();
            }
        }

        public LaserBlast(Vector2 position, SoundEffectInstance soundInstance, SpriteSheet spriteSheet, Game game) : base(spriteSheet, game)
        {
            _soundInstance = soundInstance;
            MoveDirection = new Vector2(0, -1);
            SetAnimation("Idle");
            SetBaseSpeed(300f);
            MoveTo(position);
            game.Components.Add(this);
            _currentLaserBlasts.Add(this);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Position.Y < Arkanoid2024.PLAYGROUND_MIN_Y)
            {
                Kill(killSound: false);
            }
        }

        public static void TestCollisions(Level level, List<Enemy> enemies)
        {
            for (int i = _currentLaserBlasts.Count - 1; i >= 0; i--)
            {
                _currentLaserBlasts[i].TestCollision(level, enemies);
            }
        }

        public void TestCollision(Level level, List<Enemy> enemies)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy enemy = enemies[i];

                (bool left, bool right) contacts = TestContact(enemy);

                if (contacts.left)
                {
                    EventsManager.FireEvent("EnemyHit", enemy);
                    Kill();
                    return;
                }
                if (contacts.right)
                {
                    EventsManager.FireEvent("EnemyHit", enemy);
                    Kill();
                    return;
                }
            }

            if (Bonus.CurrentFallingBonus != null)
            {
                (bool left, bool right) contacts = TestContact(Bonus.CurrentFallingBonus);
                if (contacts.left || contacts.right)
                    Kill();
            }

            Vector2 offsetPosition = Position;
            if (Arkanoid2024.TestBrick(level, offsetPosition, out Point brickPosition))
            {
                EventsManager.FireEvent("BrickHit", brickPosition);
                Kill();
            }
            else if (Arkanoid2024.TestBrick(level, offsetPosition + new Vector2(SpriteSheet.FrameWidth, 0), out brickPosition))
            {
                EventsManager.FireEvent("BrickHit", brickPosition);
                Kill();
            }
        }

        private (bool, bool) TestContact(Character character)
        {
            Vector2 leftLaser = Position;
            Vector2 rightLaser = Position + new Vector2(SpriteSheet.FrameWidth, 0);
            (bool left, bool right) contacts = (false, false);
            if (character.GetBounds().Contains(leftLaser))
            {
                contacts.left = true;
            }
            if (character.GetBounds().Contains(rightLaser))
            {
                contacts.right = true;
            }

            return contacts;
        }

        public void Kill(bool killSound = true)
        {
            _currentLaserBlasts.Remove(this);
            Game.Components.Remove(this);
            if (killSound)
            {
                _soundInstance.Stop();
            }
            Deactivate();
            Dispose();
        }

    }
}
