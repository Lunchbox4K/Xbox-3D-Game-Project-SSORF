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

//MenuManager will need functionality added to allow player to go 
//back and forth between different menus by setting the currentMenu

namespace SSORF.Management
{
    
    class MenuManager
    {

        //Which menu is currently being viewed?
        enum Menu
        {
            Main,
            VehicleSelect,
            Dealership,
            Missions,
            NumMenus
        }
        Menu CurrentMenu;
        //Needs a lot more submenus.
        States.SubMenu[] Menus;
        
        //each menu has the same cursor image so i made it a member of MenuManager
        private Texture2D CursorImage;

        //Menu manager has no "Active" state
        //if a mission other than zero is selected, state is switched
        //to a mission. If no mission is selected, menu is active. 
        public short selectedMission = 0;
       
        public MenuManager(ContentManager content)
        {
            Menus = new States.SubMenu[(int)Menu.NumMenus];
            //Load Main
            Menus[(int)Menu.Main] = new States.SubMenu(6); //mission menu has 2 buttons
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

            //Vehicleselect
           
            
            Menus[(int)Menu.VehicleSelect] = new States.SubMenu(3); //mission menu has 2 buttons
            Menus[(int)Menu.VehicleSelect].BackGround = content.Load<Texture2D>("Images\\VehicleTest");
            Menus[(int)Menu.VehicleSelect].ButtonImage[0] = content.Load<Texture2D>("Images\\vehicle1");
            Menus[(int)Menu.VehicleSelect].ButtonPosition[0] = new Vector2(100, 450);
            Menus[(int)Menu.VehicleSelect].ButtonImage[1] = content.Load<Texture2D>("Images\\vehicle2");
            Menus[(int)Menu.VehicleSelect].ButtonPosition[1] = new Vector2(450, 450);
            Menus[(int)Menu.VehicleSelect].ButtonImage[2] = content.Load<Texture2D>("Images\\TestBack");
            Menus[(int)Menu.VehicleSelect].ButtonPosition[2] = new Vector2(50, 70);

            //Dealership
            Menus[(int)Menu.Dealership] = new States.SubMenu(3); //mission menu has 2 buttons
            Menus[(int)Menu.Dealership].BackGround = content.Load<Texture2D>("Images\\VehicleTest");
            Menus[(int)Menu.Dealership].ButtonImage[0] = content.Load<Texture2D>("Images\\TestButton");
            Menus[(int)Menu.Dealership].ButtonPosition[0] = new Vector2(100, 450);
            Menus[(int)Menu.Dealership].ButtonImage[1] = content.Load<Texture2D>("Images\\TestButton");
            Menus[(int)Menu.Dealership].ButtonPosition[1] = new Vector2(450, 450);
            Menus[(int)Menu.Dealership].ButtonImage[2] = content.Load<Texture2D>("Images\\TestBack");
            Menus[(int)Menu.Dealership].ButtonPosition[2] = new Vector2(50, 70);

            //load missionsMenu stuff
            Menus[(int)Menu.Missions] = new States.SubMenu(4); //mission menu has 2 buttons
            Menus[(int)Menu.Missions].BackGround = content.Load<Texture2D>("Images\\menu");
            Menus[(int)Menu.Missions].ButtonImage[0] = content.Load<Texture2D>("Images\\button1");
            Menus[(int)Menu.Missions].ButtonPosition[0] = new Vector2(100, 300);
            Menus[(int)Menu.Missions].ButtonImage[1] = content.Load<Texture2D>("Images\\button2");
            Menus[(int)Menu.Missions].ButtonPosition[1] = new Vector2(450, 300);
            Menus[(int)Menu.Missions].ButtonImage[2] = content.Load<Texture2D>("Images\\TestButton");
            Menus[(int)Menu.Missions].ButtonPosition[2] = new Vector2(300, 350);
            Menus[(int)Menu.Missions].ButtonImage[3] = content.Load<Texture2D>("Images\\TestBack");
            Menus[(int)Menu.Missions].ButtonPosition[3] = new Vector2(50, 70);
            
            //load cursor image and set current menu to missions menu
            CursorImage = content.Load<Texture2D>("Images\\cursor");

            //This should be changed to main menu once we have it working
            CurrentMenu = Menu.Main;
            
        }

        public void update(GameTime gameTime, Objects.Player player)
        {

            Menus[(int)CurrentMenu].update(gameTime);

            //Here we could add logic to switch current menu if the back button is pressed, or return to main menu
            switch (CurrentMenu)
            { 

                case Menu.Main :
                   

                        if (Menus[(int)Menu.Main].buttonPressed == 1)
                            CurrentMenu = Menu.Missions;
                        else if (Menus[(int)Menu.Main].buttonPressed == 2)
                            CurrentMenu = Menu.VehicleSelect;
                        else if (Menus[(int)Menu.Main].buttonPressed == 3)
                            CurrentMenu = Menu.Dealership;
                        //else if (Menus[(int)Menu.Main].buttonPressed == 4)
                        //    CurrentMenu = Menu.VehicleSelect;
                        //else if (Menus[(int)Menu.Main].buttonPressed == 5)
                        //    CurrentMenu = Menu.VehicleSelect;
                        //else if (Menus[(int)Menu.Main].buttonPressed == 6)
                        //    CurrentMenu = Menu.VehicleSelect;
                        Menus[(int)Menu.Main].buttonPressed = 0;
                    
                    break;

                case Menu.Missions :

                    //Note: buttonPressed = 0 means no button has been pressed
                    //If we are in the missions menu and a button is pressed...
                    if (Menus[(int)Menu.Missions].buttonPressed == 4)
                        CurrentMenu = Menu.Main;
                    else if (Menus[(int)Menu.Missions].buttonPressed != 0)
                    {
                        //Set selected mission to a value other than zero to deactivate the menu
                        selectedMission = Menus[(int)Menu.Missions].buttonPressed;
                        //reset buttonPressed for when we return to the menu
                       
                    }
                    Menus[(int)Menu.Missions].buttonPressed = 0;
                    
                    break;

                case Menu.Dealership :
                    if (Menus[(int)Menu.Dealership].buttonPressed == 3)
                        CurrentMenu = Menu.Main;
                    Menus[(int)Menu.Dealership].buttonPressed = 0;
                    break;

                case Menu.VehicleSelect :
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

        //draw the current menu and the cursor
        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            Menus[(int)CurrentMenu].draw(spriteBatch);
            spriteBatch.Draw(CursorImage, Menus[(int)CurrentMenu].CursorPosition, Color.White);
            spriteBatch.End();
        }

    }
}
