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

//This class keeps track of when we are in the menus or playing 
//a mission and draws the appropriate scene. Also switches back 
//to menu state when a mission is completed
namespace SSORF.Management
{
    //What are we viewing?
    public enum GameState
    { 
        TitleScreen,
        MenuScreen,
        MissionScreen
    }

    

  

    public class StateManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        //Start with title screen
        GameState State = GameState.TitleScreen;

        SpriteBatch spriteBatch;

        SpriteFont font;

        States.Title title;

        MenuManager menu;

        States.Mission currentMission;

        //Player stats such as money and scooters will need to be available 
        //to both mission and menus so I included it with the state manager.
        Objects.Player player = new Objects.Player();


        public StateManager(Game game)
            : base(game)
        {

        }

        //load content for title and menu screens
        protected override void LoadContent()
        {
            ContentManager content = Game.Content;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Game.Content.Load<SpriteFont>("font");
            
            title = new States.Title();
            title.Image = Game.Content.Load<Texture2D>("Images\\senior scooter title screen");

            menu = new MenuManager(Game.Content);

            currentMission = new States.Mission();

        }

        //Do we need to use this? I'm not sure
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            //update controller if on XBOX, update keyboard otherwise
#if XBOX
            gamePadState.current = GamePad.GetState(PlayerIndex.One);
# else
            keyBoardState.current = Keyboard.GetState();
            gamePadState.current = GamePad.GetState(PlayerIndex.One);
#endif
            //What we update depends on the current GameState
            switch (State)
            { 
                // If we are viewing title screen...
                case GameState.TitleScreen:
                    // update title screen
                    title.update(gameTime);
                    //if title is exited during update...
                    if (title.Active == false)
                    {//switch to menu and update it
                        State = GameState.MenuScreen;
                        menu.update(gameTime, player);
                    }
                    break;

                // If we are viewing the menus...
                case GameState.MenuScreen:
                    //update menu
                    menu.update(gameTime, player);
                    //if mission is selected during update...
                    if (menu.selectedMission != 0)
                    {   // switch to mission, load it, then update it
                        State = GameState.MissionScreen;
                        currentMission = new States.Mission(player, Game.Content, Game.GraphicsDevice.Viewport.AspectRatio);
                        currentMission.load(Game.Content, menu.selectedMission);
                        currentMission.update(gameTime);
                        menu.selectedMission = 0;
                    }
                    break;

                // If we are playing a mission...
                case GameState.MissionScreen:
                    //update mission
                    currentMission.update(gameTime);
                    //If mission is exited or completed during update...
                    if (currentMission.Active == false)
                    {//switch back to menu and update it
                        State = GameState.MenuScreen;
                        menu.update(gameTime, player);
                    }
                    break;
            }

            base.Update(gameTime);
#if XBOX
            gamePadState.previous = gamePadState.current;
# else
            keyBoardState.previous = keyBoardState.current;
#endif
            //Sometimes you need to check the previous input so the same upgrade 
            //won't get purchased 5 times in one second if a button is held down.
            //previous input can be used to check and see if they released the 
            //button before pushing it again
        }



        //draw title, menu, or mission depending on GameState
        public override void Draw(GameTime gameTime)
        {

            switch (State)
            {
                case GameState.TitleScreen:
                    title.draw(spriteBatch);
                    break;

                case GameState.MenuScreen:
                    menu.draw(spriteBatch);
                    break;

                case GameState.MissionScreen:
                    currentMission.draw(spriteBatch, gameTime);
                    break;
            }
            

        }
    }
}
