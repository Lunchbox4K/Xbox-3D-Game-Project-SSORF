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
            camera.ProjMtx = Matrix.CreatePerspectiveFieldOfView(
                            MathHelper.ToRadians(45.0f),
                            game.GraphicsDevice.Viewport.AspectRatio, 1.0f, 2000.0f);
            //Set Level
            levelProperties = new SSORFlibrary.LevelLayout();
            levelProperties.instanced_models = new List<SSORFlibrary.LocationMapAsset>();
            levelProperties.statics_models = new List<SSORFlibrary.LocationMapAsset>();

            SSORFlibrary.LocationMapAsset tree = new SSORFlibrary.LocationMapAsset();
            tree.asset_colorID = 130;
            tree.asset_location = "Models\\tree";
            levelProperties.instanced_models.Add(tree);

            SSORFlibrary.LocationMapAsset car = new SSORFlibrary.LocationMapAsset();
            car.asset_colorID = 50;
            car.asset_location = "Models\\car1";
            levelProperties.statics_models.Add(car);

            SSORFlibrary.LocationMapAsset bench = new SSORFlibrary.LocationMapAsset();
            bench.asset_colorID = 20;
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
            handicap.asset_colorID = 120;
            handicap.asset_location = "Models\\handicapsign";
            levelProperties.instanced_models.Add(handicap);

            SSORFlibrary.LocationMapAsset cart = new SSORFlibrary.LocationMapAsset();
            cart.asset_colorID = 60;
            cart.asset_location = "Models\\shoppingcart";
            levelProperties.instanced_models.Add(cart);

            SSORFlibrary.LocationMapAsset light = new SSORFlibrary.LocationMapAsset();
            light.asset_colorID = 80;
            light.asset_location = "Models\\streetlight";
            levelProperties.instanced_models.Add(light);

            SSORFlibrary.LocationMapAsset store = new SSORFlibrary.LocationMapAsset();
            store.asset_colorID = 90;
            store.asset_location = "Models\\storefront";
            levelProperties.instanced_models.Add(store);

            levelProperties.instances_locationMap = "Images\\Terrain\\lvl3_mm";
            levelProperties.level_effect = "Effects\\TerrainTextureEffect";
            levelProperties.level_heightMap = "Images\\Terrain\\lvl3_hm";
            levelProperties.level_textureB = "Images\\Terrain\\asphalt";
            levelProperties.level_textureG = "Images\\Terrain\\terrainTextureG";
            levelProperties.level_textureMap = "Images\\Terrain\\lvl3_cm";
            levelProperties.level_textureR = "Images\\Terrain\\terrainTextureR";
            levelProperties.viewTree_refreshRate = 8;
            level = new Objects.Level(game, levelProperties);
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

            Check = new Objects.StaticModel(content, "Models\\check",
                        Vector3.Zero, Matrix.Identity, Matrix.Identity);
            
            Check.LoadModel();

            //with missionID we can have a different starting positions, checkpoints, etc. for each mission
            //We need to load the data for each mission from file using the missionID

            #region temp mission loading (needs to be done from file)
            if (missionID == 1)
            {
                numCheckPoints = 3;
                CheckPointCoords = new Vector3[numCheckPoints];
                timeLimit = new TimeSpan(0, 0, 100);

                CheckPointCoords[0] = new Vector3(140, 0, 0);
                CheckPointCoords[1] = new Vector3(0, 0, -140);
                CheckPointCoords[2] = new Vector3(0, 0, 140);

                CheckPoints = new Objects.ModelCollection(Check, numCheckPoints, CheckPointCoords);

                scooter.setStartingPosition(-0.45f, new Vector3(0, 0, 100), 1);

                prizeMoney = 100;
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


                scooter.setStartingPosition(0.0f, new Vector3(0,0,40), 1);

                prizeMoney = 200;
            }

            #endregion

            camera.update(scooter.Geometry.Location, scooter.Yaw);

            bounds = rootGame.GraphicsDevice.Viewport.TitleSafeArea;
        }


        public void update(GameTime gameTime)
        {
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
                        gamePadState.current.Buttons.Start == ButtonState.Released) 
                        state = MissionState.Paused;
#else
                scooter.UpdateAudio(0);
                if (keyBoardState.current.IsKeyDown(Keys.Q))
                        state = MissionState.Ending;
                    if (keyBoardState.current.IsKeyDown(Keys.Enter) &&
                        keyBoardState.previous.IsKeyUp(Keys.Enter))
                        state = MissionState.Playing;     
#endif
                break;

                //if we are playing update scooter/camera using player input
                case MissionState.Playing :

                if (gamepadInUse)
                {
                    scooter.update(gameTime, -gamePadState.current.ThumbSticks.Left.X, gamePadState.current.Triggers.Right, gamePadState.current.Triggers.Left);
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

                    scooter.update(gameTime, sVal, tVal, bVal);
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

                    //bounding box did not work!

                    
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
                scooter.UpdateAudio(0);
                    if (keyBoardState.current.IsKeyDown(Keys.Enter))  
                        Active = false;               
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

            spriteBatch.DrawString(smallFont, "FPS: " + fps.FPS, new Vector2(bounds.Left + 11, bounds.Top + 51), Color.Black);
            spriteBatch.DrawString(smallFont, "FPS: " + fps.FPS, new Vector2(bounds.Left + 10, bounds.Top + 50), Color.Black);
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
