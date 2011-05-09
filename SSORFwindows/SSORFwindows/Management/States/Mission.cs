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

        //was the mission completed successfully?
        private bool missionComplete = false;

        private MissionState state = MissionState.Starting;

        //used for 3..2..1..go
        TimeSpan countDown = new TimeSpan(0,0,4);
        TimeSpan timeLimit;

        private Objects.Vehicle scooter;

        private Objects.StaticModel Check;

        private Objects.ModelCollection CheckPoints;

        private Vector3[] CheckPointCoords;

        private short numCheckPoints = 0;

        private short currentCheckPoint = 0;

        private float checkPointYaw = 0.0f;
        
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

            Check = new Objects.StaticModel(content, "Models\\check",
                        Vector3.Zero, Matrix.Identity, Matrix.Identity);
            
            Check.LoadModel();

            //with missionID we can have a different starting positions, checkpoints, etc. for each mission
            //We need to load the data for each mission from file using the missionID
            if (missionID == 1)
            {
                numCheckPoints = 3;
                CheckPointCoords = new Vector3[numCheckPoints];
                timeLimit = new TimeSpan(0, 0, 10);

                CheckPointCoords[0] = new Vector3(140, 0, 0);
                CheckPointCoords[1] = new Vector3(0, 0, -140);
                CheckPointCoords[2] = new Vector3(0, 0, 140);

                CheckPoints = new Objects.ModelCollection(Check, numCheckPoints, CheckPointCoords);

                scooter.setStartingPosition(-0.45f, new Vector3(0, 0, 100), 0);
            }
            else if (missionID == 2)
            {
                numCheckPoints = 5;
                CheckPointCoords = new Vector3[numCheckPoints];
                timeLimit = new TimeSpan(0, 0, 15);

                CheckPointCoords[0] = new Vector3(0, 0, -140);
                CheckPointCoords[1] = new Vector3(140, 0, 0);
                CheckPointCoords[2] = new Vector3(0, 0, 140);
                CheckPointCoords[3] = new Vector3(-140, 0, 0);
                CheckPointCoords[4] = new Vector3(0, 0, -140);

                CheckPoints = new Objects.ModelCollection(Check, numCheckPoints, CheckPointCoords);


                scooter.setStartingPosition(0.0f, new Vector3(0,0,40), 0);
            }
            camera.update(scooter.Geometry.Location, scooter.Yaw);
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

                    checkPointYaw += 0.05f;
                    for (int i = currentCheckPoint; i < numCheckPoints; i++)
                        CheckPoints.Geometry.Orientation = Matrix.CreateRotationY(checkPointYaw);

                break;

                case MissionState.Paused :
                    //no updates while paused
#if XBOX
                    if (gamePadState.current.Buttons.Y == ButtonState.Pressed)
                        state = MissionState.Ending;
                    if (gamePadState.current.Buttons.Start == ButtonState.Pressed && 
                        gamePadState.current.Buttons.Start == ButtonState.Released) 
                        state = MissionState.Paused;
#else
                if (keyBoardState.current.IsKeyDown(Keys.Q))
                        state = MissionState.Ending;
                    if (keyBoardState.current.IsKeyDown(Keys.Enter) &&
                        keyBoardState.previous.IsKeyUp(Keys.Enter))
                        state = MissionState.Playing;     
#endif
                break;

                //if we are playing update scooter/camera using player input
                case MissionState.Playing :
#if WINDOWS
                float tVal = 0;
                float bVal = 0;
                float sVal = 0;
                if (keyBoardState.current.IsKeyDown(Keys.Up))
                    tVal = 1;
                if (keyBoardState.current.IsKeyDown(Keys.Down))
                    bVal = 1;
                if (keyBoardState.current.IsKeyDown(Keys.Left))
                    sVal = 1;
                if (keyBoardState.current.IsKeyDown(Keys.Right))
                    sVal = -1;
                
                scooter.update(gameTime, sVal, tVal, bVal);
#endif
#if XBOX
                scooter.update(gameTime, gamePadState.current.ThumbSticks.Left.X, gamePadState.current.Triggers.Right, gamePadState.current.Triggers.Left);
#endif
                    camera.update(scooter.Geometry.Location, scooter.Yaw);


                    timeLimit -= gameTime.ElapsedGameTime;
                    if (timeLimit.Milliseconds < 0)
                        state = MissionState.Ending;

                    checkPointYaw += 0.05f;
                    for (int i = currentCheckPoint; i < numCheckPoints; i++)
                        CheckPoints.Geometry.Orientation = Matrix.CreateRotationY(checkPointYaw);

                    //Collision Detection

                    //bounding box did not work!

                    
                    if (CheckPoints.CheckCollision(currentCheckPoint, scooter.Geometry))
                        currentCheckPoint += 1;
                    

                    if (currentCheckPoint == numCheckPoints)
                    { 
                        missionComplete = true;
                        state = MissionState.Ending;
                    }

#if XBOX
                    if (gamePadState.current.Buttons.Start == ButtonState.Pressed && 
                        gamePadState.current.Buttons.Start == ButtonState.Released)  
                        state = MissionState.Paused;
#else

                    if (keyBoardState.current.IsKeyDown(Keys.Enter) && 
                        keyBoardState.previous.IsKeyUp(Keys.Enter))
                        state = MissionState.Paused;     
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

        public void draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            geometry.draw(camera);

            scooter.Geometry.drawModel(gameTime, camera.ViewMtx, camera.ProjMtx);

            if(currentCheckPoint == numCheckPoints - 1)
                CheckPoints.draw(gameTime, camera, currentCheckPoint, (short)(currentCheckPoint + 1));
            else
               CheckPoints.draw(gameTime, camera, currentCheckPoint, (short)(currentCheckPoint + 1));

           

            
            //These setting allow us to print strings without screwing with the
            //3D rendering, but Carl told me it won't work with transparent objects

            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, 
                SamplerState.AnisotropicClamp, DepthStencilState.Default, 
                RasterizerState.CullCounterClockwise);

            

            //display camera and scooter coordinates for testing

            //spriteBatch.DrawString(smallFont, "Scooter coordinates: " + scooter.Geometry.Location.ToString(), new Vector2(10, 10), Color.Black);
            //spriteBatch.DrawString(smallFont, "Camera coordinates:  " + camera.Position.ToString(), new Vector2(10, 30), Color.Black);
            
            //This #if #else will change the on screen instructions when deployed on XBOX
#if XBOX
            char endKey = 'Y';
            string returnKey = "START";
#else
            char endKey = 'Q';
            string returnKey = "ENTER";
#endif
            //This switch statement prints different instructions depending on MissionState
            switch (state)
            { 
                case MissionState.Starting :
                    spriteBatch.DrawString(smallFont, "Time Left: " + timeLimit.TotalSeconds.ToString(), new Vector2(200, 10), Color.Black);
                    spriteBatch.DrawString(smallFont, "Get ready to race!!!", new Vector2(300, 550), Color.Black);
                    if (countDown.Seconds > 0)
                        spriteBatch.DrawString(largeFont, countDown.Seconds.ToString(), new Vector2(340, 100), Color.Black);
                    else
                        spriteBatch.DrawString(largeFont, "GO!", new Vector2(220, 100), Color.Black);
                break;

                case MissionState.Paused:
                    spriteBatch.DrawString(smallFont, "Time Left: " + timeLimit.TotalSeconds.ToString(), new Vector2(200, 10), Color.Black);
                    spriteBatch.DrawString(largeFont, "paused", new Vector2(80, 100), Color.Black);
                    spriteBatch.DrawString(smallFont, "Press [" + endKey + "] to quit mission", new Vector2(280, 530), Color.Black);
                    spriteBatch.DrawString(smallFont, "Press [" + returnKey + "] to return to mission", new Vector2(280, 550), Color.Black);
                break;

                case MissionState.Playing :
                    spriteBatch.DrawString(smallFont, "Time Left: " + timeLimit.TotalSeconds.ToString(), new Vector2(200, 10), Color.Black);
                    spriteBatch.DrawString(smallFont, "Press [" + returnKey + "] to pause mission", new Vector2(280, 550), Color.Black);
                break;

                case MissionState.Ending :
                if (missionComplete)
                {
                    spriteBatch.DrawString(largeFont, "Finish!", new Vector2(60, 100), Color.Black);
                    spriteBatch.DrawString(smallFont, "With " + timeLimit.TotalSeconds.ToString() + " seconds to spare!!!", new Vector2(200, 450), Color.Black);
                }
                else
                    spriteBatch.DrawString(largeFont, "Fail!", new Vector2(200, 100), Color.Black);

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
