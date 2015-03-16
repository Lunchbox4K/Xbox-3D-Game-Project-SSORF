using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.ComponentModel;


namespace _3dOnlineGamePipeline
{
    /// <summary>
    /// This XNA Content Pipeline processes data from a greyscale bitmap
    /// image and processes into a 3D terrain model with collision support.
    /// 
    /// Issues: This creates a single model, large scale workes fine with resonable resolutions.
    /// </summary>
    [ContentProcessor]
    public class TerrainProcessor : ContentProcessor<Texture2DContent, ModelContent>
    {
        #region Properties
        //================================================================================================
        // General properties for terrain generation.
        //================================================================================================
        public float Scale { get { return scale; } set { scale = value; } }
        private float scale = 16f; //Distance between verticies in finished terrain mesh

        public float Height { get { return height; } set { height = value; } }
        private float height = 64f; //Max Height

        public float TexCoordScale { get { return textScl; } set { textScl = value; } }
        private float textScl = 0.01f; //How often the texture will be repeated.Z

        public string TextureLocation { get { return textLoc; } set { textLoc = value; } }
        private string textLoc = ""; //Texture Location
        #endregion

        #region Process
        public override ModelContent Process(Texture2DContent input, ContentProcessorContext context)
        {
            //# Load the height map into a processable float array

            PixelBitmapContent<float> bmpHaightMap;
            input.ConvertBitmapType(typeof(PixelBitmapContent<float>)); //Convert to float readable
            bmpHaightMap = (PixelBitmapContent<float>)input.Mipmaps[0];

            //# Start building the mesh

            MeshBuilder builder = MeshBuilder.StartMesh("terrain");

            //Create Terrain Verticies
            for (int i = 0; i < bmpHaightMap.Height; i++)
                for (int j = 0; j < bmpHaightMap.Width; j++)
                {
                    Vector3 location;
                    //build with center on 0,0 (X,Z)
                    location.X = scale * (j - ((bmpHaightMap.Width - 1) / 2.0f));
                    location.Z = scale * (i - ((bmpHaightMap.Height - 1) / 2.0f));
                    location.Y = (bmpHaightMap.GetPixel(j, i) - 1) * height;
                    //Save location
                    builder.CreatePosition(location);
                }

            //Create Material and point it the Terrain Texture.
            BasicMaterialContent material = new BasicMaterialContent();
            material.SpecularColor = new Vector3(.4f, .4f, .4f);
            //if(!string.IsNullOrEmpty(textLoc))
            //{
            //    string directory = Path.GetDirectoryName(input.Identity.SourceFilename);
            //    string texture = Path.Combine(directory, textLoc);

            //    material.Texture = new ExternalReference<TextureContent>(texture);
            //}
            builder.SetMaterial(material);


            //Create a vertex channel for holding texture coords
            int texCoordId = builder.CreateVertexChannel<Vector2>(
                VertexChannelNames.TextureCoordinate(0));
            // Create the individual triangles that make up our terrain.
            for (int i = 0; i < bmpHaightMap.Height - 1; i++)
                for (int j = 0; j < bmpHaightMap.Width - 1; j++)
                {
                    addVertex(builder, texCoordId, bmpHaightMap.Width, j, i);
                    addVertex(builder, texCoordId, bmpHaightMap.Width, j + 1, i);
                    addVertex(builder, texCoordId, bmpHaightMap.Width, j + 1, i + 1);

                    addVertex(builder, texCoordId, bmpHaightMap.Width, j, i);
                    addVertex(builder, texCoordId, bmpHaightMap.Width, j + 1, i + 1);
                    addVertex(builder, texCoordId, bmpHaightMap.Width, j, i + 1);
                }
            MeshContent terrainMesh = builder.FinishMesh();

            //Convert to model and return.
            ModelContent model = context.Convert<MeshContent, ModelContent>(terrainMesh,
                                                              "ModelProcessor");
            model.Tag = new TerrainInfoContent(terrainMesh, scale,
                bmpHaightMap.Width, bmpHaightMap.Height);

            return model;
        }
        #endregion

        #region Helper Functions
        
        /// <summary>
        /// Adds a single vertex. Squared texture so width accounts for length.
        /// </summary>
        /// <param name="builder">Builder to add vertex to.</param>
        /// <param name="texCoordId">Vertex index for texture coordinate</param>
        /// <param name="w">Heightmap width</param>
        /// <param name="j">Width index</param>
        /// <param name="i">Height index</param>
        private void addVertex(MeshBuilder builder, int texCoordId, int w, int j, int i)
        {
            builder.SetVertexChannelData(texCoordId, new Vector2(j, i) * TexCoordScale);
            builder.AddTriangleVertex(j + i * w);
        }

        #endregion
    }
}