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

//used to encapsulate one menu screen

//We may need another class or two for different types of menus
//but that is up to the menu programmers
namespace SSORF.Management.States
{
    class SubMenu
    {

        private Texture2D backGround;
        private Vector2 cursorPosition;
        //Offset is difference between button position and cursor position
        //so the cursor does not completely overlap the button
        private Vector2 cursorOffset;
        //which button is the cursor hovering over?
        public short selectedButton = 1;
        //if buttonPressed equals zero, no button has been pressed
        public short buttonPressed = 0;
        public short numButtons;
        //could use a button class for these two arrays instead but may not be necessary
        private Texture2D[] buttonImage;
        private Vector2[] buttonPosition;
        public bool[] buttonHighlight;

        public SubMenu(short NumButtons)
        {
            Rectangle screen = SSORF.Management.StateManager.notbounds;
            cursorOffset = new Vector2(screen.Left + -30, screen.Top + 25);
            numButtons = NumButtons;
            buttonImage = new Texture2D[numButtons];
            buttonPosition = new Vector2[numButtons];
            buttonHighlight = new bool[numButtons];
        }

        public void update(GameTime gameTime)
        {

            //If xbox use the D-Pad to navigate menus
#if XBOX
            if (gamePadState.current.DPad.Right == ButtonState.Pressed &&
                gamePadState.previous.DPad.Right == ButtonState.Released)
            {
                if (selectedButton == 1)
                    selectedButton = numButtons;
                else
                    selectedButton -= 1;
                AudioManager.playSound(AudioManager.CLICK_CUE);
            }

            if (gamePadState.current.DPad.Left == ButtonState.Pressed &&
                gamePadState.previous.DPad.Left == ButtonState.Released)
            {
                if (selectedButton == numButtons)
                    selectedButton = 1;
                else
                    selectedButton += 1;
                AudioManager.playSound(AudioManager.CLICK_CUE);
            }

            if (gamePadState.current.Buttons.A == ButtonState.Pressed && gamePadState.previous.Buttons.A == ButtonState.Released)
            {
                buttonPressed = selectedButton;
                AudioManager.playSound(AudioManager.CLICK_CUE);
            }
#else
            //if we are at the last button go back to the first
            if (keyBoardState.current.IsKeyDown(Keys.Left) &&
                keyBoardState.previous.IsKeyUp(Keys.Left))
            {
                if (selectedButton == 1)
                    selectedButton = numButtons;
                else
                    selectedButton -= 1;
                AudioManager.playSound(AudioManager.CLICK_CUE);
            }

            //if we are at the first button we can jump to the last one
            if (keyBoardState.current.IsKeyDown(Keys.Right) &&
                keyBoardState.previous.IsKeyUp(Keys.Right))
            {
                if (selectedButton == numButtons)
                    selectedButton = 1;
                else
                    selectedButton += 1;
                AudioManager.playSound(AudioManager.CLICK_CUE);
            }

            //if we are at the last button go back to the first
            if (keyBoardState.current.IsKeyDown(Keys.Up) &&
                keyBoardState.previous.IsKeyUp(Keys.Up))
            {
                if (selectedButton == 1)
                    selectedButton = numButtons;
                else
                    selectedButton -= 1;
                AudioManager.playSound(AudioManager.CLICK_CUE);
            }

            //if we are at the first button we can jump to the last one
            if (keyBoardState.current.IsKeyDown(Keys.Down) &&
                keyBoardState.previous.IsKeyUp(Keys.Down))
            {
                if (selectedButton == numButtons)
                    selectedButton = 1;
                else
                    selectedButton += 1;
                AudioManager.playSound(AudioManager.CLICK_CUE);
            }

            //space bar will activate the button
            if (keyBoardState.previous.IsKeyUp(Keys.Space) && keyBoardState.current.IsKeyDown(Keys.Space))
            {
                buttonPressed = selectedButton;
                AudioManager.playSound(AudioManager.CLICK_CUE);
            }
#endif
            //update the cursor position so it is to the left of the button
            updateCursor();

        }
         public void updateCursor()
         {CursorPosition = buttonPosition[selectedButton - 1] + cursorOffset;}

        //Draw backgound and buttons
        public void draw(SpriteBatch spriteBatch, Matrix scale)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, scale);
            spriteBatch.Draw(backGround, Vector2.Zero, Color.White);
            spriteBatch.End();
            spriteBatch.Begin();
            for (int i = 0; i < numButtons; i++) 
                    spriteBatch.Draw(buttonImage[i], buttonPosition[i], Color.White);
            spriteBatch.End();
        }

        public void drawWithHighlights(SpriteBatch spriteBatch, Matrix scale)
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, scale);
            spriteBatch.Draw(backGround, Vector2.Zero, Color.White);
            spriteBatch.End();
            spriteBatch.Begin();
            for (int i = 0; i < numButtons; i++)
                if (buttonHighlight[i] == true)
                    spriteBatch.Draw(buttonImage[i], buttonPosition[i], Color.Gray);
                else
                    spriteBatch.Draw(buttonImage[i], buttonPosition[i], Color.Black);
            spriteBatch.End();
        }



        //Accessors and mutators
        public Texture2D BackGround { get { return backGround; } set { backGround = value; } }

        public Vector2 CursorPosition { get { return cursorPosition; } set { cursorPosition = value; } }

        public Texture2D[] ButtonImage { get { return buttonImage; } set { buttonImage = value; } }

        public Vector2[] ButtonPosition { get { return buttonPosition; } set { buttonPosition = value; } }

        public short SelectedButton { get { return selectedButton; } set { selectedButton = value; } }
    }
}
