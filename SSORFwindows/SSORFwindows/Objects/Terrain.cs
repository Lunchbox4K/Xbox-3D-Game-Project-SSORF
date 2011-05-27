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

using Microsoft.Xna.Framework.Net;


namespace SSORF.Objects
{
    #region Terrain Info

    /// <summary>
    /// 
    /// </summary>
    public class TerrainInfo
    {
        #region Members

        private float terrainScale;
        private float[,] heights;
        private Vector3[,] normals;
        private Vector3 heightmapPosition;
        private float heightmapWidth;
        private float heightmapHeight;

        #endregion

        #region Accessors / Mutators

        #endregion

        public TerrainInfo(float[,] heights,
            Vector3[,] normals, float terrainScale)
        {
            if (heights == null)
                throw new ArgumentNullException("heights");
            if (normals == null)
                throw new ArgumentNullException("normals");

            this.terrainScale = terrainScale;
            this.heights = heights;
            this.normals = normals;

            heightmapWidth =
                (heights.GetLength(0) - 1) * terrainScale;
            heightmapHeight =
                (heights.GetLength(1) - 1) * terrainScale;

            heightmapPosition.X =
                -(heights.GetLength(0) - 1) / 2.0f * terrainScale;
            heightmapPosition.Z =
                -(heights.GetLength(1) - 1) / 2.0f * terrainScale;
        }


        public bool IsOnHeightmap(Vector3 position)
        {
            // first we'll figure out where on the heightmap "position" is...
            Vector3 positionOnHeightmap = position - heightmapPosition;

            // ... and then check to see if that value goes outside the bounds of the
            // heightmap.
            return (positionOnHeightmap.X > 0 &&
                positionOnHeightmap.X < heightmapWidth &&
                positionOnHeightmap.Z > 0 &&
                positionOnHeightmap.Z < heightmapHeight);
        }

        public void GetHeightAndNormal
            (Vector3 position, out float height, out Vector3 normal)
        {
            // the first thing we need to do is figure out where on the heightmap
            // "position" is. This'll make the math much simpler later.
            Vector3 positionOnHeightmap = position - heightmapPosition;

            // we'll use integer division to figure out where in the "heights" array
            // positionOnHeightmap is. Remember that integer division always rounds
            // down, so that the result of these divisions is the indices of the "upper
            // left" of the 4 corners of that cell.
            int left, top;
            left = (int)positionOnHeightmap.X / (int)terrainScale;
            top = (int)positionOnHeightmap.Z / (int)terrainScale;

            // next, we'll use modulus to find out how far away we are from the upper
            // left corner of the cell. Mod will give us a value from 0 to terrainScale,
            // which we then divide by terrainScale to normalize 0 to 1.
            float xNormalized = (positionOnHeightmap.X % terrainScale) / terrainScale;
            float zNormalized = (positionOnHeightmap.Z % terrainScale) / terrainScale;

            // Now that we've calculated the indices of the corners of our cell, and
            // where we are in that cell, we'll use bilinear interpolation to calculuate
            // our height. This process is best explained with a diagram, so please see
            // the accompanying doc for more information.
            // First, calculate the heights on the bottom and top edge of our cell by
            // interpolating from the left and right sides.
            float topHeight = MathHelper.Lerp(
                heights[left, top],
                heights[left + 1, top],
                xNormalized);

           float bottomHeight = MathHelper.Lerp(
                heights[left, top + 1],
                heights[left + 1, top + 1],
                xNormalized);

            // next, interpolate between those two values to calculate the height at our
            // position.
            height = MathHelper.Lerp(topHeight, bottomHeight, zNormalized);

            // We'll repeat the same process to calculate the normal.
            Vector3 topNormal = Vector3.Lerp(
                normals[left, top],
                normals[left + 1, top],
                xNormalized);

            Vector3 bottomNormal = Vector3.Lerp(
                normals[left, top + 1],
                normals[left + 1, top + 1],
                xNormalized);

            normal = Vector3.Lerp(topNormal, bottomNormal, zNormalized);
            normal.Normalize();
        }


        public float HeightmapHeight
        {
            get { return heightmapHeight; }
        }
        public float HeightmapWidth
        {
            get { return heightmapWidth; }
        }
        public float HeightmapScale
        {
            get { return terrainScale; }
        }
    }
    /// <summary>
    /// This class will load the HeightMapInfo when the game starts. This class needs 
    /// to match the HeightMapInfoWriter.
    /// </summary>
    public class TerrainInfoReader : ContentTypeReader<TerrainInfo>
    {
        protected override TerrainInfo Read(ContentReader input,
            TerrainInfo existingInstance)
        {
            float terrainScale = input.ReadSingle();
            int width = input.ReadInt32();
            int height = input.ReadInt32();
            float[,] heights = new float[width, height];
            Vector3[,] normals = new Vector3[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    heights[x, z] = input.ReadSingle();
                }
            }
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    normals[x, z] = input.ReadVector3();
                }
            }
            return new TerrainInfo(heights, normals, terrainScale);
        }
    }

    #endregion

    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Terrain : StaticModel
    {
        #region Constants

        #endregion
        //---------------------------------------------------------------------
        #region Members
        public TerrainInfo terrainInfo; //Contains heightmap detection
        public Texture2D textureR;
        public Texture2D textureG;
        public Texture2D textureB;
        public Texture2D textureMap;
        #endregion
        //---------------------------------------------------------------------
        #region Constructor

        public Terrain(GraphicsDevice graphicsDevice, 
                       ContentManager contentManager)
            : base(contentManager, "", Vector3.Zero, Matrix.Identity, 1f)
        {

        }

        #endregion
        //---------------------------------------------------------------------
        #region Load Unload

        public override void UnloadModel()
        {
            base.UnloadModel();
            terrainInfo = null;
        }

        public void LoadModel(string HeightMap, string TextureMap, 
                                       string TextureR, string TextureG, string TextureB)
        {
            //Set heightmap generators asset location
            modelAsset = HeightMap;
            //Set textures for terrain effect
            textureMap = base.content.Load<Texture2D>(TextureMap);
            textureR = base.content.Load<Texture2D>(TextureR);
            textureG = base.content.Load<Texture2D>(TextureG);
            textureB = base.content.Load<Texture2D>(TextureB);

            base.LoadModel();
            if (isLoaded)
            {
                terrainInfo = model.Tag as TerrainInfo;
                if (model.Tag == null)
                    throw new NullReferenceException
                        ("Heightmap Info Unable to Load");
            }
        }

        public void LoadShaders(string effectLocation)
        {
            Effect effect = content.Load<Effect>(effectLocation);
            foreach (ModelMesh mesh in model.Meshes)
            {
                //BasicEffect basicEffect = (mesh.Effects[0] as BasicEffect);
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    effect.Parameters["mTextureWeight"].SetValue(textureMap);
                    effect.Parameters["mTextureR"].SetValue(textureR);
                    effect.Parameters["mTextureG"].SetValue(textureG);
                    effect.Parameters["mTextureB"].SetValue(textureB);
                    part.Effect = effect;
                }
            }
        }

        #endregion
        //---------------------------------------------------------------------
        #region Draw / Update

        public void drawLevel(GameTime gameTime, Matrix view, Matrix projection)
        {
            if (isLoaded)
            {
                Matrix[] boneTransforms = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(boneTransforms);
                Matrix worldMatrix = orientation;

                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (Effect effect in mesh.Effects)
                    {
                            effect.CurrentTechnique = effect.Techniques["Technique1"];
                            effect.Parameters["mWorld"].SetValue(
                                           boneTransforms[mesh.ParentBone.Index] *
                                           worldMatrix *
                                           Matrix.CreateTranslation(location));
                            effect.Parameters["mView"].SetValue(view);
                            effect.Parameters["mProjection"].SetValue(projection);

                        }
                        mesh.Draw();
                    }
                
            }
        }

        #endregion
    }
}
