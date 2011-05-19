using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using SSORFlibrary;
namespace LocationMapGenerator
{ 
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// TODO: change the ContentProcessor attribute to specify the correct
    /// display name for this processor.
    /// </summary>
    [ContentProcessor(DisplayName = "LocationMapGenerator.ContentProcessor1")]
    public class LocationMapProcessor : ContentProcessor<Texture2DContent, SSORFlibrary.LocationMap>
    {

        public float Scale { get { return scale; } set { scale = value; } }
        private float scale = 8f;
        public int ReadScale { get { return readScale; } set { readScale = value; } }
        private int readScale = 1;

        public override LocationMap Process(Texture2DContent input, ContentProcessorContext context)
        {
            //Create Location Map and assign the scale.
            LocationMap returnMap = new LocationMap();
            returnMap.scale = scale;       
 
            //Load the height map into a readable bitmap
            PixelBitmapContent<Color> bmpMap;
            input.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
            bmpMap = (PixelBitmapContent<Color>)input.Mipmaps[0];

            //Create Tmp Location Map
            List<List<Vector3>> tmpMap = new List<List<Vector3>>();
            for (int i = 0; i < bmpMap.Height; i += readScale)
            {
                tmpMap.Add(new List<Vector3>());
                for (int j = 0; j < bmpMap.Width; j += readScale)
                {
                    Vector3 colorData;
                    Color color;
                    color = bmpMap.GetPixel(j, i);
                    //At most we will only need 3 floats
                    colorData.X = color.R; //Model ID
                    colorData.Y = color.G; //Model Yaw ??
                    colorData.Z = color.B; //Model Scale??
                    tmpMap[i].Add(colorData);
                }
            }
            returnMap.Color = new Vector3[tmpMap.Count][];
            for (int i = 0; i < tmpMap.Count; i++)
            {
                returnMap.Color[i] = tmpMap[i].ToArray();
            }

            return returnMap;
        }
    }
}