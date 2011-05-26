using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace SSORF.Objects
{
    class InstancedModel : StaticModel
    {
        public const byte MAX_TRANSFORMS = 64;
        //public readonly string EFFECT_LOCATION = "Effects\\InstancedModelEffect";
        
        //Used to send instance transformations to the HLSL Effect
        static VertexDeclaration instanceVertDeclaration = new VertexDeclaration
        (
            new VertexElement(0,  VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
        );
        private Matrix[] instanceLocations;
        private Matrix[] instanceRootBones;

        //Used to multiply agenst one model instance
        private List<Matrix> locations;
        //Used to run vertex math on GPU
        private DynamicVertexBuffer instanceVertBuffer;

        public InstancedModel(ContentManager Content, string AssetLocation)
            : base(Content, AssetLocation, Vector3.Zero, Matrix.Identity, Matrix.Identity)
        {
            locations = new List<Matrix>();
        }

        public override void LoadModel()
        {
            model = content.Load<Model>(modelAsset);
            //Effect tmpEffect = content.Load<Effect>(EFFECT_LOCATION);
            if (model == null)
                throw new InvalidCastException("Invalid Asset");
            instanceRootBones = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(instanceRootBones);

            isLoaded = true;
        }


        public void addModel(Matrix Transform)
        {
            locations.Add(Transform);
        }
        public void addModel(Vector3 Location, Matrix Orientation)
        {
            Matrix tmp = Matrix.Identity;
            Matrix scale;
            Matrix.CreateScale(0.1f, out scale);
            Matrix rotation = Matrix.Identity;
            Matrix.Multiply(ref scale, ref rotation, out tmp);
            tmp.M41 += Location.X*22;
            tmp.M42 += Location.Y*22;
            tmp.M43 += Location.Z*22;
            addModel(tmp);
        }

        public void draw
            (GameTime gameTime, GraphicsDevice graphics, Matrix View, Matrix Projection)
        {
            // Set renderstates for drawing 3D models.
            graphics.BlendState = BlendState.Opaque;
            graphics.DepthStencilState = DepthStencilState.Default;


            // Gather instance transform matrices into a single array.
            if ( instanceLocations == null || instanceLocations.Length != locations.Count)
                Array.Resize(ref instanceLocations, MAX_TRANSFORMS);

            for(int i = 0; i < locations.Count; i++)
            {
                for (int j = 0; j < MAX_TRANSFORMS; j++)
                {
                    if (i < locations.Count)
                        instanceLocations[j] = locations[i];
                    i++;
                }
                drawInstances(graphics, View, Projection);
            }
            
        }

        

        private void drawInstances(GraphicsDevice graphics, Matrix View,
                                  Matrix Projection)
        {
            if (instanceLocations.Length == 0)
                return;
            //Maske sure the buffer contains all the instances
            if ((instanceVertBuffer == null) || instanceVertBuffer.IsDisposed ||
                (instanceLocations.Length > instanceVertBuffer.VertexCount))
            {
                //if (instanceVertBuffer != null)
                //    instanceVertBuffer.Dispose();

                instanceVertBuffer = new DynamicVertexBuffer(graphics, instanceVertDeclaration,
                                                               instanceLocations.Length, BufferUsage.WriteOnly);
            }
            // Update transformations for each pass.
            instanceVertBuffer.SetData(instanceLocations, 0, instanceLocations.Length, SetDataOptions.Discard);

            //Draw Loop
            foreach(ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    //Tell the GPU to take the models vertex and our own custom vertex.
                    graphics.SetVertexBuffers(
                        new VertexBufferBinding(part.VertexBuffer, part.VertexOffset, 0),
                        new VertexBufferBinding(instanceVertBuffer, 0, 1)
                    );

                    graphics.Indices = part.IndexBuffer;

                    // Set up the instance rendering effect.
                    Effect effect = part.Effect;

                    effect.CurrentTechnique = effect.Techniques["HwInstancing"];

                    effect.Parameters["mWorld"].SetValue(instanceRootBones[mesh.ParentBone.Index]);
                    effect.Parameters["mView"].SetValue(View);
                    effect.Parameters["mProjection"].SetValue(Projection);

                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        graphics.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                               part.NumVertices, part.StartIndex,
                                                               part.PrimitiveCount, instanceLocations.Length);
                    }
                }
                instanceVertBuffer.Dispose();
        }

        public List<Matrix> Locations
        {
            get { return locations; }
            set { locations = value; }
        }
    }
}
