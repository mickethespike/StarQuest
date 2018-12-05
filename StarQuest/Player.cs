using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarQuest
{
    class Player
    {
        private GameFrame _game;

        public Vector2 _position;
        private Vector2 _velocity;

        public Player(GameFrame game)
        {
            _game = game;
        }

        public bool IsCollidable(TileType type)
        {
            switch(type)
        {
                case TileType.Air:
                    return false;

                default:
                    return true;
            }
        }
    
        public void Update(GameTime time, Viewport view)
        {
            _velocity.Y -= 100f + _velocity.Y;

            int tileX = (int)(_position.X / GameFrame.TileSize);
            int tileY = (int)((_position.Y) / GameFrame.TileSize - 1);

            if (_game.IsPointValid(tileX, tileY))
            {
                Tile tileUnderPlayer = _game.GetTile(tileX, tileY);
                if (IsCollidable(tileUnderPlayer.Type))
                {
                    _velocity.Y = 0;
                }
                Console.WriteLine(tileX + " - " + tileY);
            }

            if (Input.IsKeyPressed(Keys.Space) && _velocity.Y <= 0.1f)
                _velocity.Y = 50;

            if (Input.IsKeyDown(Keys.Right))
                _velocity.X = 300;

            if (Input.IsKeyDown(Keys.Left))
                _velocity.X = -300;

            _position.X += _velocity.X * time.Delta;
            _position.Y -= _velocity.Y * time.Delta;
        }

        public void Draw(SpriteBatch batch, Viewport view)
        {
            batch.DrawFilledRectangle(_position.X, view.Height - _position.Y, 50, 50, Color.Red);
        }
    }
}
