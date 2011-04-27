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
using SSORF.Management;

//Contains 3D model and specs for a scooter
namespace SSORF.Objects
{
    public class Vehicle
    {
        Model geometry;
        Matrix rotationMtx;
        Vector3 position = Vector3.Zero;
        Vector3 lookVector = Vector3.Zero;
        float power = 1.4f;
        float yaw = 0.0f;
        //vehicle still needs list of specs such as weight, name, etc
        //Also need a way to add upgrades to vehicles

        // How long until we should play the next sound.
        TimeSpan timeDelay = TimeSpan.Zero;

        // Random number generator for choosing between sound variations.
        static Random random = new Random();

        public void load(ContentManager content, short vehicleID)
        {
            geometry = content.Load<Model>("Models\\scooter" + vehicleID.ToString());

        //This will also need to load vehicle specs from a file using vehicle ID
        }


        public void update(GameTime gameTime, AudioManager audioManager)
        {
            //If XBOX use the joystick..
#if XBOX
            if (gamePadState.current.ThumbSticks.Left.X > 0.0f)
                yaw += (float)gameTime.ElapsedGameTime.TotalMilliseconds *
                     MathHelper.ToRadians(-0.2f) * gamePadState.current.ThumbSticks.Left.X;

            if (gamePadState.current.ThumbSticks.Left.X < 0.0f)
                yaw += (float)gameTime.ElapsedGameTime.TotalMilliseconds *
                    MathHelper.ToRadians(0.2f) * -gamePadState.current.ThumbSticks.Left.X;

            rotationMtx = Matrix.CreateFromYawPitchRoll(yaw, 0.0f, 0.0f);

            if (gamePadState.current.ThumbSticks.Left.Y > 0.0f)
                position += rotationMtx.Forward * power * gamePadState.current.ThumbSticks.Left.Y;

            if (gamePadState.current.ThumbSticks.Left.Y < 0.0f)
                position -= rotationMtx.Forward * power * -gamePadState.current.ThumbSticks.Left.Y;
#else
            //Otherwise use the direction keys...

            if (keyBoardState.current.IsKeyDown(Keys.Right))
                yaw += (float)gameTime.ElapsedGameTime.TotalMilliseconds *
                     MathHelper.ToRadians(-0.2f);

            if (keyBoardState.current.IsKeyDown(Keys.Left))
                yaw += (float)gameTime.ElapsedGameTime.TotalMilliseconds *
                    MathHelper.ToRadians(0.2f);

            //Update rotations so the player will move forward in 
            //a different direction as they turn the vehicle
            rotationMtx = Matrix.CreateFromYawPitchRoll(yaw, 0.0f, 0.0f);

            if (keyBoardState.current.IsKeyDown(Keys.Up))
                position += rotationMtx.Forward * power;

            if (keyBoardState.current.IsKeyDown(Keys.Down))
                position -= rotationMtx.Forward * power;

#endif
            // Update Sounds
            // If the time delay has run out, trigger another single-shot sound.
            timeDelay -= gameTime.ElapsedGameTime;

            if (timeDelay < TimeSpan.Zero)
            {
                // For variety, randomly choose between three slightly different
                // variants of the sound (CatSound0, CatSound1, and CatSound2).
                string soundName = "CatSound" + random.Next(3);

                audioManager.Play3DSound(soundName, false, this);

                timeDelay += TimeSpan.FromSeconds(1.25f);
            }
        
        
        }
 
        //Accessors and Mutators
        public Model Geometry { get { return geometry; } set { geometry = value; } }
    
        public Matrix RotationMtx { get { return rotationMtx; } }

        public Vector3 Position { get { return position; } set { position = value; } }

        public float Yaw { get { return yaw; } set { yaw = value; } }

    }
}
