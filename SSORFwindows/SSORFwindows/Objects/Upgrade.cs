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

//needs a way to add the upgrades to vehicle specs
namespace SSORF.Objects
{
    //different types of upgrades (probably need more types)
    public enum UpgradeType
    { Power, Weight }

    class Upgrade
    {
        //what is the upgrade called?
        string name;
        private UpgradeType type;
        //The amount of weight or power, depending on the type of upgrade
        private short value;

    }



}
