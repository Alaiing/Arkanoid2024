using Microsoft.Xna.Framework;
using Oudidon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid2024
{
    public class Brick : Character
    {
        public const int BRICK_SILVER = 1;
        public const int BRICK_GOLDEN = 2;

        private int _points;
        private int _health;
        private int _maxHealth;

        public bool IsDestroyed => _maxHealth > 0 && _health <= 0;

        public Brick(SpriteSheet spriteSheet, Game game, int maxHealth, int points) : base(spriteSheet, game)
        {
            _points = points;
            _maxHealth = maxHealth;
            SetAnimation("Idle");
        }

        public override void Reset()
        {
            base.Reset();
            _health = _maxHealth;
            Visible = true;
        }

        public void Hit()
        {
            if (_maxHealth > 0)
            {
                _health--;
                if (_health <= 0 )
                {
                    Visible = false;
                    EventsManager.FireEvent("BrickDestroyed", this);
                }
                else
                {
                    SetAnimation("Hit", () => SetAnimation("Idle"));
                }
            }

        }
    }
}
