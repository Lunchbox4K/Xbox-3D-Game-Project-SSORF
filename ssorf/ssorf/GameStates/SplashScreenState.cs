using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ssorf.GameStates
{
    class SplashScreenState : Base.GameState
    {
        Texture2D splashImage;

        public SplashScreenState(Game game):base(game)
        {
            base.stateID = (byte)GameStateIdentifiers.SplashScreen;
        }

        public override void Initialize()
        {
            splashImage = Game.Content.Load<Texture2D>(@"800x600SplashScreen");
            base.Initialize();
        }

        public override void Draw(GameTime gameTime)
        {
            Management.GameStateManager.sb.Begin();
            Management.GameStateManager.sb.Draw(splashImage, GraphicsDevice.Viewport.Bounds, Color.White);
            Management.GameStateManager.sb.End();
            base.Draw(gameTime);
        }
    }
}
