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
    public class SimpleModel
    {
        private Model model;
        private Matrix worldMtx = Matrix.Identity;

        public void setPosition(Vector3 position)
        {
            worldMtx = Matrix.CreateTranslation(position);
        }

        public void rotate(float yaw)
        {
            worldMtx = Matrix.CreateRotationY(yaw) * worldMtx;

        }

        public void draw(ThirdPersonCamera Camera)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Projection = Camera.ProjMtx;
                    effect.View = Camera.ViewMtx;
                    //effect.World = mesh.ParentBone.Transform * worldMtx;

                    effect.World = transforms[mesh.ParentBone.Index] * WorldMtx;
                
                }
                mesh.Draw();
            }

        }//end draw

        public Model Mesh { get { return model; } set { model = value; } }
        public Matrix WorldMtx { get { return worldMtx; } set { worldMtx = value; } }

    }
}
