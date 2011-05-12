using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//This class needs an array of scooters and the ability to add scooters to the players
//list of purchased scooters or buy upgrades if they have enough money.
namespace SSORF.Objects
{
     struct upgradeSpecs
        {
            public float power;
            public float weight;
        }

    class Player
    {
       

        private int money = 1000;

        //An array of the ID numbers for scooters owned
        private short[] scootersOwned = new short[10] {1,2,0,0,0,0,0,0,0,0};

        private bool[,] upgradesPurchased = new bool[10,3];

        //An array of stat changes corresponding to 
        private upgradeSpecs[] scooterUpgrades = new upgradeSpecs[10];

        //Id number of the selected scooter
        private short selectedScooter = 1;

        public short SelectedScooter
        {
            get { return selectedScooter; }
            set { selectedScooter = value; } 
        }

        public int Money
        {
            get { return money; }
        }

        private bool PurchaseItem(short cost)
        {
            if (cost > money)
                return false;
            else
                money -= cost;

            return true;
        }

        public string PurchaseUpgrade(SSORFlibrary.UpgradeData upgrade)
        {
            //if upgrade already purchased for that scooter return false
            if (upgradesPurchased[selectedScooter,upgrade.IDnum] == true)
                return "This upgrade has already been purchased";
            //if player has insufficent funds return false
            if (PurchaseItem(upgrade.cost) == false)
                return "You don't have enough cash for this upgrade";

            //otherwise add upgrade values to total upgrade specs for that scooter
            scooterUpgrades[selectedScooter].power += upgrade.power;
            scooterUpgrades[selectedScooter].weight += upgrade.weight;
            upgradesPurchased[selectedScooter, upgrade.IDnum] = true;
            return "Upgrade purchased";
        }
    }
}
