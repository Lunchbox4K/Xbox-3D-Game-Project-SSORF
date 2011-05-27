using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework;

namespace _3dOnlineGamePipeline
{
    public class TerrainInfoContent
    {
        #region Members

        public float[,] Height
        { get { return height; } }
        private float[,] height;

        public Vector3[,] Normals
        { get { return normals; } set { normals = value; } }
        private Vector3[,] normals;

        public float TerrainScale
        { get { return terrainScale; } }
        private float terrainScale;

        #endregion

        public TerrainInfoContent(MeshContent terrainMesh, float TerrainScale, 
            int terrainWidth, int terrainLength)
        {
            // validate input params
            if (terrainMesh == null)
                throw new ArgumentNullException("terrainMesh");
            if (terrainWidth <= 0)
                throw new ArgumentOutOfRangeException("terrainWidth");
            if (terrainLength <= 0)
                throw new ArgumentOutOfRangeException("terrainLength");

            terrainScale = TerrainScale;

            height = new float[terrainWidth, terrainLength];
            normals = new Vector3[terrainWidth, terrainLength];

            //Look at terrain mesh
            GeometryContent geometry = terrainMesh.Geometry[0];
            for (int i = 0; i < geometry.Vertices.VertexCount; i++)
            {
                // ... and look up its position and normal.
                Vector3 location = geometry.Vertices.Positions[i];
                Vector3 normal = (Vector3)geometry.Vertices.Channels
                    [VertexChannelNames.Normal()][i];

                // from the position's X and Z value, we can tell what X and Y
                // coordinate of the arrays to put the height and normal into.
                int arrayX = (int)
                    ((location.X / terrainScale) + (terrainWidth - 1) / 2.0f);
                int arrayY = (int)
                    ((location.Z / terrainScale) + (terrainLength - 1) / 2.0f);

                height[arrayX, arrayY] = location.Y;
                normals[arrayX, arrayY] = normal;
            }
        }
    }

    [ContentTypeWriter]
    public class TerrainInfoWriter : ContentTypeWriter<TerrainInfoContent>
    {
        protected override void Write(ContentWriter output, TerrainInfoContent value)
        {
            output.Write(value.TerrainScale);

            output.Write(value.Height.GetLength(0));
            output.Write(value.Height.GetLength(1));
            foreach (float height in value.Height)
            {
                output.Write(height);
            }
            foreach (Vector3 normal in value.Normals)
            {
                output.Write(normal);
            }
        }
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return "SSORF.Objects.TerrainInfo, " +
                "SSORFwindows, Version=1.0.0.0, Culture=neutral";
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "SSORF.Objects.TerrainInfoReader, " +
                "SSORFwindows, Version=1.0.0.0, Culture=neutral";
        }
    }
}
