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
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Input : Microsoft.Xna.Framework.GameComponent
    {
        public const int MAX_GAME_PADS = 4;

        #region Members

        private KeyboardState keyState;
        private KeyboardState prevKeyState;
        private GamePadState[] padState;
        private GamePadState[] prevPadState;
        private PlayerIndex padCount;

        #endregion

        #region Constructor & Initialize

        public Input(Game game, PlayerIndex numberOfGamePads)
            : base(game)
        {
            padCount = numberOfGamePads;
        }



        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            //Create a new array of game pad states for each player
            padState = new GamePadState[(int)padCount + 1];
            prevPadState = new GamePadState[(int)padCount + 1];
            base.Initialize();
        }

        #endregion

        #region Update

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            //Get old keyboard state
            prevKeyState = keyState;
            //Get old controller state
            prevPadState = padState;
            //Update new keyboard state
            keyState = Keyboard.GetState();
            //Update new gamepad state
            for (int i = 0; i < (int)padCount + 1; i++)
            {
                padState[i] = GamePad.GetState((PlayerIndex)i);
            }
            base.Update(gameTime);
        }

        #endregion

        #region Accesors Mutators

        /// <summary>
        /// Gets the new keys pressed
        /// </summary>
        public Keys[] PressedKeys
        {
            get {
                return keyState.GetPressedKeys();
            }
        }
        /// <summary>
        /// Gets the new keys pressed
        /// </summary>
        public Keys[] OldPressedKeys
        {
            get
            {
                return prevKeyState.GetPressedKeys();
            }
        }
        /// <summary>
        /// Returns an array[] of new GamePadStates
        /// </summary>
        public GamePadState[] getPadStates
        {
            get {
                return padState;
            }
        }
        /// <summary>
        /// Returns an array[] of old GamePadStates
        /// </summary>
        public GamePadState[] getOldPadStates
        {
            get
            {
                return prevPadState;
            }
        }
        /// <summary>
        /// Returns the new buttons pressed for a specific player
        /// </summary>
        /// <param name="player">PlayerIndex - Player to get input from</param>
        /// <returns></returns>
        public GamePadButtons GetButtonsPressed(PlayerIndex player)
        {
            if (player > padCount)
                throw new IndexOutOfRangeException
                    ("Not enough controllers registered!");
            GamePadButtons pressedbuttons;
            pressedbuttons = padState[((int)player)].Buttons;

            return pressedbuttons;
        }
        /// <summary>
        /// Returns the old buttons pressed for a specific player
        /// </summary>
        /// <param name="player">PlayerIndex - Player to get input from</param>
        /// <returns></returns>
        public GamePadButtons GetOldButtonsPressed(PlayerIndex player)
        {
            if (player > padCount)
                throw new IndexOutOfRangeException
                    ("Not enough controllers registered!");
            GamePadButtons pressedbuttons;
            pressedbuttons = prevPadState[(int)player].Buttons;

            return pressedbuttons;
        }

        #endregion
    }
}
