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
        public Matrix WorldMtx = Matrix.Identity;

        public void rotate(float yaw)
        {
            WorldMtx = Matrix.CreateRotationY(yaw);

        }

        public void draw(Matrix View, Matrix Proj, Vector3 position)
        {
            WorldMtx *= Matrix.CreateTranslation(position);
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Projection = Proj;
                    effect.View = View;
                    //effect.World = mesh.ParentBone.Transform * worldMtx;

                    effect.World = transforms[mesh.ParentBone.Index] * WorldMtx;
                
                }
                mesh.Draw();
            }

        }//end draw

        public void draw(Matrix View, Matrix Proj)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Projection = Proj;
                    effect.View = View;
                    //effect.World = mesh.ParentBone.Transform * worldMtx;

                    effect.World = transforms[mesh.ParentBone.Index] * WorldMtx;

                }
                mesh.Draw();
            }
        }

        public Model Mesh { get { return model; } set { model = value; } }
        public Vector3 Position { get { return WorldMtx.Translation; } }

    }
}
