using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ssorf.Management
{
    public class GameStateManager
    {
        /// <summary>
        /// All of the game states that are active
        /// </summary>
        public static List<Base.GameState> GameStates = new List<Base.GameState>();
        /// <summary>
        /// The current game state
        /// </summary>
        private static Base.GameState currentState = null;
        /// <summary>
        /// The next state of the gamestate manager
        /// </summary>
        private static Base.GameState nextState = null;

        /// <summary>
        /// The 0 -> 2 number that slides the frame from current 0 to black 1 to full next 2,
        /// Then the states are switched
        /// </summary>
        private static float SlideAmount = 0.0f;
        /// <summary>
        /// The slide speed in % per second, The transition from Current To Next Will Take [x] seconds in total 
        /// </summary>
        private static float SlideSpeedAmount = 1f;

        private int GAME_WIDTH;
        private int GAME_HEIGHT;

        Texture2D blackCube;
        public static SpriteBatch sb;

        public GameStateManager(GraphicsDevice graphics, int width, int height)
        {
            GAME_WIDTH = width;
            GAME_HEIGHT = height;

            blackCube = new Texture2D(graphics, 4, 4, false, SurfaceFormat.Color);
            Color[] colors = new Color[16];
            for (int x = 0; x < 16; x++)
            {
                colors[x] = Color.Black;
            }
            blackCube.SetData<Color>(colors);

            sb = new SpriteBatch(graphics);

        }
        public void Initialize()
        {
            if (currentState != null)
                currentState.Initialize();
        }
        public void Update(GameTime gameTime)
        {
            if (nextState != null)
            {
                nextState.OnStateReset();
                nextState.Update(gameTime);
            }
            else
            {
                currentState.Update(gameTime);
            }

        }
        public void Draw(GameTime gameTime)
        {
            Color c = Color.Black;

            if (nextState != null)
            {
                if (SlideAmount <= 1.0)
                {
                    currentState.Draw(gameTime);
                    c.A = (byte)(SlideAmount * 255);
                    sb.Begin();
                    sb.Draw(blackCube, new Rectangle(0, 0, GAME_WIDTH, GAME_HEIGHT), c);
                    sb.End();
                }
                else
                {
                    nextState.Draw(gameTime);
                    c.A = (byte)(255 - ((SlideAmount * 255)));
                    sb.Begin();
                    sb.Draw(blackCube, new Rectangle(0, 0, GAME_WIDTH, GAME_HEIGHT), c);
                    sb.End();
                }
                SlideAmount += (float)((SlideSpeedAmount * 2) * gameTime.ElapsedGameTime.TotalMilliseconds);
                if (SlideAmount >= 2)
                {
                    SlideAmount = 0;
                    currentState = nextState;
                    nextState = null;
                }
            }
            else
            {
                currentState.Draw(gameTime);
            }
        }
        
        public static void ChangeStateTo(byte State)
        {
            if (nextState != null)
            {
                // We are in a transition right now, CHILL!
                return;
            }
            if (currentState == null)
            {
                Base.GameState temp = findStateWithName(State);
                if (temp != null)
                {
                    currentState = temp;
                    currentState.OnStateReset();
                }
                else
                {
                    throw new Exception("Could Not Find State " + nextState);
                }
            }
            else
            {
                // We are running a state, we need to switch
                Base.GameState temp = findStateWithName(State);
                if (temp != null)
                {
                    nextState = temp;
                    nextState.Initialize();
                }
                else
                {
                    throw new Exception("Could Not Find State " + nextState);
                }
            }
        }

        public static Base.GameState findStateWithName(byte state)
        {
            foreach (Base.GameState gs in GameStates)
            {
                if (gs.stateID == state)
                {
                    return gs;
                }
            }
            return null;
        }

        public static void LoadStates()
        {
            foreach (Base.GameState gs in GameStates)
            {
                gs.LoadContent();
            }
        }
        public static void UnloadStates()
        {
            foreach (Base.GameState gs in GameStates)
            {
                gs.UnloadContent();
            }
        }
    }
}
