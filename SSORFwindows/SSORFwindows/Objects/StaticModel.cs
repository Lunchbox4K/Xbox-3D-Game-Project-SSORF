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
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class StaticModel
    {
        public static StaticModel[] modelList;

        protected ContentManager content;
        protected Model model;
        protected bool isLoaded;
        protected bool isEnabled;
        protected string modelAsset;

        protected Vector3 location;
        protected Matrix orientation;
        protected float scale;
        protected Matrix transform;
        protected Vector3 velocity;

        protected BoundingSphere boundingSphere;
        protected List<BoundingSphere> meshSpheres;

        protected static int modelIDloop;
        protected int modelID;

        //For some reason, bounding box collision detection was not working
        //so I added this as a temporary fix...
        public bool TemporaryCollisionDetection(StaticModel otherModel)
        { 
            Matrix thisWorld = scale * orientation * Matrix.CreateTranslation(location);
            Matrix otherWorld = otherModel.Scale * otherModel.Orientation 
                * Matrix.CreateTranslation(otherModel.Location);

            //Loop through model meshes and compare bounding spheres
            foreach (ModelMesh theseMeshes in model.Meshes)
            {
                foreach (ModelMesh otherMeshes in otherModel.Geometry.Meshes)
                {
                    if (theseMeshes.BoundingSphere.Transform(thisWorld).Intersects(
                        otherMeshes.BoundingSphere.Transform(otherWorld)))
                        return true;
                }
            }
            return false;
        }

        public StaticModel(ContentManager Content, string AssetLocation, 
            Vector3 Location, Matrix Orientation, float Scale)
        {
        modelIDloop += 1;
            modelID = modelIDloop;
            modelAsset = AssetLocation;
            content = Content;
            model = null;
            isLoaded = false;
            location = Location;
            orientation = Orientation;
            scale = Scale;
            velocity = Vector3.Zero;
            transform = Matrix.Identity;
            //Matrix.Multiply(ref scale, ref orientation, out transform);
            transform *= Matrix.CreateScale(scale) * orientation;
            transform = Matrix.Multiply(transform, Matrix.CreateTranslation(location));

        }

        protected void regModelToList()
        {
            if (model == null)
                throw new InvalidCastException("Invalid Asset");
            calcBoundingSpheres();
            if (modelList == null)
            {
                modelList = new StaticModel[1];
                modelList[0] = this;
            }
            else
            {
                Array.Resize<StaticModel>(ref modelList, modelList.Length + 1);
                modelList[modelList.Length - 1] = this;
            }
        }
        protected void unregModelFromList()
        {
            if (modelList.Length <= 1)
                modelList = null;
            else
            {
                //Remove model from active model list
                StaticModel[] tmpList = modelList;
                Array.Resize<StaticModel>(ref modelList, modelList.Length - 1);
                int j = 0;
                for (int i = 0; i < modelList.Length; i++)
                {
                    if (this.ID == tmpList[i].ID)
                        j++;
                    modelList[i] = tmpList[j];
                    j++;
                }
            }
        }

        public virtual void LoadModel()
        {
            try
            {
                model = content.Load<Model>(modelAsset);
                regModelToList();
                isLoaded = true;
            }
            catch
            {
                throw new InvalidCastException("Invalid Asset");
            }
        }

        protected void calcBoundingSpheres()
        {
            //Transform for
            BoundingSphere mainSphere = new BoundingSphere(Vector3.Zero, 0f);
            meshSpheres = new List<BoundingSphere>();
            //Loop through each mesh and save the bounding sphere
            foreach (ModelMesh theseMeshes in model.Meshes)
            {
                BoundingSphere tmpSphere = new BoundingSphere();
                tmpSphere.Center = theseMeshes.BoundingSphere.Center;
                tmpSphere.Radius = theseMeshes.BoundingSphere.Radius;
                meshSpheres.Add(tmpSphere);
                mainSphere = BoundingSphere.CreateMerged(mainSphere, tmpSphere);
            }
            mainSphere.Center = location;
            boundingSphere = mainSphere;
        }

        private void updateBoundingSpheres()
        {
            Matrix locationTranslation = Matrix.CreateTranslation(location);
            boundingSphere.Transform(locationTranslation);
            for (int i = 0; i < meshSpheres.Count; i++)
                meshSpheres[i].Transform(locationTranslation);
        }

        public virtual void UnloadModel()
        {
            if (isLoaded)
            {
                model = null;
                unregModelFromList();

                isLoaded = false;

            }
        }

        public virtual void drawModel(GameTime gameTime, Matrix view, Matrix projection)
        {
            if (isLoaded)
            {

                Matrix[] boneTransforms = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(boneTransforms);
                Matrix worldMatrix = scale * orientation * Matrix.CreateTranslation(location);

                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.World = boneTransforms[mesh.ParentBone.Index] * worldMatrix;
                        effect.View = view;
                        effect.Projection = projection;

                        effect.EnableDefaultLighting();
                        effect.PreferPerPixelLighting = true;

                        // Set the fog to match the black background color
                        effect.FogEnabled = false;
                        effect.FogColor = Vector3.Zero;
                        effect.FogStart = 1000;
                        effect.FogEnd = 3200;
                    }

                    mesh.Draw();
                }
            }
        }

        public BoundingSphere GetBoundingSphere
        {
            get{ return boundingSphere; }
        }

        public BoundingSphere GetBoundingSphereTransform(Matrix transform)
        {
            BoundingSphere tmp = boundingSphere;
            return tmp.Transform(transform);
        }

        public BoundingSphere[] GetBoundingSpheres
        {
            get {return meshSpheres.ToArray();}
        }

        //For temp collision detection
        public Model Geometry
        { get { return model; } }

        public Vector3 Location
        {
            get { return location; }
            set { location = value; }
        }

        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public bool IsLoaded
        {
            get { return isLoaded; }
        }

        public Matrix Orientation
        {
            get { return orientation; }
            set { orientation = value; }
        }

        public string ModelAsset
        {
            get { return modelAsset; }
        }

        public int ID
        {
            get { return modelID; }
        }

        public Vector3 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
    }
}
