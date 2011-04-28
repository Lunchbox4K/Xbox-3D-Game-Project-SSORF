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
            //load missionsMenu stuff
            Menus[(int)Menu.Missions] = new States.SubMenu(2); //mission menu has 2 buttons
            Menus[(int)Menu.Missions].BackGround = content.Load<Texture2D>("Images\\menu");
            Menus[(int)Menu.Missions].ButtonImage[0] = content.Load<Texture2D>("Images\\button1");
            Menus[(int)Menu.Missions].ButtonPosition[0] = new Vector2(100, 300);
            Menus[(int)Menu.Missions].ButtonImage[1] = content.Load<Texture2D>("Images\\button2");
            Menus[(int)Menu.Missions].ButtonPosition[1] = new Vector2(450, 300);
            
            //load cursor image and set current menu to missions menu
            CursorImage = content.Load<Texture2D>("Images\\cursor");

            //This should be changed to main menu once we have it working
            CurrentMenu = Menu.Missions;
            
        }

        public void update(GameTime gameTime)
        {

            Menus[(int)CurrentMenu].update(gameTime);

            //Here we could add logic to switch current menu if the back button is pressed, or return to main menu
            switch (CurrentMenu)
            { 
                case Menu.Missions :

                    //Note: buttonPressed = 0 means no button has been pressed
                    //If we are in the missions menu and a button is pressed...
                    if (Menus[(int)Menu.Missions].buttonPressed != 0)
                    {
                        //Set selected mission to a value other than zero to deactivate the menu
                        selectedMission = Menus[(int)Menu.Missions].buttonPressed;
                        //reset buttonPressed for when we return to the menu
                        Menus[(int)Menu.Missions].buttonPressed = 0;
                    }

                    
                    break;

                case Menu.VehicleSelect :
                    //Code goes here
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
