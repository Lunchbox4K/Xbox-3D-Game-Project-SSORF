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
            RaceState,
            NumMenus,
        }

        Objects.MessageBox messageBox = new Objects.MessageBox();

        Menu CurrentMenu;
        //Needs a lot more submenus.
        States.SubMenu[] Menus;
        
        //each menu has the same cursor image so i made it a member of MenuManager
        private Texture2D CursorImage;

        //Menu manager has no "Active" state
        //if a mission other than zero is selected, state is switched
        //to a mission. If no mission is selected, menu is active. 
        public short selectedMission = 0;
       
        //used to display upgrade data
        UpgradeData[] upgrades;

        SpriteFont menuFont;

        public MenuManager(ContentManager content)
        {
            //Load UpgradeData
            upgrades = content.Load<UpgradeData[]>("upgrades");
            //font for displaying upgrade data
            menuFont = content.Load<SpriteFont>("menuFont");

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

            #region Load VehicleSelect select owned vehicles
            Menus[(int)Menu.VehicleSelect] = new States.SubMenu(3); //mission menu has 2 buttons
            Menus[(int)Menu.VehicleSelect].BackGround = content.Load<Texture2D>("Images\\Garage1");
            Menus[(int)Menu.VehicleSelect].ButtonImage[0] = content.Load<Texture2D>("Images\\vehicle1");
            Menus[(int)Menu.VehicleSelect].ButtonPosition[0] = new Vector2(20, 140);
            Menus[(int)Menu.VehicleSelect].ButtonImage[1] = content.Load<Texture2D>("Images\\vehicle2");
            Menus[(int)Menu.VehicleSelect].ButtonPosition[1] = new Vector2(20, 190);
            Menus[(int)Menu.VehicleSelect].ButtonImage[2] = content.Load<Texture2D>("Images\\BackButton");
            Menus[(int)Menu.VehicleSelect].ButtonPosition[2] = new Vector2(20, 550);
            #endregion

            #region Load Dealership buy new vehicles
            Menus[(int)Menu.Dealership] = new States.SubMenu(4); //mission menu has 2 buttons
            Menus[(int)Menu.Dealership].BackGround = content.Load<Texture2D>("Images\\Dealership1");
            Menus[(int)Menu.Dealership].ButtonImage[0] = content.Load<Texture2D>("Images\\TestButton");
            Menus[(int)Menu.Dealership].ButtonPosition[0] = new Vector2(20, 150);
            Menus[(int)Menu.Dealership].ButtonImage[1] = content.Load<Texture2D>("Images\\TuneShopButton");
            Menus[(int)Menu.Dealership].ButtonPosition[1] = new Vector2(20, 250);
            Menus[(int)Menu.Dealership].ButtonImage[2] = content.Load<Texture2D>("Images\\BackButton");
            Menus[(int)Menu.Dealership].ButtonPosition[2] = new Vector2(20, 550);
            Menus[(int)Menu.Dealership].ButtonImage[3] = content.Load<Texture2D>("Images\\Purchase Button");
            Menus[(int)Menu.Dealership].ButtonPosition[3] = new Vector2(150, 540);
            #endregion

            #region Load TuneShop buy upgrades
            Menus[(int)Menu.TuneShop] = new States.SubMenu(4); //tune shop has 4 buttons
            Menus[(int)Menu.TuneShop].BackGround = content.Load<Texture2D>("Images\\TuneShopTest");
            for (int i = 0; i < 3; i++)
                Menus[(int)Menu.TuneShop].ButtonImage[i] = content.Load<Texture2D>("Images\\" + upgrades[i].button);    
            Menus[(int)Menu.TuneShop].ButtonPosition[0] = new Vector2(150, 350);
            Menus[(int)Menu.TuneShop].ButtonPosition[1] = new Vector2(250, 350);
            Menus[(int)Menu.TuneShop].ButtonPosition[2] = new Vector2(350, 350);
            Menus[(int)Menu.TuneShop].ButtonImage[3] = content.Load<Texture2D>("Images\\BackButton");
            Menus[(int)Menu.TuneShop].ButtonPosition[3] = new Vector2(25, 70);
            #endregion

            #region Load MissionsMenu
            Menus[(int)Menu.Missions] = new States.SubMenu(3); //mission menu has 2 buttons
            Menus[(int)Menu.Missions].BackGround = content.Load<Texture2D>("Images\\menu");
            Menus[(int)Menu.Missions].ButtonImage[0] = content.Load<Texture2D>("Images\\button1");
            Menus[(int)Menu.Missions].ButtonPosition[0] = new Vector2(100, 300);
            Menus[(int)Menu.Missions].ButtonImage[1] = content.Load<Texture2D>("Images\\button2");
            Menus[(int)Menu.Missions].ButtonPosition[1] = new Vector2(450, 300);
            Menus[(int)Menu.Missions].ButtonImage[2] = content.Load<Texture2D>("Images\\BackButton");
            Menus[(int)Menu.Missions].ButtonPosition[2] = new Vector2(600, 550);
            #endregion

            //load messageBox background
            messageBox.Background = content.Load<Texture2D>("Images\\messagebox");

            //load cursor image and set current menu to main menu
            CursorImage = content.Load<Texture2D>("Images\\cursor");
            CurrentMenu = Menu.Main;



        }

        public void update(GameTime gameTime, Objects.Player player)
        {
            if (!messageBox.Active)
            {

                Menus[(int)CurrentMenu].update(gameTime);

                switch (CurrentMenu)
                {

                    case Menu.Main:

                        if (Menus[(int)Menu.Main].buttonPressed == 1)
                            CurrentMenu = Menu.Missions;
                        else if (Menus[(int)Menu.Main].buttonPressed == 2)
                            CurrentMenu = Menu.Dealership;
                        else if (Menus[(int)Menu.Main].buttonPressed == 3)
                            CurrentMenu = Menu.VehicleSelect;
                        //else if (Menus[(int)Menu.Main].buttonPressed == 4)
                        //    CurrentMenu = Menu.VehicleSelect;
                        //else if (Menus[(int)Menu.Main].buttonPressed == 5)
                        //    CurrentMenu = Menu.VehicleSelect;
                        //else if (Menus[(int)Menu.Main].buttonPressed == 6)
                        //    CurrentMenu = Menu.VehicleSelect;
                        Menus[(int)Menu.Main].buttonPressed = 0;

                        break;

                    case Menu.Missions:

                        //Note: buttonPressed = 0 means no button has been pressed
                        //If we are in the missions menu and a button is pressed...
                        if (Menus[(int)Menu.Missions].buttonPressed == 3)
                            CurrentMenu = Menu.Main;
                        else if (Menus[(int)Menu.Missions].buttonPressed != 0)
                        {
                            //Set selected mission to a value other than zero to deactivate the menu
                            selectedMission = Menus[(int)Menu.Missions].buttonPressed;
                            //reset buttonPressed for when we return to the menu

                        }
                        Menus[(int)Menu.Missions].buttonPressed = 0;

                        break;

                    case Menu.Dealership:
                        if (Menus[(int)Menu.Dealership].buttonPressed == 3)
                            CurrentMenu = Menu.Main;
                        if (Menus[(int)Menu.Dealership].buttonPressed == 2)
                            CurrentMenu = Menu.TuneShop;

                        Menus[(int)Menu.Dealership].buttonPressed = 0;
                        break;

                    case Menu.TuneShop:
                        if (Menus[(int)Menu.TuneShop].buttonPressed == 4)
                            CurrentMenu = Menu.Dealership;

                        if (Menus[(int)Menu.TuneShop].buttonPressed > 0 &&
                            Menus[(int)Menu.TuneShop].buttonPressed < 4)
                        {
                            string TuneShopMessage = player.PurchaseUpgrade(
                            upgrades[Menus[(int)Menu.TuneShop].buttonPressed - 1]);
                            messageBox.setMessage("Message", TuneShopMessage);
                            messageBox.Active = true;
                        }

                        Menus[(int)Menu.TuneShop].buttonPressed = 0;
                        break;

                    case Menu.VehicleSelect:
                        if (Menus[(int)Menu.VehicleSelect].buttonPressed == 3)
                            CurrentMenu = Menu.Main;
                        if (Menus[(int)Menu.VehicleSelect].buttonPressed > 0 &&
                            Menus[(int)Menu.VehicleSelect].buttonPressed < 3)
                        {
                            player.SelectedScooter = Menus[(int)Menu.VehicleSelect].buttonPressed;
                            CurrentMenu = Menu.Main;
                        }
                        Menus[(int)Menu.VehicleSelect].buttonPressed = 0;
                        break;

                    //etc.
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

            
            switch (CurrentMenu)
            {
                
                //display upgrade data
                case Menu.TuneShop :

                    spriteBatch.DrawString(menuFont, "You have $" + player.Money.ToString(),
                        new Vector2(50,550), Color.Tan);

                    if (Menus[(int)Menu.TuneShop].SelectedButton != 4)
                    {
                        drawUpgradeSpecs(spriteBatch);
                    }
                    break;
            }

            spriteBatch.Draw(CursorImage, Menus[(int)CurrentMenu].CursorPosition, Color.White);

            if (messageBox.Active)
                messageBox.draw(spriteBatch, menuFont, Color.SlateGray, Color.Black);

            spriteBatch.End();
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

    }
}
