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
        //Needs a lot more submenus.
        States.SubMenu MissionsMenu;
        //Which menu is currently being viewed?
        States.SubMenu CurrentMenu;
        //each menu has the same cursor image so i made it a member of MenuManager
        private Texture2D CursorImage;

        //Menu manager has no "Active" state
        //if a mission other than zero is selected, state is switched
        //to a mission. If no mission is selected, menu is active. 
        public short selectedMission = 0;

        public MenuManager(ContentManager content)
        {
            //load missionsMenu stuff
            MissionsMenu = new States.SubMenu(2); //mission menu has 2 buttons
            MissionsMenu.BackGround = content.Load<Texture2D>("Textures\\menu");
            MissionsMenu.ButtonImage[0] = content.Load<Texture2D>("Textures\\button1");
            MissionsMenu.ButtonPosition[0] = new Vector2(100, 300);
            MissionsMenu.ButtonImage[1] = content.Load<Texture2D>("Textures\\button2");
            MissionsMenu.ButtonPosition[1] = new Vector2(450, 300);
            
            //load cursor image and set current menu to missions menu
            CursorImage = content.Load<Texture2D>("Textures\\cursor");
            CurrentMenu = MissionsMenu;
        }

        public void update(GameTime gameTime)
        {
            
            CurrentMenu.update(gameTime);
            //After updating the menu we could check to see 
            //if we need to return to a previous menu or proceed 
            //to the next menu and set the currentMenu to something else;

            //If we are in the missions menu and a button is pressed...
            if (CurrentMenu == MissionsMenu && CurrentMenu.buttonPressed != 0)
            {
                //Set selected mission to a value other than zero to deactivate the menu
                selectedMission = CurrentMenu.buttonPressed;
                //reset buttonPressed for when we return to the menu
                CurrentMenu.buttonPressed = 0;
            }
        }

        //draw the current menu and the cursor
        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            CurrentMenu.draw(spriteBatch);
            spriteBatch.Draw(CursorImage, CurrentMenu.CursorPosition, Color.White);
            spriteBatch.End();
        }

    }
}
