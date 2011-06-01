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
    class fpsCalculator
    {
        private TimeSpan secondCounter;
        private int frameCounter;
        private int fps;

        public fpsCalculator()
        {
            secondCounter = TimeSpan.Zero;
            frameCounter = 0;
            fps = 0;
        }

        public void update(GameTime gameTime)
        {
            secondCounter += gameTime.ElapsedGameTime;
            if (secondCounter.Seconds >= 1)
            {
                secondCounter = TimeSpan.Zero;
                fps = frameCounter;
                frameCounter = 0;
            }
        }
        public void draw(GameTime gameTime)
        {
            frameCounter++;
        }
        public int FPS
        {
            get { return fps; }
        }
    }
}
