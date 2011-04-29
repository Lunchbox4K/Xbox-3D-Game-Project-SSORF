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
        protected ContentManager content;
        protected Model model;
        protected bool isLoaded;
        protected string modelAsset;
        protected Vector3 location;
        protected Matrix orientation;
        protected BoundingBox boundingBox;

        public StaticModel(ContentManager Content, string AssetLocation, 
            Vector3 Location, Matrix Orientation)
        {
            modelAsset = AssetLocation;
            content = Content;
            model = null;
            isLoaded = false;
            location = Location;
            orientation = Orientation;
        }
        public virtual void LoadModel()
        {
            try
            {
                model = content.Load<Model>(modelAsset);
                if (model == null)
                    throw new InvalidCastException("Invalid Asset");
                calcBoundingBox();
                isLoaded = true;
            }
            catch
            {
                throw new InvalidCastException("Invalid Asset");
            }
        }
        private void calcBoundingBox()
        {
            Matrix[] boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            Vector3 modelMin = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3 modelMax = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            foreach (ModelMesh mesh in model.Meshes)
            {
                Vector3 meshMin = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                Vector3 meshMax = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    //Stride is the size of the vertex in bytes
                    int stride = part.VertexBuffer.VertexDeclaration.VertexStride;
                    byte[] vertexData = new byte[stride * part.NumVertices];
                    part.VertexBuffer.GetData(part.VertexOffset, vertexData, part.NumVertices, 1, stride);

                    Vector3 vertPosition=new Vector3();
                    for (int ndx = 0; ndx < vertexData.Length; ndx += stride)
                    {
                        vertPosition.X = BitConverter.ToSingle(vertexData, ndx);
                        vertPosition.Y = BitConverter.ToSingle(vertexData, ndx + sizeof(float));
                        vertPosition.Z = BitConverter.ToSingle(vertexData, ndx + sizeof(float) * 2);

                        // update our running values from this vertex
                        meshMin = Vector3.Min(meshMin, vertPosition);
                        meshMax = Vector3.Max(meshMax, vertPosition);
                    }
                }
                // transform by mesh bone transforms
                meshMin = Vector3.Transform(meshMin, boneTransforms[mesh.ParentBone.Index]);
                meshMax = Vector3.Transform(meshMax, boneTransforms[mesh.ParentBone.Index]);
                // Expand model extents by the ones from this mesh
                modelMin = Vector3.Min(modelMin, meshMin);
                modelMax = Vector3.Max(modelMax, meshMax);
            }
            // Create and return the model bounding box
            boundingBox = new BoundingBox(modelMin, modelMax);
        }

        public virtual void UnloadModel()
        {
            if (isLoaded)
            {
                model = null;
                isLoaded = false;
            }
        }
        public virtual void drawModel(GameTime gameTime, Matrix view, Matrix projection)
        {
            if (isLoaded)
            {

                Matrix[] boneTransforms = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(boneTransforms);
                Matrix worldMatrix = orientation * Matrix.CreateTranslation(location);

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

        public BoundingBox getBoundingBox
        {
            get { return boundingBox; }
        }
        public Vector3 Location
        {
            get { return location; }
            set { location = value; }
        }
        public string ModelAsset
        {
            get { return modelAsset; }
        }
    }
}
