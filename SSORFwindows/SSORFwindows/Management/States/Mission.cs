using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        #region members

        private bool isLoaded = false;

        public Game rootGame; 
        public bool Active = true;
        Objects.Player player;
        //was the mission completed successfully?
        private bool missionComplete = false;
        private MissionState state = MissionState.Starting;
        //used for 3..2..1..go
        TimeSpan countDown = new TimeSpan(0,0,4);
        TimeSpan timeLimit;
        int prizeMoney;
        private Objects.Vehicle scooter = new Objects.Vehicle();
        private Objects.StaticModel Check;
        private Objects.ModelCollection CheckPoints;
        private Vector3[] CheckPointCoords;
        private short numCheckPoints = 0;
        private short currentCheckPoint = 0;
        private float checkPointYaw = 0.0f;
        Objects.ThirdPersonCamera camera = new Objects.ThirdPersonCamera();
#if XBOX
        bool gamepadInUse = true;
#elif WINDOWS
        bool gamepadInUse = false;
#endif

        //use these fonts to print strings
        private SpriteFont largeFont;
        private SpriteFont smallFont;
        //Level
        SSORFlibrary.LevelLayout levelProperties;
        SSORF.Objects.Level level;

        SSORF.Objects.fpsCalculator fps;
        SSORF.Objects.CollisionDetection collisions;
        SSORF.Objects.Collision[] collisionList;
        List<SSORF.Objects.StaticModel> playerModels;
        //string debugMessage = "";

        private Rectangle bounds;
        #endregion

        //empty constructor for making missions without parameters
        public Mission()
        { }

        public Mission(Objects.Player playerInfo, SSORFlibrary.ScooterData ScooterSpecs, Game game, short missionID)
        {
            fps = new Objects.fpsCalculator();
            player = playerInfo;
            rootGame = game;
            scooter.load(game.Content, ScooterSpecs, player.UpgradeTotals[ScooterSpecs.IDnum]);
            playerModels = new List<Objects.StaticModel>();
            playerModels.Add(scooter.Geometry);
            camera.ProjMtx = Matrix.CreatePerspectiveFieldOfView(
                            MathHelper.ToRadians(45.0f),
                            game.GraphicsDevice.Viewport.AspectRatio, 1.0f, 2000.0f);
            //Set Level
            levelProperties = new SSORFlibrary.LevelLayout();
            levelProperties.instanced_models = new List<SSORFlibrary.LocationMapAsset>();
            levelProperties.statics_models = new List<SSORFlibrary.LocationMapAsset>();

            #region missions 1-4 use level 1 (round racetrack)
            if (missionID < 5)
            {
                prizeMoney = missionID * 50;

                SSORFlibrary.LocationMapAsset tree = new SSORFlibrary.LocationMapAsset();
                tree.asset_colorID = 130;
                tree.asset_location = "Models\\tree";
                levelProperties.instanced_models.Add(tree);

                SSORFlibrary.LocationMapAsset tirepile = new SSORFlibrary.LocationMapAsset();
                tirepile.asset_colorID = 110;
                tirepile.asset_location = "Models\\tirepile";
                levelProperties.statics_models.Add(tirepile);

                SSORFlibrary.LocationMapAsset bush = new SSORFlibrary.LocationMapAsset();
                bush.asset_colorID = 20;
                bush.asset_location = "Models\\bush";
                levelProperties.instanced_models.Add(bush);

                levelProperties.level_heightMap = "Images\\Terrain\\lvl1_hm";
                levelProperties.level_textureB = "Images\\Terrain\\terrainTextureB";
                levelProperties.level_textureG = "Images\\Terrain\\terrainTextureG";
                levelProperties.level_textureMap = "Images\\Terrain\\lvl1_cm";
                levelProperties.level_textureR = "Images\\Terrain\\terrainTextureR";
            }
            #endregion

            #region missions 5-8 use level 2 (park)
            else if (missionID < 9)
            {
                prizeMoney = missionID * 100;

                SSORFlibrary.LocationMapAsset tree = new SSORFlibrary.LocationMapAsset();
                tree.asset_colorID = 130;
                tree.asset_location = "Models\\tree";
                levelProperties.instanced_models.Add(tree);

                SSORFlibrary.LocationMapAsset bush = new SSORFlibrary.LocationMapAsset();
                bush.asset_colorID = 20;
                bush.asset_location = "Models\\bush";
                levelProperties.instanced_models.Add(bush);

                SSORFlibrary.LocationMapAsset bench = new SSORFlibrary.LocationMapAsset();
                bench.asset_colorID = 30;
                bench.asset_location = "Models\\bench";
                levelProperties.instanced_models.Add(bench);

                levelProperties.level_heightMap = "Images\\Terrain\\lvl2_hm";
                levelProperties.level_textureB = "Images\\Terrain\\asphalt2";
                levelProperties.level_textureG = "Images\\Terrain\\terrainTextureG";
                levelProperties.level_textureMap = "Images\\Terrain\\lvl2_cm";
                levelProperties.level_textureR = "Images\\Terrain\\terrainTextureR";
            }
            #endregion

            #region missions 9-12 use level 3 (parking lot)
            else if (missionID < 13)
            {
                prizeMoney = missionID * 150;

                SSORFlibrary.LocationMapAsset handicap = new SSORFlibrary.LocationMapAsset();
                handicap.asset_colorID = 120;
                handicap.asset_location = "Models\\handicapsign";
                levelProperties.instanced_models.Add(handicap);

                //SSORFlibrary.LocationMapAsset can = new SSORFlibrary.LocationMapAsset();
                //can.asset_colorID = 40;
                //can.asset_location = "Models\\garbagecan";
                //levelProperties.instanced_models.Add(can);

                //SSORFlibrary.LocationMapAsset storesign = new SSORFlibrary.LocationMapAsset();
                //storesign.asset_colorID = 100;
                //storesign.asset_location = "Models\\storesign";
                //levelProperties.instanced_models.Add(storesign);

                SSORFlibrary.LocationMapAsset cart = new SSORFlibrary.LocationMapAsset();
                cart.asset_colorID = 60;
                cart.asset_location = "Models\\shoppingcart";
                levelProperties.instanced_models.Add(cart);

                //SSORFlibrary.LocationMapAsset store = new SSORFlibrary.LocationMapAsset();
                //store.asset_colorID = 90;
                //store.asset_location = "Models\\storefront";
                //levelProperties.instanced_models.Add(store);

                SSORFlibrary.LocationMapAsset car = new SSORFlibrary.LocationMapAsset();
                car.asset_colorID = 50;
                car.asset_location = "Models\\car1";
                levelProperties.statics_models.Add(car);

                levelProperties.level_heightMap = "Images\\Terrain\\lvl3_hm";
                levelProperties.level_textureB = "Images\\Terrain\\asphalt2";
                levelProperties.level_textureG = "Images\\Terrain\\terrainTextureG";
                levelProperties.level_textureMap = "Images\\Terrain\\lvl3_cm";
                levelProperties.level_textureR = "Images\\Terrain\\terrainTextureR";
            }
            #endregion

            #region missions 13-16 use level 4 (large racetrack)
            else
            {
                prizeMoney = missionID * 200;

                SSORFlibrary.LocationMapAsset tree = new SSORFlibrary.LocationMapAsset();
                tree.asset_colorID = 130;
                tree.asset_location = "Models\\tree";
                levelProperties.instanced_models.Add(tree);

                SSORFlibrary.LocationMapAsset tirepile = new SSORFlibrary.LocationMapAsset();
                tirepile.asset_colorID = 110;
                tirepile.asset_location = "Models\\tirepile";
                levelProperties.statics_models.Add(tirepile);

                SSORFlibrary.LocationMapAsset bush = new SSORFlibrary.LocationMapAsset();
                bush.asset_colorID = 30;
                bush.asset_location = "Models\\bush";
                levelProperties.instanced_models.Add(bush);

                levelProperties.level_heightMap = "Images\\Terrain\\lvl4_hm";
                levelProperties.level_textureB = "Images\\Terrain\\terrainTextureB";
                levelProperties.level_textureG = "Images\\Terrain\\terrainTextureG";
                levelProperties.level_textureMap = "Images\\Terrain\\lvl4_cm";
                levelProperties.level_textureR = "Images\\Terrain\\terrainTextureR";
            }
            #endregion

            levelProperties.instances_locationMap = "Images\\Terrain\\lvl"+missionID.ToString()+"_mm";
            levelProperties.level_effect = "Effects\\TerrainTextureEffect";
            levelProperties.viewTree_refreshRate = 8;
            level = new Objects.Level(game, levelProperties);

            //Initialize the variable in the constructor
            collisions = new Objects.CollisionDetection();
            collisions.setPlayerModels(playerModels);
        }

        //missionId can be used to load checkpoint coordinates for that mission
        //from a file, as well as the filenames/locations of levelObjects, etc.
        public void load(ContentManager content, short missionID)
        {
            //load fonts
            largeFont = content.Load<SpriteFont>("missionFont");
            smallFont = content.Load<SpriteFont>("font");
            //use IDnum to load the correct content
            //geometry = new Objects.SimpleModel();
            //geometry.Mesh = content.Load<Model>("Models\\level" + missionID.ToString());

            //Load Level
            level.LoadContent();

            //Grabs the raw static and instanced model data from the level for processing
            // Will probably grab a list of checkpoints if the list is created in the level class
            collisions.setModels(level.StaticModels, level.InstancedModels, level.ModelInstances);
           

            Check = new Objects.StaticModel(content, "Models\\check",
                        Vector3.Zero, Matrix.Identity, 1f);
            
            Check.LoadModel();

            //with missionID we can have a different starting positions, checkpoints, etc. for each mission
            //We need to load the data for each mission from file using the missionID

            #region set checkpoints
            numCheckPoints = (short)level.m_checkpoints.Count;
            CheckPointCoords = new Vector3[numCheckPoints];

            for (int i = 0; i < numCheckPoints; i++)
                CheckPointCoords[(int)level.m_checkpoints[i].W] = new Vector3(
                    level.m_checkpoints[i].X, level.m_checkpoints[i].Y, level.m_checkpoints[i].Z);

            CheckPoints = new Objects.ModelCollection(Check, numCheckPoints, CheckPointCoords);

            //needs to be different for each mission
            timeLimit = new TimeSpan(0, 0, (int)level.timelimit);



            float startingYaw = (level.playerStart.W / 255.0f) * MathHelper.TwoPi;

            scooter.setStartingPosition(startingYaw, 
                new Vector3(level.playerStart.X, level.playerStart.Y, level.playerStart.Z), 0);

            #endregion

            camera.update(scooter.Geometry.Location, scooter.Yaw);

            bounds = rootGame.GraphicsDevice.Viewport.TitleSafeArea;

            //Starts the collision detector
            collisions.start();

            isLoaded = true;
        }

        public void unload()
        {
            //Unloads the Collision
            if (collisions != null)
            {
                collisions.stop();
            }
            //level.unload();

            isLoaded = false;
        }

        public void update(GameTime gameTime)
        {
            //Updates the bounding spheres location and velocity
            //Waites for the thread functions sleep command outside
            //  the data lock.
            List<Objects.StaticModel> tmpPlayerModelList = 
                new List<Objects.StaticModel>();
            tmpPlayerModelList.Add(scooter.Geometry);
            collisions.setPlayerModels(tmpPlayerModelList);
            collisionList = collisions.waitToGetCollisions;
            #region debug message stuff
            //debugMessage = "";
            //BoundingSphere[] staticSpheres = collisions.waitToGetStaticSpheres;
            //BoundingSphere[][] instancedSpheres = collisions.waitToGetInstancedSpheres;

            //debugMessage += "Static Spheres: \n   ";
            //for (int i = 0; i < staticSpheres.Length; i++)
            //    debugMessage += "R{" + staticSpheres[i].Radius.ToString() + "}  " + staticSpheres[i].Center.ToString() + " \n   ";
            //debugMessage += "\n";

            //debugMessage += "Instanced Spheres: \n   ";
            //for (int i = 0; i < instancedSpheres.Length; i++)
            //    for(int j = 0; j < instancedSpheres[i].Length; j++)
            //        debugMessage += "R{" + instancedSpheres[i][j].Radius.ToString() + "}  " + instancedSpheres[i][j].Center.ToString() + " \n   ";
            //debugMessage += "\n";
            //BoundingSphere[] playerSpheres = collisions.waitToGetPlayerSpheres;
            //debugMessage += "Player Spheres: \n   ";
            //for (int i = 0; i < playerSpheres.Length; i++)
            //    debugMessage += "R{" + playerSpheres[i].Radius.ToString() + "}  " + playerSpheres[i].Center.ToString() + "\n   ";

            //debugMessage += "\n COLLISION: ";
            //if (collisionList != null && collisionList.Length > 0)
            //{
            //    for (int i = 0; i < collisionList.Length; i++)
            //    {
            //        debugMessage += "\n     " + Objects.StaticModel.modelList[collisionList[i].modelB_ID - 1].ModelAsset.ToString() +
            //            "   Coords: " + collisionList[i].objectSphere.Center.ToString() +
            //            "  Distance: " + (collisionList[i].objectSphere.Center - collisionList[i].playerSphere.Center).ToString();
            //    }
            //debugMessage += "\n";
            //}
            #endregion
            fps.update(gameTime);

            //Update Level
            level.update(gameTime, camera.ViewMtx, camera.ProjMtx);

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
                        gamePadState.previous.Buttons.Start == ButtonState.Released)
                    {
                        AudioManager.ResumeAudio();
                        state = MissionState.Playing;
                    }
#else
               
                if (keyBoardState.current.IsKeyDown(Keys.Q))
                {
                    AudioManager.ResumeAudio();
                    state = MissionState.Ending;
                }
                if (keyBoardState.current.IsKeyDown(Keys.Enter) &&
                    keyBoardState.previous.IsKeyUp(Keys.Enter))
                {
                    AudioManager.ResumeAudio();
                    state = MissionState.Playing;
                }     
#endif
                break;

                //if we are playing update scooter/camera using player input
                case MissionState.Playing :

                //Used to store coordinates of object closest to the scooter, using scooter location as origin
                Vector3 closestObjectOffSet = Vector3.Zero;

                //if scooter exceeds min/max X or Z set edge of map as closest object
                if(scooter.Geometry.Location.X > 780)
                    closestObjectOffSet = 
                        new Vector3(790, scooter.Geometry.Location.Y, scooter.Geometry.Location.Z) - 
                        scooter.Geometry.Location;
                else if (scooter.Geometry.Location.X < -780)
                    closestObjectOffSet = 
                        new Vector3(-790, scooter.Geometry.Location.Y, scooter.Geometry.Location.Z) - 
                        scooter.Geometry.Location;
                else if(scooter.Geometry.Location.Z > 780)
                    closestObjectOffSet =
                        new Vector3(scooter.Geometry.Location.X, scooter.Geometry.Location.Y, 790) -
                        scooter.Geometry.Location;
                else if (scooter.Geometry.Location.Z < -780)
                    closestObjectOffSet =
                        new Vector3(scooter.Geometry.Location.X, scooter.Geometry.Location.Y, -790) -
                        scooter.Geometry.Location;

                //Check collision distance
                for (int i = 0; i < collisionList.Length; i++)
                {
                    //find location of object using scooter model as origin
                    Vector3 collisionOffSet = collisionList[i].objectSphere.Center - scooter.Geometry.Location;

                    //if distance from player to object is less than either bounding sphere....
                    if (collisionOffSet.Length() < collisionList[i].objectSphere.Radius ||
                        collisionOffSet.Length() < collisionList[i].playerSphere.Radius)
                    {
                        //check to see if it is the closest object in the case of multiple collisions
                        if (closestObjectOffSet == Vector3.Zero)
                            closestObjectOffSet = collisionOffSet;
                        else if (collisionOffSet.Length() < closestObjectOffSet.Length())
                            closestObjectOffSet = collisionOffSet;
                    }


                }

                if (gamepadInUse)
                {
                    scooter.update(gameTime, -gamePadState.current.ThumbSticks.Left.X, gamePadState.current.Triggers.Right, gamePadState.current.Triggers.Left, closestObjectOffSet);
                }
                else
                {
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

                    scooter.update(gameTime, sVal, tVal, bVal, closestObjectOffSet);
                }

                scooter.setNormal(level.TerrainCollision);
                    camera.update(scooter.Geometry.Location, scooter.Yaw);


                    timeLimit -= gameTime.ElapsedGameTime;
                    if (timeLimit.Milliseconds < 0)
                        state = MissionState.Ending;

                    checkPointYaw += 0.05f;
                    for (int i = currentCheckPoint; i < numCheckPoints; i++)
                        CheckPoints.Geometry.Orientation = Matrix.CreateRotationY(checkPointYaw);

                    //Collision Detection

                    if (CheckPoints.CheckCollision(currentCheckPoint, scooter.Geometry))
                        currentCheckPoint += 1;
                    

                    if (currentCheckPoint == numCheckPoints)
                    {
                        player.Money += prizeMoney;
                        missionComplete = true;
                        state = MissionState.Ending;
                    }

#if XBOX
                    if (gamePadState.current.Buttons.Start == ButtonState.Pressed &&
                        gamePadState.current.Buttons.Start == ButtonState.Released)
                    {
                        AudioManager.PauseAudio();
                        state = MissionState.Paused;
                    }
#else

                    if (keyBoardState.current.IsKeyDown(Keys.Enter) && 
                        keyBoardState.previous.IsKeyUp(Keys.Enter))
                    {
                        AudioManager.PauseAudio();
                        state = MissionState.Paused;
                    }    
#endif
                break;


                //If mission has ended wait for user to confirm returning to menu
                case MissionState.Ending :
#if XBOX
                    if (gamePadState.current.Buttons.Start == ButtonState.Pressed)
                    {
                        AudioManager.ResumeAudio();
                        Active = false;
                    }
#else
               
                    if (keyBoardState.current.IsKeyDown(Keys.Enter))
                    {
                        AudioManager.ResumeAudio();
                        Active = false;
                    }             
#endif
                break;
            }

        }

        public void draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            fps.draw(gameTime);
            //Draw Level
            level.draw(gameTime, spriteBatch, camera.ViewMtx, camera.ProjMtx);

            scooter.Geometry.drawModel(gameTime, camera.ViewMtx, camera.ProjMtx);

            CheckPoints.draw(gameTime, camera, currentCheckPoint, (short)(currentCheckPoint + 1));
           

            
            //These setting allow us to print strings without screwing with the
            //3D rendering, but Carl told me it won't work with transparent objects

            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, 
                SamplerState.AnisotropicClamp, DepthStencilState.Default, 
                RasterizerState.CullCounterClockwise);

            

            //display camera and scooter coordinates for testing

            spriteBatch.DrawString(smallFont, "FPS: " + fps.FPS, new Vector2(bounds.Left + 11, bounds.Top + 21), Color.LightGreen);
            spriteBatch.DrawString(smallFont, "FPS: " + fps.FPS, new Vector2(bounds.Left + 10, bounds.Top + 20), Color.Black);

            //spriteBatch.DrawString(smallFont, "Main Thread Active: " + Thread.CurrentThread.IsAlive, new Vector2(bounds.Left + 11, bounds.Top + 41), Color.SteelBlue);
            //spriteBatch.DrawString(smallFont, "Main Thread Active: " + Thread.CurrentThread.IsAlive, new Vector2(bounds.Left + 10, bounds.Top + 40), Color.Black);
            //spriteBatch.DrawString(smallFont, "Main Thread State: " + Thread.CurrentThread.ThreadState, new Vector2(bounds.Left + 11, bounds.Top + 61), Color.SteelBlue);
            //spriteBatch.DrawString(smallFont, "Main Thread State: " + Thread.CurrentThread.ThreadState, new Vector2(bounds.Left + 10, bounds.Top + 60), Color.Black);

            //spriteBatch.DrawString(smallFont, "Collision Thread Active: " + collisions.CollisionThread.IsAlive, new Vector2(bounds.Left + 11, bounds.Top + 81), Color.Salmon);
            //spriteBatch.DrawString(smallFont, "Collision Thread Active: " + collisions.CollisionThread.IsAlive, new Vector2(bounds.Left + 10, bounds.Top + 80), Color.Black);
            //spriteBatch.DrawString(smallFont, "Collision Thread State: " + collisions.CollisionThread.ThreadState, new Vector2(bounds.Left + 11, bounds.Top + 101), Color.Salmon);
            //spriteBatch.DrawString(smallFont, "Collision Thread State: " + collisions.CollisionThread.ThreadState, new Vector2(bounds.Left + 10, bounds.Top + 100), Color.Black);

            //spriteBatch.DrawString(smallFont, debugMessage, new Vector2(bounds.Left + 10, bounds.Top + 120), Color.Orange);
            //spriteBatch.DrawString(smallFont, debugMessage, new Vector2(bounds.Left + 11, bounds.Top + 121), Color.Black);

            char endKey;
            string returnKey;
#if XBOX
            endKey = 'Y';
            returnKey = "START";
#else
            endKey = 'Q';
            returnKey = "ENTER";
#endif
            //This switch statement prints different instructions depending on MissionState
            switch (state)
            { 
                case MissionState.Starting :
                    spriteBatch.DrawString(smallFont, "Time Left: " + timeLimit.TotalSeconds.ToString("#.##"), new Vector2(bounds.Left + 400, bounds.Top + 30), Color.Black);
                    spriteBatch.DrawString(smallFont, "Get ready to race!!!", new Vector2(bounds.Left + 300, bounds.Top + 550), Color.Black);
                    spriteBatch.DrawString(largeFont, Math.Abs(scooter.Speed * 2.23f).ToString("0"), new Vector2(bounds.Left + 50, bounds.Bottom - 200), Color.Red);
                    if (countDown.Seconds > 0)
                        spriteBatch.DrawString(largeFont, countDown.Seconds.ToString(), new Vector2(bounds.Left + 450, bounds.Top + 100), Color.Black);
                    else
                        spriteBatch.DrawString(largeFont, "GO!", new Vector2(bounds.Left + 400, bounds.Top + 100), Color.Black);
                break;

                case MissionState.Paused:
                spriteBatch.DrawString(smallFont, "Time Left: " + timeLimit.TotalSeconds.ToString("#.##"), new Vector2(bounds.Left + 400, bounds.Top + 30), Color.Black);
                    spriteBatch.DrawString(largeFont, "paused", new Vector2(bounds.Left + 400, bounds.Top + 100), Color.Black);
                    spriteBatch.DrawString(smallFont, "Press [" + endKey + "] to quit mission", new Vector2(bounds.Left + 280, bounds.Bottom - 70), Color.Black);
                    spriteBatch.DrawString(smallFont, "Press [" + returnKey + "] to return to mission", new Vector2(bounds.Left + 280, bounds.Bottom - 50), Color.Black);
                    spriteBatch.DrawString(largeFont, Math.Abs(scooter.Speed * 2.23f).ToString("0"), new Vector2(bounds.Left + 50, bounds.Bottom - 200), Color.Red);
                break;

                case MissionState.Playing :
                    spriteBatch.DrawString(smallFont, "Time Left: " + timeLimit.TotalSeconds.ToString("#.##"), new Vector2(bounds.Left + 400, bounds.Top + 30), Color.Black);
                    spriteBatch.DrawString(smallFont, "Press [" + returnKey + "] to pause mission", new Vector2(bounds.Left + 280, bounds.Bottom - 50), Color.Black);
                    spriteBatch.DrawString(largeFont, Math.Abs(scooter.Speed * 2.23f).ToString("0"), new Vector2(bounds.Left + 50, bounds.Bottom - 200), Color.Red);
                break;

                case MissionState.Ending :
                if (missionComplete)
                {
                    spriteBatch.DrawString(largeFont, "Finish!", new Vector2(bounds.Left + 400, bounds.Top + 100), Color.Black);
                    spriteBatch.DrawString(smallFont, "You earned $" + prizeMoney.ToString(), new Vector2(bounds.Left + 200, bounds.Bottom - 130), Color.Black);
                    spriteBatch.DrawString(smallFont, "With " + timeLimit.TotalSeconds.ToString("#.##") + " seconds to spare!!!", new Vector2(bounds.Left + 200, bounds.Bottom - 110), Color.Black);
                }
                else
                    spriteBatch.DrawString(largeFont, "Fail!", new Vector2(bounds.Left + 450, bounds.Top + 100), Color.Black);

                    spriteBatch.DrawString(smallFont, "Press [" + returnKey + "] to return to menu", new Vector2(bounds.Left + 240, bounds.Bottom - 50), Color.Black);
                break;
            
            }

            spriteBatch.End();

        }

        //accessors and mutators
        //public Objects.SimpleModel Geometry { get { return geometry; } set { geometry = value; } }

        public Objects.ThirdPersonCamera Camera { get { return camera; } 
            set { camera = value; } }

        public bool IsLoaded
        {
            get { return isLoaded; }
        }
    }
}
