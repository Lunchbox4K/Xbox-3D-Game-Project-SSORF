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

//Should be modified to display a production team logo 
//for a few seconds before the title screen comes up;

namespace SSORF.Management.States
{
    class Title
    {
        public bool Active = true;
        private Texture2D image;
        private double endTime;

        public Texture2D Image { get {return image;} set {image = value;} }

        public void update(GameTime gameTime)
        {
            //Set time for title screen to expire
            if (endTime == 0)
                endTime = gameTime.TotalGameTime.TotalSeconds + 3.0; 

            //Check if title screen has expired
            if (gameTime.TotalGameTime.TotalSeconds > endTime)
                Active = false;

            //If player pushes start on controller or enter
            //on keyboard, kill the title screen...
#if XBOX
               if (gamePadState.current.Buttons.Start == ButtonState.Pressed)
                Active = false;
# else
            if (keyBoardState.current.IsKeyDown(Keys.Enter))
                Active = false;
#endif

        }

        //draw title
        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(image, Vector2.Zero, Color.White);
            spriteBatch.End();
        }

    }

    
}
