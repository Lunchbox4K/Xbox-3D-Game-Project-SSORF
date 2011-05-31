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
        string debugMessage = "";

        bool playerTouchingWall = false;
        TimeSpan wallTime = TimeSpan.Zero;

        private Rectangle bounds;
        #endregion

        //empty constructor for making missions without parameters
        public Mission()
        { }

        public Mission(Objects.Player playerInfo, SSORFlibrary.ScooterData ScooterSpecs, Game game)
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

            //Loads level properties.
            setFirstLevel();

            //Initialize the variable in the constructor
            collisions = new Objects.CollisionDetection();
            collisions.setPlayerModels(playerModels);
        }

        public void setFirstLevel() //TEMP FCN TO CLEAN CODE A LITTLE
        {
            //Set Level
            levelProperties = new SSORFlibrary.LevelLayout();
            levelProperties.instanced_models = new List<SSORFlibrary.LocationMapAsset>();
            levelProperties.statics_models = new List<SSORFlibrary.LocationMapAsset>();

            levelProperties.level_heightMap = "Images\\Terrain\\lvl1_hm";
            levelProperties.level_textureMap = "Images\\Terrain\\lvl1_cm";
            levelProperties.level_textureB = "Images\\Terrain\\terrainTextureB";
            levelProperties.level_textureG = "Images\\Terrain\\terrainTextureG";
            levelProperties.level_textureR = "Images\\Terrain\\terrainTextureR";

            levelProperties.locationMap = "Images\\Terrain\\lvl1_mm";

            levelProperties.playerSpawns = 7;       //R Value (X)
            levelProperties.borderPoints = 1;       //R Value (X)
            levelProperties.trackSpawnPoints = 2;   //R Value (X)
            levelProperties.checkpointSpawn = 3;    //R Value (X)
            levelProperties.checkpointAsset = "Models\\Check";  

            levelProperties.viewTree_refreshRate = 8;

            addModelsToLevel(); //Set Models

            level = new Objects.Level(rootGame, levelProperties);
        }

        public void addModelsToLevel()
        {
            SSORFlibrary.LocationMapAsset tree = new SSORFlibrary.LocationMapAsset();
            tree.asset_colorID = 128;
            tree.asset_location = "Models\\tree";
            levelProperties.instanced_models.Add(tree);

            SSORFlibrary.LocationMapAsset car = new SSORFlibrary.LocationMapAsset();
            car.asset_colorID = 222;
            car.asset_location = "Models\\tree2";
            levelProperties.statics_models.Add(car);

            SSORFlibrary.LocationMapAsset bench = new SSORFlibrary.LocationMapAsset();
            bench.asset_colorID = 99;
            bench.asset_location = "Models\\bench";
            levelProperties.instanced_models.Add(bench);

            SSORFlibrary.LocationMapAsset can = new SSORFlibrary.LocationMapAsset();
            can.asset_colorID = 40;
            can.asset_location = "Models\\garbagecan";
            levelProperties.instanced_models.Add(can);

            SSORFlibrary.LocationMapAsset storesign = new SSORFlibrary.LocationMapAsset();
            storesign.asset_colorID = 100;
            storesign.asset_location = "Models\\storesign";
            levelProperties.instanced_models.Add(storesign);

            SSORFlibrary.LocationMapAsset handicap = new SSORFlibrary.LocationMapAsset();
            handicap.asset_colorID = 256;
            handicap.asset_location = "Models\\handicapsign";
            levelProperties.instanced_models.Add(handicap);

            SSORFlibrary.LocationMapAsset cart = new SSORFlibrary.LocationMapAsset();
            cart.asset_colorID = 60;
            cart.asset_location = "Models\\shoppingcart";
            levelProperties.instanced_models.Add(cart);

            SSORFlibrary.LocationMapAsset light = new SSORFlibrary.LocationMapAsset();
            light.asset_colorID = 64;
            light.asset_location = "Models\\streetlight";
            levelProperties.instanced_models.Add(light);

            SSORFlibrary.LocationMapAsset store = new SSORFlibrary.LocationMapAsset();
            store.asset_colorID = 90;
            store.asset_location = "Models\\storefront";
            levelProperties.instanced_models.Add(store);
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
            collisions.setBorders(level.getLocationMap, levelProperties.borderPoints);

            //Grabs the raw static and instanced model data from the level for processing
            // Will probably grab a list of checkpoints if the list is created in the level class
            collisions.setModels(level.StaticModels, level.InstancedModels, level.ModelInstances);
           

            //with missionID we can have a different starting positions, checkpoints, etc. for each mission
            //We need to load the data for each mission from file using the missionID

            #region temp mission loading (needs to be done from file)
            if (missionID == 1)
            {
                //numCheckPoints = 3;
                //CheckPointCoords = new Vector3[numCheckPoints];
                //timeLimit = new TimeSpan(0, 0, 100);

                //CheckPointCoords[0] = new Vector3(140, 0, 0);
                //CheckPointCoords[1] = new Vector3(0, 0, -140);
                //CheckPointCoords[2] = new Vector3(0, 0, 140);

                //CheckPoints = new Objects.ModelCollection(Check, numCheckPoints, CheckPointCoords);

                scooter.setStartingPosition(-0.45f, new Vector3(0, 0, 100), 1);

                prizeMoney = 100;
            }
            else if (missionID == 2)
            {
                //numCheckPoints = 5;
                //CheckPointCoords = new Vector3[numCheckPoints];
                //timeLimit = new TimeSpan(0, 0, 15);

                //CheckPointCoords[0] = new Vector3(0, 0, -140);
                //CheckPointCoords[1] = new Vector3(140, 0, 0);
                //CheckPointCoords[2] = new Vector3(0, 0, 140);
                //CheckPointCoords[3] = new Vector3(-140, 0, 0);
                //CheckPointCoords[4] = new Vector3(0, 0, -140);

                //CheckPoints = new Objects.ModelCollection(Check, numCheckPoints, CheckPointCoords);


                scooter.setStartingPosition(0.0f, new Vector3(0,0,40), 1);

                prizeMoney = 200;
            }

            #endregion

            //camera.update(scooter.Geometry.Location, scooter.Yaw);

            //Mitchell's Temp Direction Fix for Circle Map
            scooter.Geometry.Orientation *= Matrix.CreateRotationY(MathHelper.PiOver2);
            scooter.Yaw += MathHelper.PiOver2;

            bounds = rootGame.GraphicsDevice.Viewport.TitleSafeArea;


            //Starts the collision detector
            collisions.start();


        }

        public void unload()
        {
            //Unloads the Collision
            if (collisions != null)
            {
                collisions.stop();
            }
            //Unload All the Models
            //scooter.Geometry.UnloadModel();
            //level.unload();
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
            playerTouchingWall = collisions.waitToGetPlayerTouchingWall[0]; //Forced on 1st player



            debugMessage = "";
            BoundingSphere[] staticSpheres = collisions.waitToGetStaticSpheres;
            BoundingSphere[][] instancedSpheres = collisions.waitToGetInstancedSpheres;

            debugMessage += "\n Collisions: ";
            if (collisionList != null && collisionList.Length > 0)
            {
                for (int i = 0; i < collisionList.Length; i++)
                {
                    debugMessage += "\n     " + Objects.StaticModel.modelList[collisionList[i].modelB_ID - 1].ModelAsset.ToString() +
                        "   Coords: " + collisionList[i].objectSphere.Center.ToString() +
                        "  Distance: " + (collisionList[i].objectSphere.Center - collisionList[i].playerSphere.Center).ToString();
                }
            debugMessage += "\n";
            }

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

                    //checkPointYaw += 0.05f;
                    //for (int i = currentCheckPoint; i < numCheckPoints; i++)
                    //    CheckPoints.Geometry.Orientation = Matrix.CreateRotationY(checkPointYaw);

                    //Spawn Player To Locations

                    scooter.Geometry.Location = level.getSpawns.Position;
    
                    //scooter.Geometry.Orientation = 
                    //scooter.Geometry.Orientation 
                    camera.update(scooter.Geometry.Location, scooter.Yaw);
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

                for (int i = 0; i < collisionList.Length; i++)
                    if (Objects.StaticModel.modelList[collisionList[i].modelB_ID].ModelAsset == "Models\\Check")
                    {
                        level.disableStaticAsset(collisionList[i].modelB_ID);
                    }
                    else
                    {
                        //find location of object using scooter model as origin
                        Vector3 collisionOffSet = collisionList[i].objectSphere.Center - scooter.Geometry.Location;

                        //if distance from player to object is greater than either bounding sphere....
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


                    //timeLimit -= gameTime.ElapsedGameTime;
                    //if (timeLimit.Milliseconds < 0)
                    //    state = MissionState.Ending;

                    checkPointYaw += 0.05f;
                    //for (int i = currentCheckPoint; i < numCheckPoints; i++)
                    //    CheckPoints.Geometry.Orientation = Matrix.CreateRotationY(checkPointYaw);

                    //Collision Detection

                    //if (CheckPoints.CheckCollision(currentCheckPoint, scooter.Geometry))
                    //    currentCheckPoint += 1;
                    

                    if (currentCheckPoint == numCheckPoints)
                    {
                        player.Money += prizeMoney;
                        missionComplete = true;
                        //state = MissionState.Ending;
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

            //if(currentCheckPoint == numCheckPoints - 1)
            //    CheckPoints.draw(gameTime, camera, currentCheckPoint, (short)(currentCheckPoint + 1));
            //else
            //   CheckPoints.draw(gameTime, camera, currentCheckPoint, (short)(currentCheckPoint + 1));

           

            
            //These setting allow us to print strings without screwing with the
            //3D rendering, but Carl told me it won't work with transparent objects

            spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, 
                SamplerState.AnisotropicClamp, DepthStencilState.Default, 
                RasterizerState.CullCounterClockwise);

            

            //display camera and scooter coordinates for testing

            spriteBatch.DrawString(smallFont, "FPS: " + fps.FPS, new Vector2(bounds.Left + 11, bounds.Top + 21), Color.LightGreen);
            spriteBatch.DrawString(smallFont, "FPS: " + fps.FPS, new Vector2(bounds.Left + 10, bounds.Top + 20), Color.Black);

            spriteBatch.DrawString(smallFont, "Main Thread Active: " + Thread.CurrentThread.IsAlive, new Vector2(bounds.Left + 11, bounds.Top + 41), Color.SteelBlue);
            spriteBatch.DrawString(smallFont, "Main Thread Active: " + Thread.CurrentThread.IsAlive, new Vector2(bounds.Left + 10, bounds.Top + 40), Color.Black);
            spriteBatch.DrawString(smallFont, "Main Thread State: " + Thread.CurrentThread.ThreadState, new Vector2(bounds.Left + 11, bounds.Top + 61), Color.SteelBlue);
            spriteBatch.DrawString(smallFont, "Main Thread State: " + Thread.CurrentThread.ThreadState, new Vector2(bounds.Left + 10, bounds.Top + 60), Color.Black);

            spriteBatch.DrawString(smallFont, "Collision Thread Active: " + collisions.CollisionThread.IsAlive, new Vector2(bounds.Left + 11, bounds.Top + 81), Color.Salmon);
            spriteBatch.DrawString(smallFont, "Collision Thread Active: " + collisions.CollisionThread.IsAlive, new Vector2(bounds.Left + 10, bounds.Top + 80), Color.Black);
            spriteBatch.DrawString(smallFont, "Collision Thread State: " + collisions.CollisionThread.ThreadState, new Vector2(bounds.Left + 11, bounds.Top + 101), Color.Salmon);
            spriteBatch.DrawString(smallFont, "Collision Thread State: " + collisions.CollisionThread.ThreadState, new Vector2(bounds.Left + 10, bounds.Top + 100), Color.Black);

            if (playerTouchingWall)
                wallTime += gameTime.ElapsedGameTime;

            spriteBatch.DrawString(smallFont, "Player touching wall: " + wallTime, new Vector2(bounds.Left + 10, bounds.Top + 120), Color.Azure);
            spriteBatch.DrawString(smallFont, "Player touching wall: " + wallTime, new Vector2(bounds.Left + 11, bounds.Top + 121), Color.Black);

            spriteBatch.DrawString(smallFont, debugMessage, new Vector2(bounds.Left + 10, bounds.Top + 140), Color.Orange);
            spriteBatch.DrawString(smallFont, debugMessage, new Vector2(bounds.Left + 11, bounds.Top + 141), Color.Black);

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
                    spriteBatch.DrawString(smallFont, "Time Left: " + timeLimit.TotalSeconds.ToString(), new Vector2(bounds.Left + 200, bounds.Top + 10), Color.Black);
                    spriteBatch.DrawString(smallFont, "Get ready to race!!!", new Vector2(bounds.Left + 300, bounds.Top + 550), Color.Black);
                    spriteBatch.DrawString(largeFont, Math.Abs(scooter.Speed * 2.23f).ToString("0"), new Vector2(bounds.Left + 50, bounds.Top + 400), Color.Red);
                    if (countDown.Seconds > 0)
                        spriteBatch.DrawString(largeFont, countDown.Seconds.ToString(), new Vector2(bounds.Left + 340, bounds.Top + 100), Color.Black);
                    else
                        spriteBatch.DrawString(largeFont, "GO!", new Vector2(bounds.Left + 220, bounds.Top + 100), Color.Black);
                break;

                case MissionState.Paused:
                    spriteBatch.DrawString(smallFont, "Time Left: " + timeLimit.TotalSeconds.ToString(), new Vector2(bounds.Left + 200, bounds.Top + 10), Color.Black);
                    spriteBatch.DrawString(largeFont, "paused", new Vector2(bounds.Left + 80, bounds.Top + 100), Color.Black);
                    spriteBatch.DrawString(smallFont, "Press [" + endKey + "] to quit mission", new Vector2(bounds.Left + 280, bounds.Bottom - 70), Color.Black);
                    spriteBatch.DrawString(smallFont, "Press [" + returnKey + "] to return to mission", new Vector2(bounds.Left + 280, bounds.Bottom - 50), Color.Black);
                    spriteBatch.DrawString(largeFont, Math.Abs(scooter.Speed * 2.23f).ToString("0"), new Vector2(bounds.Left + 50, bounds.Bottom - 200), Color.Red);
                break;

                case MissionState.Playing :
                    spriteBatch.DrawString(smallFont, "Time Left: " + timeLimit.TotalSeconds.ToString(), new Vector2(bounds.Left + 200, bounds.Top + 10), Color.Black);
                    spriteBatch.DrawString(smallFont, "Press [" + returnKey + "] to pause mission", new Vector2(bounds.Left + 280, bounds.Bottom - 50), Color.Black);
                    spriteBatch.DrawString(largeFont, Math.Abs(scooter.Speed * 2.23f).ToString("0"), new Vector2(bounds.Left + 50, bounds.Bottom - 200), Color.Red);
                break;

                case MissionState.Ending :
                if (missionComplete)
                {
                    spriteBatch.DrawString(largeFont, "Finish!", new Vector2(bounds.Left + 60, 100), Color.Black);
                    spriteBatch.DrawString(smallFont, "You earned $" + prizeMoney.ToString(), new Vector2(bounds.Left + 200, bounds.Bottom - 130), Color.Black);
                    spriteBatch.DrawString(smallFont, "With " + timeLimit.TotalSeconds.ToString() + " seconds to spare!!!", new Vector2(bounds.Left + 200, bounds.Bottom - 110), Color.Black);
                }
                else
                    spriteBatch.DrawString(largeFont, "Fail!", new Vector2(bounds.Left + 200, 100), Color.Black);

                    spriteBatch.DrawString(smallFont, "Press [" + returnKey + "] to return to menu", new Vector2(bounds.Left + 240, bounds.Bottom - 50), Color.Black);
                break;
            
            }

            spriteBatch.End();

        }

        //accessors and mutators
        //public Objects.SimpleModel Geometry { get { return geometry; } set { geometry = value; } }

        public Objects.ThirdPersonCamera Camera { get { return camera; } 
            set { camera = value; } }
    }
}
