using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//This class needs an array of scooters and the ability to add scooters to the players
//list of purchased scooters or buy upgrades if they have enough money.
namespace SSORF.Objects
{
    
    class Player
    {

        //private int money = 0;

        //An array of the ID numbers for scooters owned
        private short[] scootersOwned = new short[10] {0,0,0,0,0,0,0,0,0,0};
        //Id number of the selected scooter
        private short selectedScooter = 1;

        public short SelectedScooter
        {
            get { return selectedScooter; }
            set { selectedScooter = value; } 
        }

    }
}
