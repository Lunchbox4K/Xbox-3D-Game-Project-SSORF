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
        string message;

        public MessageBox()
        {}

        public void setMessage(string Title, string Message)
        {
            title = Title;
            message = Message;
        }

        public void update()
        {

#if XBOX
            if (gamePadState.current.Buttons.A == ButtonState.Pressed)
                Active = false;
#else
            if (keyBoardState.current.IsKeyDown(Keys.Space) &&
                keyBoardState.previous.IsKeyUp(Keys.Space))
                Active = false;
#endif

        }

        public void draw(SpriteBatch spriteBatch, SpriteFont font, Color backgroundColor, Color fontColor)
        {
            spriteBatch.Draw(background, new Vector2(160, 180), backgroundColor);
            spriteBatch.DrawString(font, title, new Vector2(300, 200), fontColor);
            spriteBatch.DrawString(font, message, new Vector2(170, 270), fontColor);
        }

        public Texture2D Background
        {
            get { return background; }
            set { background = value; }
        }
    }
}
