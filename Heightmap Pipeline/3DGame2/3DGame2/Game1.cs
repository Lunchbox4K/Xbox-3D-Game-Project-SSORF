using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;

namespace _3dOnlineGame
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Input playerInput;
        CameraManager cameras;
        Model modela;
        Terrain terrain;
        //TerrainInfo terrainInfo;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this); 
            Content.RootDirectory = "Content";
            //Required for Gamer, Signing in players, networking, and xbox support
            Components.Add(new GamerServicesComponent(this));
            //Create and register the input component
            playerInput = new Input(this, PlayerIndex.Four);
            Components.Add(playerInput);
            //Create and register the camera component
            cameras = new CameraManager(this);
            Components.Add(cameras);

            terrain = new Terrain(this, GraphicsDevice, Content, cameras);
            Components.Add(terrain);

            //playarTest = new playerManager(this);
            //Components.Add(playarTest);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            cameras.setGraphicsDevice(GraphicsDevice);

            modela = Content.Load<Model>("box");
            (modela.Meshes[0].Effects[0] as BasicEffect).EnableDefaultLighting();
            modela = Content.Load<Model>("box");
            (modela.Meshes[0].Effects[0] as BasicEffect).EnableDefaultLighting();

            base.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (IsActive)
            {
                if (Gamer.SignedInGamers.Count <= 0)
                {
#if WINDOWS
                    Guide.ShowSignIn(1, false);
#elif XBOX
                    Guide.ShowSignIn(4, false);
#endif

                }
                else
                {
                    for (int i = 0; i < LocalNetworkGamer.SignedInGamers.Count; i++)
                    {
                        GamePadButtons buttonspressed = playerInput.GetButtonsPressed((PlayerIndex)i);
                        if (buttonspressed.A == ButtonState.Pressed)
                        {
                            Vector3 newLocation = cameras.getLocation(i);
                            Vector3 newDirection = cameras.getDirection(i);

                            newLocation.Z += 1;
                            newDirection.Z += 1;
                            cameras.setLocation(i, newLocation);
                            cameras.setDirection(i, newDirection);
                        }
                        else if (buttonspressed.B == ButtonState.Pressed)
                        {
                            Vector3 newLocation = cameras.getLocation(i);
                            Vector3 newDirection = cameras.getDirection(i);

                            newLocation.Z -= 1;
                            newDirection.Z -= 1;
                            cameras.setLocation(i, newLocation);
                            cameras.setDirection(i, newDirection);
                        }
                    }
                }
            }
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (IsActive)
            {
                GraphicsDevice.Clear(Color.CornflowerBlue);
                for (int i = 0; i < cameras.LocalplayerCount; i++)
                {
                    GraphicsDevice.Viewport = cameras.getViewPort(i);
                    modela.Draw(Matrix.Identity, cameras.getView(i), cameras.getProjection(i));
                    //.Draw(Matrix.Identity, cameras.getView(i), cameras.getProjection(i));
                }
                GraphicsDevice.Viewport = cameras.defaultViewPort;
            }
            base.Draw(gameTime);
        }
    }
}
