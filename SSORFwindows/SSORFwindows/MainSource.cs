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

//Main source code doesn't do much except update input and exit game.
//StateManager is the only game component so it's LoadContent(), Update(),
//and Draw() methods will be called automatically

namespace SSORF
{
    //these 2 global classes make player input available to all the other classes
    public static class keyBoardState
    {
        public static KeyboardState previous = Keyboard.GetState();
        public static KeyboardState current;
    }

    public static class gamePadState
    {
        public static GamePadState previous = GamePad.GetState(PlayerIndex.One);
        public static GamePadState current;
    }

    public class MainSource : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Management.StateManager stateManager;

        public MainSource()
        {
            Content.RootDirectory = "Content";

            //set preferred screen dimensions...
            graphics = new GraphicsDeviceManager(this);
#if WINDOWS
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            graphics.IsFullScreen = false;
#elif XBOX
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 1024;
#endif


            stateManager = new Management.StateManager(this);

            Components.Add(stateManager);
        }

        //Do we need to use this? I'm not sure
        protected override void Initialize()
        {
            base.Initialize();
        }

        //StateManager.LoadContent() is called here
        protected override void LoadContent()
        {

        }

        //Do we need to use this? I'm not sure
        protected override void UnloadContent()
        {

        }

        //calls stateManager.Update() automatically and checks to see if exiting game
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
#if XBOX
            if (gamePadState.current.Buttons.Back == ButtonState.Pressed)
                this.Exit();
#else
            if (keyBoardState.current.IsKeyDown(Keys.Escape))
                this.Exit();
#endif
            keyBoardState.previous = keyBoardState.current;
            base.Update(gameTime);

            
        }

        //Clears screen and automatically calls stateManager.Draw()
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }
    }
}
