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
using SSORFlibrary;

//MenuManager will need functionality added to allow player to go 
//back and forth between different menus by setting the currentMenu

namespace SSORF.Management
{
    
    class MenuManager
    {

        //Which menu is currently being viewed?
        enum Menu : int
        {
            Main = 0,
            VehicleSelect,
            Dealership,
            Missions,
            TuneShop,
            Options,
            Credits,
            NumMenus,
        }

        #region declarations

        //if a mission other than zero is selected, state is switched
        public short selectedMission = 0;

        private ContentManager GameContent;

        private SpriteFont menuFont;
        private Objects.MessageBox messageBox = new Objects.MessageBox();
        private string message;

        private Menu CurrentMenu;
        private States.SubMenu[] Menus;
 
        private Texture2D CursorImage;
        private Texture2D fixedImage;
        //used to display upgrade data
        private UpgradeData[] upgrades;
        private ScooterData[] scooters;

        //used to display scooter models in vehicle select
        Matrix view, proj;
        private Objects.SimpleModel[] scooterModels = new Objects.SimpleModel[5];
        private float scooterYaw = 0.0f;
        private short[] scooterIDnums = new short[8];
        private short VSBackButton;

        GraphicsDevice graphics;

        #endregion

        public MenuManager(ContentManager content, Objects.Player player, GraphicsDevice graphicsDevice)
        {
            graphics = graphicsDevice;
            GameContent = content;
            //font for displaying upgrade data
            menuFont = content.Load<SpriteFont>("menuFont");

            //Load UpgradeData
            upgrades = content.Load<UpgradeData[]>("upgrades");
            //Load ScooterData
            scooters = content.Load<ScooterData[]>("scooters");

            //load scooter models
            for (int i = 0; i < 5; i++)
            {
                scooterModels[i] = new Objects.SimpleModel();
                scooterModels[i].Mesh = content.Load<Model>("Models\\scooter" + i.ToString());
            }

            Menus = new States.SubMenu[(int)Menu.NumMenus];

            #region Load Main Menu
            Menus[(int)Menu.Main] = new States.SubMenu(6); //main menu has 7 buttons
            Menus[(int)Menu.Main].BackGround = content.Load<Texture2D>("Images\\game menu");
            Menus[(int)Menu.Main].ButtonImage[0] = content.Load<Texture2D>("Images\\Missions");
            Menus[(int)Menu.Main].ButtonPosition[0] = new Vector2(35, 140);
            Menus[(int)Menu.Main].ButtonImage[1] = content.Load<Texture2D>("Images\\Dealership");
            Menus[(int)Menu.Main].ButtonPosition[1] = new Vector2(35, 190);
            Menus[(int)Menu.Main].ButtonImage[2] = content.Load<Texture2D>("Images\\Garage");
            Menus[(int)Menu.Main].ButtonPosition[2] = new Vector2(35, 240);
            Menus[(int)Menu.Main].ButtonImage[3] = content.Load<Texture2D>("Images\\Versus");
            Menus[(int)Menu.Main].ButtonPosition[3] = new Vector2(35, 290);
            Menus[(int)Menu.Main].ButtonImage[4] = content.Load<Texture2D>("Images\\Options");
            Menus[(int)Menu.Main].ButtonPosition[4] = new Vector2(35, 340);
            Menus[(int)Menu.Main].ButtonImage[5] = content.Load<Texture2D>("Images\\Credits");
            Menus[(int)Menu.Main].ButtonPosition[5] = new Vector2(35, 390);
            #endregion

            #region Load VehicleSelect (select owned vehicles)

            loadVehicleSelect(content, player.ScootersOwned);

            #endregion

            #region Load Dealership (buy new vehicles)

            Menus[(int)Menu.Dealership] = new States.SubMenu(10);
            Menus[(int)Menu.Dealership].BackGround = content.Load<Texture2D>("Images\\Dealership1");

            int y = 100;
            for (int i = 0; i < 8; i++)
            {
                Menus[(int)Menu.Dealership].ButtonImage[i] = content.Load<Texture2D>("Images\\vehicle" + i.ToString());
                Menus[(int)Menu.Dealership].ButtonPosition[i] = new Vector2(20, y);
                y += 45;
            }
            Menus[(int)Menu.Dealership].ButtonImage[8] = content.Load<Texture2D>("Images\\TuneShopButton");
            Menus[(int)Menu.Dealership].ButtonPosition[8] = new Vector2(650, 5);
            Menus[(int)Menu.Dealership].ButtonImage[9] = content.Load<Texture2D>("Images\\BackButton");
            Menus[(int)Menu.Dealership].ButtonPosition[9] = new Vector2(20, 550);
            

            #endregion

            #region Load TuneShop (buy upgrades)

            Menus[(int)Menu.TuneShop] = new States.SubMenu(10); //tune shop has 4 buttons
            Menus[(int)Menu.TuneShop].BackGround = content.Load<Texture2D>("Images\\TuneShopTest");
            for (int i = 0; i < 9; i++)
                Menus[(int)Menu.TuneShop].ButtonImage[i] = content.Load<Texture2D>("Images\\" + upgrades[i].button);
            Menus[(int)Menu.TuneShop].ButtonPosition[0] = new Vector2(150, 300);
            Menus[(int)Menu.TuneShop].ButtonPosition[1] = new Vector2(250, 300);
            Menus[(int)Menu.TuneShop].ButtonPosition[2] = new Vector2(350, 300);
            Menus[(int)Menu.TuneShop].ButtonPosition[3] = new Vector2(150, 380);
            Menus[(int)Menu.TuneShop].ButtonPosition[4] = new Vector2(250, 380);
            Menus[(int)Menu.TuneShop].ButtonPosition[5] = new Vector2(350, 380);
            Menus[(int)Menu.TuneShop].ButtonPosition[6] = new Vector2(150, 460);
            Menus[(int)Menu.TuneShop].ButtonPosition[7] = new Vector2(250, 460);
            Menus[(int)Menu.TuneShop].ButtonPosition[8] = new Vector2(350, 460);
            Menus[(int)Menu.TuneShop].ButtonImage[9] = content.Load<Texture2D>("Images\\BackButton");
            Menus[(int)Menu.TuneShop].ButtonPosition[9] = new Vector2(25, 70);
            #endregion

            #region Load MissionsMenu
            Menus[(int)Menu.Missions] = new States.SubMenu(3); //mission menu has 2 buttons
            Menus[(int)Menu.Missions].BackGround = content.Load<Texture2D>("Images\\Options");
            Menus[(int)Menu.Missions].ButtonImage[0] = content.Load<Texture2D>("Images\\button1");
            Menus[(int)Menu.Missions].ButtonPosition[0] = new Vector2(100, 250);
            Menus[(int)Menu.Missions].ButtonImage[1] = content.Load<Texture2D>("Images\\button2");
            Menus[(int)Menu.Missions].ButtonPosition[1] = new Vector2(450, 300);
            Menus[(int)Menu.Missions].ButtonImage[2] = content.Load<Texture2D>("Images\\BackButton");
            Menus[(int)Menu.Missions].ButtonPosition[2] = new Vector2(600, 550);
            #endregion

            #region Load Credits
            Menus[(int)Menu.Credits] = new States.SubMenu(1); //mission menu has 2 buttons
            Menus[(int)Menu.Credits].BackGround = content.Load<Texture2D>("Images\\Credits Form");
            Menus[(int)Menu.Credits].ButtonImage[0] = content.Load<Texture2D>("Images\\BackButton");
            Menus[(int)Menu.Credits].ButtonPosition[0] = new Vector2(50, 550);
            #endregion

            #region Load Options
            
            Menus[(int)Menu.Options] = new States.SubMenu(3); //mission menu has 3 buttons
            Menus[(int)Menu.Options].BackGround = content.Load<Texture2D>("Images\\Options Form");
            //Menus[(int)Menu.Options].ButtonImage[0] = content.Load<Texture2D>("Images\\Music Button");
            //Menus[(int)Menu.Options].ButtonPosition[0] = new Vector2(75, 150);
            Menus[(int)Menu.Options].ButtonImage[0] = content.Load<Texture2D>("Images\\On Button");
            Menus[(int)Menu.Options].ButtonPosition[0] = new Vector2(250, 150);
            Menus[(int)Menu.Options].ButtonImage[1] = content.Load<Texture2D>("Images\\Off Button");
            Menus[(int)Menu.Options].ButtonPosition[1] = new Vector2(350, 150);
            Menus[(int)Menu.Options].ButtonImage[2] = content.Load<Texture2D>("Images\\BackButton");
            Menus[(int)Menu.Options].ButtonPosition[2] = new Vector2(50, 550);
            #endregion

            //load messageBox background
            messageBox.Background = content.Load<Texture2D>("Images\\messagebox");
            fixedImage = content.Load<Texture2D>("Images\\Music Button");
            //load cursor image and set current menu to main menu
            CursorImage = content.Load<Texture2D>("Images\\cursor");
            CurrentMenu = Menu.Main;

            //for 3D camera
            proj = Matrix.CreatePerspectiveFieldOfView(
                            MathHelper.ToRadians(45.0f),
                            1.33f, 1.0f, 1000.0f);
            view = Matrix.CreateLookAt(Vector3.Zero, new Vector3(0, 0, -20), Vector3.Up);
        }

        public void update(GameTime gameTime, Objects.Player player)
        {
            if (!messageBox.Active)
            {

                Menus[(int)CurrentMenu].update(gameTime);

                switch (CurrentMenu)
                {

                    #region update MainMenu
                    case Menu.Main:

                        if (Menus[(int)Menu.Main].buttonPressed == 1)
                        {
                            CurrentMenu = Menu.Missions;
                            Menus[(int)Menu.Missions].updateCursor();
                        }
                        else if (Menus[(int)Menu.Main].buttonPressed == 2)
                        {
                            CurrentMenu = Menu.Dealership;
                            Menus[(int)Menu.Dealership].updateCursor();
                        }
                        else if (Menus[(int)Menu.Main].buttonPressed == 3)
                        {
                            loadVehicleSelect(GameContent, player.ScootersOwned);
                            CurrentMenu = Menu.VehicleSelect;
                            Menus[(int)Menu.VehicleSelect].updateCursor();
                        }
                        //else if (Menus[(int)Menu.Main].buttonPressed == 4)
                        //    CurrentMenu = Menu.VehicleSelect;
                        else if (Menus[(int)Menu.Main].buttonPressed == 5)
                            CurrentMenu = Menu.Options;
                        else if (Menus[(int)Menu.Main].buttonPressed == 6)
                            CurrentMenu = Menu.Credits;
                        Menus[(int)Menu.Main].buttonPressed = 0;

                        break;
                    #endregion

                    #region update MissionsMenu
                    case Menu.Missions:

                        //Note: buttonPressed = 0 means no button has been pressed
                        //If we are in the missions menu and a button is pressed...
                        if (Menus[(int)Menu.Missions].buttonPressed == 3)
                        {
                            CurrentMenu = Menu.Main;
                            Menus[(int)Menu.Missions].selectedButton = 1;
                        }
                        else if (Menus[(int)Menu.Missions].buttonPressed != 0)
                        {
                            //Set selected mission to a value other than zero to deactivate the menu
                            selectedMission = Menus[(int)Menu.Missions].buttonPressed;
                            //reset buttonPressed for when we return to the menu

                        }

                        Menus[(int)Menu.Missions].buttonPressed = 0;
                        break;
                    #endregion

                    #region update Dealership

                    case Menu.Dealership:
                        if (Menus[(int)Menu.Dealership].buttonPressed == 10)
                        {
                            CurrentMenu = Menu.Main;
                            Menus[(int)Menu.Dealership].selectedButton = 1;
                        }
                        if (Menus[(int)Menu.Dealership].buttonPressed == 9)
                        {
                            CurrentMenu = Menu.TuneShop;
                            Menus[(int)Menu.Dealership].selectedButton = 1;
                        }

                        // < 3 should get changed to < 9 when we have rest of scooters 
                        if (Menus[(int)Menu.Dealership].buttonPressed > 0 &&
                           Menus[(int)Menu.Dealership].buttonPressed < 6) 
                        {
                            message = player.PurchaseScooter(
                                scooters[Menus[(int)Menu.Dealership].buttonPressed - 1]);
                            messageBox.setMessage("Message", message);
                            messageBox.Active = true;
                        }
                        
                        Menus[(int)Menu.Dealership].buttonPressed = 0;
                        break;

                    #endregion

                    #region update TuneShop
                    case Menu.TuneShop:
                        if (Menus[(int)Menu.TuneShop].buttonPressed == 10)
                        {
                            CurrentMenu = Menu.Dealership;
                            Menus[(int)Menu.TuneShop].selectedButton = 1;
                        }

                        if (Menus[(int)Menu.TuneShop].buttonPressed > 0 &&
                            Menus[(int)Menu.TuneShop].buttonPressed < 10)
                        {
                            message = player.PurchaseUpgrade(
                                upgrades[Menus[(int)Menu.TuneShop].buttonPressed - 1]);
                            messageBox.setMessage("Message", message);
                            messageBox.Active = true;
                        }

                        Menus[(int)Menu.TuneShop].buttonPressed = 0;
                        break;
                    #endregion

                    #region update VehicleSelect

                    case Menu.VehicleSelect:

                        if (Menus[(int)Menu.VehicleSelect].buttonPressed == VSBackButton)
                        {
                            CurrentMenu = Menu.Main;
                            Menus[(int)Menu.VehicleSelect].selectedButton = 1;
                        }
                        if (Menus[(int)Menu.VehicleSelect].buttonPressed > 0 &&
                            Menus[(int)Menu.VehicleSelect].buttonPressed < VSBackButton)
                        {
                            player.SelectedScooter = scooterIDnums[Menus[(int)Menu.VehicleSelect].buttonPressed - 1];
                            CurrentMenu = Menu.Main;
                            Menus[(int)Menu.VehicleSelect].selectedButton = 1;
                        }

                        Menus[(int)Menu.VehicleSelect].buttonPressed = 0;

                        break;
                    #endregion

                    #region update Credits
                    case Menu.Credits:
                         if (Menus[(int)Menu.Credits].buttonPressed == 1)
                            CurrentMenu = Menu.Main;
                        Menus[(int)Menu.Credits].buttonPressed = 0;

                        break;
                    //etc.
                    #endregion

                    #region update Options
                    case Menu.Options:
                        if (Menus[(int)Menu.Options].buttonPressed == 1)
                            AudioManager.setMusicPlaying(true);
                        if (Menus[(int)Menu.Options].buttonPressed == 2)
                            AudioManager.setMusicPlaying(false);
                        if (Menus[(int)Menu.Options].buttonPressed == 3)
                        {
                            CurrentMenu = Menu.Main;
                            Menus[(int)Menu.Options].selectedButton = 1;
                        }
                        Menus[(int)Menu.Options].buttonPressed = 0;

                        break;
                    //etc.
                    #endregion
                }


            }
            else
                messageBox.update();

        }
        //draw the current menu and the cursor
        public void draw(SpriteBatch spriteBatch, Objects.Player player)
        {
            spriteBatch.Begin();
            Menus[(int)CurrentMenu].draw(spriteBatch);

            #region draw upgrade/vehicle specs and player money
            switch (CurrentMenu)
            {
                case Menu.Main :
                    spriteBatch.DrawString(menuFont, "Current Vehicle:  " + scooters[player.SelectedScooter].name,
                        new Vector2(40, 545), Color.Black);
                    spriteBatch.DrawString(menuFont, "You have $" + player.Money.ToString(),
                        new Vector2(40, 570), Color.Black);
                    break;
                
                //display upgrade data
                case Menu.TuneShop :
                    spriteBatch.DrawString(menuFont, "Current Vehicle:  " + scooters[player.SelectedScooter].name,
                        new Vector2(40, 545), Color.Tan);
                    spriteBatch.DrawString(menuFont, "You have $" + player.Money.ToString(),
                        new Vector2(40,570), Color.Tan);

                    if (Menus[(int)Menu.TuneShop].SelectedButton != 10)
                    {
                        drawUpgradeSpecs(spriteBatch);
                    }
                    break;

                case Menu.Dealership :

                    spriteBatch.DrawString(menuFont, "You have $" + player.Money.ToString(),
                        new Vector2(350, 560), Color.Black);

                    if (Menus[(int)Menu.Dealership].SelectedButton < 6)
                    {
                        drawVehicleSpecs(spriteBatch, new Vector2(350, 150), 
                            Menus[(int)Menu.Dealership].SelectedButton - 1);
                    }

                    break;


                case Menu.VehicleSelect:

                    if (Menus[(int)Menu.VehicleSelect].SelectedButton != VSBackButton)
                        drawVehicleSpecs(spriteBatch, new Vector2(280, 190), scooterIDnums[Menus[(int)Menu.VehicleSelect].SelectedButton - 1],
                            player.UpgradeTotals[scooterIDnums[Menus[(int)Menu.VehicleSelect].SelectedButton - 1]]);
                    
                    break;
                    
                case Menu.Options:
                    spriteBatch.Draw(fixedImage, new Vector2(75, 150), Color.White);
                    break;

            }

            #endregion

            spriteBatch.Draw(CursorImage, Menus[(int)CurrentMenu].CursorPosition, Color.White);

            if (messageBox.Active)
                messageBox.draw(spriteBatch, menuFont, Color.DimGray, Color.Black);

            spriteBatch.End();

            #region draw scooter model for Garage and Dealership
            DepthStencilState newDepthStencilState = new DepthStencilState();
            DepthStencilState oldDepthStencilState = graphics.DepthStencilState;

            newDepthStencilState.DepthBufferFunction = CompareFunction.Less;
            graphics.DepthStencilState = newDepthStencilState;
            //graphics.ReferenceStencil = 1;

            if (CurrentMenu == Menu.VehicleSelect &&
                Menus[(int)CurrentMenu].SelectedButton != VSBackButton)
            {
                scooterYaw += 0.02f;
                scooterModels[scooterIDnums[Menus[(int)CurrentMenu].SelectedButton - 1]].rotate(scooterYaw);
                scooterModels[scooterIDnums[Menus[(int)CurrentMenu].SelectedButton - 1]].draw(view, proj, new Vector3(35,-30,-100));
            }

            else if (CurrentMenu == Menu.Dealership && Menus[(int)CurrentMenu].SelectedButton < 6)
            {
                scooterYaw += 0.02f;
                scooterModels[Menus[(int)CurrentMenu].SelectedButton - 1].rotate(scooterYaw);
                scooterModels[Menus[(int)CurrentMenu].SelectedButton - 1].draw(view, proj, new Vector3(4, -30, -100)); 
            }

            graphics.DepthStencilState = oldDepthStencilState;
            #endregion

        }

        public void loadVehicleSelect(ContentManager content, bool[] ScootersOwned)
        {
            short totalVehiclesOwned = 0;

            for (int i = 0; i < 8; i++)
            {
                if (ScootersOwned[i] == true)
                {
                    scooterIDnums[totalVehiclesOwned] = (short)i;
                    totalVehiclesOwned += 1;
                }
            }

            VSBackButton = (short)(totalVehiclesOwned + 1);
            Menus[(int)Menu.VehicleSelect] = new States.SubMenu((short)(totalVehiclesOwned + 1));
            Menus[(int)Menu.VehicleSelect].BackGround = content.Load<Texture2D>("Images\\Garage1");

            int y = 100;
            for (int i = 0; i < totalVehiclesOwned; i++)
            {

                Menus[(int)Menu.VehicleSelect].ButtonImage[i] = content.Load<Texture2D>("Images\\vehicle" + scooterIDnums[i].ToString());
                Menus[(int)Menu.VehicleSelect].ButtonPosition[i] = new Vector2(20, y);
                y += 45;
            }

            Menus[(int)Menu.VehicleSelect].ButtonImage[VSBackButton - 1] = content.Load<Texture2D>("Images\\BackButton");
            Menus[(int)Menu.VehicleSelect].ButtonPosition[VSBackButton - 1] = new Vector2(20, 550);
        }

        public void drawUpgradeSpecs(SpriteBatch spriteBatch)
        {

            spriteBatch.DrawString(menuFont,
                upgrades[Menus[(int)Menu.TuneShop].SelectedButton - 1].name,
                new Vector2(200, 100), Color.Tan);

            spriteBatch.DrawString(menuFont, 
                upgrades[Menus[(int)Menu.TuneShop].SelectedButton - 1].description1, 
                new Vector2(200, 140), Color.Tan);

            spriteBatch.DrawString(menuFont, 
                upgrades[Menus[(int)Menu.TuneShop].SelectedButton - 1].description2, 
                new Vector2(200, 160), Color.Tan);

            spriteBatch.DrawString(menuFont, 
                "Power:  " + upgrades[Menus[(int)Menu.TuneShop].SelectedButton - 1].power.ToString(), 
                new Vector2(200, 200), Color.Tan);

            spriteBatch.DrawString(menuFont, 
                "Weight:  " + upgrades[Menus[(int)Menu.TuneShop].SelectedButton - 1].weight.ToString(), 
                new Vector2(200, 220), Color.Tan);

            spriteBatch.DrawString(menuFont, 
                "Cost:  $" + upgrades[Menus[(int)Menu.TuneShop].SelectedButton - 1].cost.ToString(), 
                new Vector2(200, 240), Color.Tan);
        }

        public void drawVehicleSpecs(SpriteBatch spriteBatch, Vector2 location, int ID)
        {
            spriteBatch.DrawString(menuFont, scooters[ID].name,
                location, Color.Black);
            location.Y += 20;

            spriteBatch.DrawString(menuFont, "Power:  " + scooters[ID].outputPower.ToString(),
                location, Color.Black);
            location.Y += 20;

            spriteBatch.DrawString(menuFont, "Weight:  " + scooters[ID].weight.ToString(),
                location, Color.Black);
            location.Y += 20;

            spriteBatch.DrawString(menuFont, "Cost:  $" + scooters[ID].cost.ToString(),
                location, Color.Black);
        }

        public void drawVehicleSpecs(SpriteBatch spriteBatch, Vector2 location, 
            int ID, Objects.upgradeSpecs upgradeTotals)
        {
            spriteBatch.DrawString(menuFont, scooters[ID].name,
                location, Color.Black);
            location.Y += 20;

            spriteBatch.DrawString(menuFont, "Power:  " + (scooters[ID].outputPower + upgradeTotals.power).ToString(),
                location, Color.Black);
            location.Y += 20;

            spriteBatch.DrawString(menuFont, "Weight:  " + (scooters[ID].weight + upgradeTotals.weight).ToString(),
                location, Color.Black);
            location.Y += 20;

            spriteBatch.DrawString(menuFont, "Cost:  $" + scooters[ID].cost.ToString(),
                location, Color.Black);
        }


        public ScooterData[] ScooterSpecs
        {
            get { return scooters; }
        }
    }
}
