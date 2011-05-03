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


namespace _3DGame2.Objects
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
        protected Matrix scale;
        protected BoundingBox boundingBox;

        public StaticModel(ContentManager Content, string AssetLocation, 
            Vector3 Location, Matrix Orientation, Matrix Scale)
        {
            modelAsset = AssetLocation;
            content = Content;
            model = null;
            isLoaded = false;
            location = Location;
            orientation = Orientation;
            scale = Scale;
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

        /// <summary>
        /// Latest Update 4/26/11 - Fixed the calculator from only sending boxes
        /// at coord (0,0,0).
        /// 
        /// Use getBoundingBox to get the value you need.
        /// 
        /// Only Calculates Location. Not Rotations (Orientation).
        ///  >   Use the public rotate function to get a better result.
        /// </summary>
        private void calcBoundingBox()
        {
            // Initialize minimum and maximum corners of the bounding box to max and min values
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // For each mesh of the model
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    // Vertex buffer parameters
                    int vertexStride = part.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = part.NumVertices * vertexStride;

                    // Get vertex data as float
                    int vartexDataSize = vertexBufferSize / sizeof(float);
                    float[] vertexData = new float[vartexDataSize];
                    part.VertexBuffer.GetData<float>(vertexData);

                    // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
                    for (int i = 0; i < vartexDataSize; i += vertexStride / sizeof(float))
                    {
                        Vector3 transformedPosition = 
                            Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]), 
                                              Matrix.CreateTranslation(location));

                        min = Vector3.Min(min, transformedPosition);
                        max = Vector3.Max(max, transformedPosition);
                    }
                }
            }
            // Create and return bounding box
            boundingBox =  new BoundingBox(min, max);
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

        public BoundingBox getBoundingBox
        {
            get { return boundingBox; }
        }
        public Vector3 Location
        {
            get { return location; }
            set { location = value; }
        }
        public Matrix Scale
        {
            get { return scale; }
            set { scale = value; }
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
    }
}
