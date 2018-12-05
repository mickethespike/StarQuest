using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.TextureAtlases;
using System;
using System.Collections.Generic;

namespace StarQuest
{
    public class GameFrame : Game
    {
        private Color _clearColor = Color.DarkSlateBlue * 0.33f;
        private GraphicsDeviceManager _graphicsManager;
        private SpriteBatch _spriteBatch;

        private Player _player;
        public Tile[,] Tiles;
        public const int TileSize = 32;

        Dictionary<TileType, TextureRegion2D> _tileTextures;

        public GameFrame()
        {
            _graphicsManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Input.AddWindow(Window);

            base.Initialize();

            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            //Window.IsBorderless = true;
            //Window.IsMaximized = true;

            LoadWorld();
        }

        private void LoadWorld()
        {
            Tiles = new Tile[100, 50];
            _player = new Player(this);

            Random rng = new Random();
            for (int x = 0; x < 100; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    Tile tileD = new Tile(y == 5 ? TileType.Snow : TileType.Dirt);
                    double flipChance = rng.NextDouble();

                    if (flipChance > 0.666)
                        tileD.Flip = SpriteEffects.FlipHorizontally;
                    else if (flipChance > 0.333 && tileD.Type != TileType.Snow)
                        tileD.Flip = SpriteEffects.FlipVertically;
                    else
                        tileD.Flip = SpriteEffects.None;

                    SetTile(x, y, tileD);
                }
            }
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            Texture2D tileTexture = Content.Load<Texture2D>("Textures/tiles");
            _tileTextures = new Dictionary<TileType, TextureRegion2D>
            {
                { TileType.Dirt, new TextureRegion2D(tileTexture, 32, 0, 32, 32) },
                { TileType.Snow, new TextureRegion2D(tileTexture, 0, 0,  32, 32) }
            };
        }

        protected override void UnloadContent()
        {
        }

        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            Input.Update(gameTime);
            if (Input.IsKeyDown(Keys.Escape))
                Exit();

            _player.Update(gameTime, GraphicsDevice.Viewport);

            base.Update(gameTime);
        }

        public bool IsPointValid(int x, int y)
        {
            if (x < 0 || x > Tiles.GetLength(0))
                return false;
            if (y < 0 || y > Tiles.GetLength(1))
                return false;
            return true;
        }

        public Tile GetTile(int x, int y)
        {
            return Tiles[x, y];
        }

        public void SetTile(int x, int y, Tile tile)
        {
            Tiles[x, y] = tile;
        }

        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_clearColor);

            Viewport view = GraphicsDevice.Viewport;

            _spriteBatch.Begin();

            for (int x = 0; x < Tiles.GetLength(0); x++)
            {
                for (int y = 0; y < Tiles.GetLength(1); y++)
                {
                    Tile tile = GetTile(x, y);
                    if (tile.Type == TileType.Air)
                        continue;

                    if (!_tileTextures.TryGetValue(tile.Type, out TextureRegion2D texRegion))
                        continue;

                    int tileX = x * TileSize;
                    int tileY = view.Height - y * TileSize - TileSize;
                    Vector2 tilePos = new Vector2(tileX, tileY);

                    _spriteBatch.Draw(
                        texRegion, tilePos, Color.White, 0, Vector2.Zero, Vector2.One, tile.Flip, 0);
                }
            }

            _player.Draw(_spriteBatch, view);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }

    public struct Tile
    {
        public TileType Type;
        public SpriteEffects Flip;

        public Tile(TileType type)
        {
            Type = type;
            Flip = SpriteEffects.None;
        }
    }

    public enum TileType
    {
        Air = 0,
        Snow,
        Dirt,
    }
}