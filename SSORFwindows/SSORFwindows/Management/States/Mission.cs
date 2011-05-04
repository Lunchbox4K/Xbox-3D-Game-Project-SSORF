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

//Mission class needs checkpoints and levelObjects
//also needs a timer displayed to show the elapsed time

namespace SSORF.Management.States
{
    //Starting is used for 3..2..1..go and Ending would 
    //normally be used when all checkpoints are cleared

    public enum MissionState
    { Starting, Playing, Ending, Paused }

    class Mission
    {
        public bool Active = true;

        private MissionState state = MissionState.Starting;

        //used for 3..2..1..go
        TimeSpan countDown = new TimeSpan(0,0,4);

        private Objects.Vehicle scooter;
        
        private Objects.SimpleModel geometry;

        Objects.ThirdPersonCamera camera = new Objects.ThirdPersonCamera();

        //One checkpoint for now...
        

        //use these fonts to print strings
        private SpriteFont largeFont;
        private SpriteFont smallFont;

        //empty constructor for making missions without parameters
        public Mission()
        { }

        //when scooter selection menu is implemented we will pass in 
        //the chosen vehicle to the Mission constructor
        public Mission(Objects.Vehicle selectedScooter, float aspectRatio)
        {
            scooter = selectedScooter;
            camera.ProjMtx = Matrix.CreatePerspectiveFieldOfView(
                            MathHelper.ToRadians(45.0f),
                            aspectRatio, 1.0f, 1000.0f);
        }

        //missionId can be used to load checkpoint coordinates for that mission
        //from a file, as well as the filenames/locations of levelObjects, etc.
        public void load(ContentManager content, short missionID)
        {
            //load fonts
            largeFont = content.Load<SpriteFont>("missionFont");
            smallFont = content.Load<SpriteFont>("font");
            //use IDnum to load the correct content
            geometry = new Objects.SimpleModel();
            geometry.Mesh = content.Load<Model>("Models\\level" + missionID.ToString());
            //with IDnum we could also have a different starting positions for each mission
            scooter.Position = Vector3.Zero;
            scooter.Yaw = 0.0f;
        }

        public void update(GameTime gameTime)
        {
            //What we update depends on MissionState
            switch (state)
            { 
                //when starting update countdown
                case MissionState.Starting :
                    countDown -= gameTime.ElapsedGameTime;
                    if (countDown.Milliseconds < 0)
                        state = MissionState.Playing;
                break;

                //if we are playing update scooter/camera using player input
                case MissionState.Playing :
                    scooter.update(gameTime);
                    camera.update(scooter.Position, scooter.Yaw);
#if XBOX
                    if (gamePadState.current.Buttons.Y == ButtonState.Pressed)
                        state = MissionState.Ending;
#else
                    if (keyBoardState.current.IsKeyDown(Keys.E))
                        state = MissionState.Ending;
#endif
                break;

                //If mission has ended wait for user to confirm returning to menu
                case MissionState.Ending :
#if XBOX
                    if (gamePadState.current.Buttons.Start == ButtonState.Pressed)  
                        Active = false;
#else
                    if (keyBoardState.current.IsKeyDown(Keys.Enter))  
                        Active = false;               
#endif
                break;
            }

        }

        public void draw(SpriteBatch spriteBatch)
        {
            geometry.draw(camera);

            scooter.Geometry.draw(camera);

            
            //These setting allow us to print strings without screwing with the
            //3D rendering, but Carl told me it won't work with transparent objects
            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, 
                SamplerState.AnisotropicClamp, DepthStencilState.Default, 
                RasterizerState.CullCounterClockwise);

            //display camera and scooter coordinates for testing
            spriteBatch.DrawString(smallFont, "Scooter coordinates: " + scooter.Position.ToString(), new Vector2(10, 10), Color.Black);
            spriteBatch.DrawString(smallFont, "Camera coordinates:  " + camera.Position.ToString(), new Vector2(10, 30), Color.Black);
            
            //This #if #else will change the on screen instructions when deployed on XBOX
#if XBOX
            char endKey = 'Y';
            string returnKey = "START";
#else
            char endKey = 'E';
            string returnKey = "ENTER";
#endif
            //This switch statement prints different instructions depending on MissionState
            switch (state)
            { 
                case MissionState.Starting :
                    spriteBatch.DrawString(smallFont, "Get ready to race!!!", new Vector2(300, 550), Color.Black);
                    if (countDown.Seconds > 0)
                        spriteBatch.DrawString(largeFont, countDown.Seconds.ToString(), new Vector2(340, 100), Color.Black);
                    else
                        spriteBatch.DrawString(largeFont, "GO!", new Vector2(220, 100), Color.Black);
                break;

                case MissionState.Playing :
                    spriteBatch.DrawString(smallFont, "Press [" + endKey + "] to end mission", new Vector2(280, 550), Color.Black);
                break;

                case MissionState.Ending :
                    spriteBatch.DrawString(largeFont, "Finish!", new Vector2(60, 100), Color.Black);
                    spriteBatch.DrawString(smallFont, "Press [" + returnKey + "] to return to menu", new Vector2(240, 550), Color.Black);
                break;
            
            }

            spriteBatch.End();

        }

        //accessors and mutators
        public Objects.SimpleModel Geometry { get { return geometry; } set { geometry = value; } }

        public Objects.ThirdPersonCamera Camera { get { return camera; } 
            set { camera = value; } }
    }
}
