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
        string title;
        string message1;
        string message2;

        public MessageBox()
        {}

        public void setMessage(string Title, string Message1)
        {
            title = Title;
            message1 = Message1;
            message2 = string.Empty;
        }
        public void setMessage(string Title, string Message1, string Message2)
        {
            title = Title;
            message1 = Message1;
            message2 = Message2;
        }

        public void update()
        {

#if XBOX
            if (gamePadState.current.Buttons.A == ButtonState.Pressed)
                active = false;
#else
            if (keyBoardState.current.IsKeyDown(Keys.Space) &&
                keyBoardState.previous.IsKeyUp(Keys.Space))
                Active = false;
#endif

        }

        public void draw(SpriteBatch spriteBatch, SpriteFont font, Color backgroundColor, Color fontColor)
        {
            spriteBatch.Draw(background, new Vector2(300, 200), backgroundColor);
            spriteBatch.DrawString(font, title, new Vector2(320, 210), fontColor);
            spriteBatch.DrawString(font, message1, new Vector2(320, 280), fontColor);
            spriteBatch.DrawString(font, message2, new Vector2(320, 300), fontColor);
        }

        public Texture2D Background
        {
            get { return background; }
            set { background = value; }
        }
    }
}
