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

namespace SSORF.Objects
{
    class MessageBox
    {
        public bool Active = false;
        Texture2D background;
        string message;

        public MessageBox()
        {}

        public void setMessage(string Message)
        {
            message = Message;
        }

        public void update()
        {

#if XBOX
            if (gamePadState.current.Buttons.A == ButtonState.Pressed &&
                gamePadState.previous.Buttons.A == ButtonState.Released)
                Active = false;
#else
            if (keyBoardState.current.IsKeyDown(Keys.Space) &&
                keyBoardState.previous.IsKeyUp(Keys.Space))
                Active = false;
#endif

        }

        public void draw(SpriteBatch spriteBatch, SpriteFont font, Color backgroundColor, Color fontColor)
        {
            Rectangle screen = SSORF.Management.StateManager.bounds;
            spriteBatch.Draw(background, new Vector2(screen.Left + 160, screen.Top + 180), backgroundColor);
            spriteBatch.DrawString(font, message, new Vector2(screen.Left + 170, screen.Top + 210), fontColor);

            string button;
#if XBOX
            
            button = "A";
#else
            button = "SPACE";
            
#endif
            spriteBatch.DrawString(font, "Pess [" + button + "] to continue", new Vector2(screen.Left + 170, screen.Top + 270), fontColor);
        }

        public Texture2D Background
        {
            get { return background; }
            set { background = value; }
        }
    }
}
