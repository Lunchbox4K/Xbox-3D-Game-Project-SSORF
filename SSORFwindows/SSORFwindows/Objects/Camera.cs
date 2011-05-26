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

namespace SSORF.Objects
{
    public class ThirdPersonCamera
    {
        private Matrix viewMtx;
        private Matrix projMtx;
        private Viewport viewport;
        private Vector3 cameraPosition;
        //Offset represents the distance between player and camera
        private Vector3 offset;

        //constructor
        public ThirdPersonCamera()
        {
            offset = new Vector3(0, 38, 60);
            cameraPosition = offset;
            viewMtx = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
        }

        public void update(Vector3 PlayerPosition, float PlayerYaw)
        { 
            //Camera rotation is equal to player rotation
            Matrix rotationMtx = Matrix.CreateRotationY(PlayerYaw);
            //Rotate the offset vector
		    Vector3 rotatedOffset = Vector3.Transform(offset, rotationMtx);
            //Move camera with player
            cameraPosition = rotatedOffset + PlayerPosition;
            //Reconstruct view matrix so camera is pointed at the player
		    viewMtx = Matrix.CreateLookAt(cameraPosition, PlayerPosition + rotationMtx.Forward * 70, Vector3.Up);
        
        }

        //accessors and mutators

        public Vector3 Position { get { return cameraPosition; } }

        public Matrix ViewMtx { get { return viewMtx; } set { viewMtx = value; } }

        public Matrix ProjMtx { get { return projMtx; } set { projMtx = value; } }
     
        public Viewport ViewPort { get { return viewport; } set { viewport = value; } }

    }

   
}
