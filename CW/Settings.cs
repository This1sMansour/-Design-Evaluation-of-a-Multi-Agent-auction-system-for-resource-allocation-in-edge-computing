using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CW
{
    class Settings
    {
        //      AuctioneerAgent         Settings
        public static int RegistrationFee = 10;
        public static double CommissionPercentage = 2;

        //      DeviceAgent             Settings
        public static int NoDeviceAgents = 10;

        public static int MaxRequiredLotsForTask = 7;
        public static int MinRequiredLotsForTask = 1;

        public static int MaxMoneyPerLot = 100;
        public static int MinMoneyPerLot = 20;

        public static int MaxSurchargePerRound = 30;
        public static int MinSurchargePerRound = 1;

        public static int MaxTotalRound = 10;
        public static int MinTotalRound = 0;

        //      EdgeServerAgent         Settings
        public static int NoEdgeServerAgents = 7;

        public static int MaxNoLots = 5;
        public static int MinNoLots = 1;

        public static int MaxDiscountPerRound = 10;
        public static int MinDiscountPerRound = 1;

        public static int MaxBasePricePerLot = 100;
        public static int MinBasePricePerLot = 20;

        //public static int MaxTotalRound = 10;
        //public static int MinTotalRound = 0;

        public static int MaxMoney = 500;
        public static int MinMoney = 100;

        //      CentralisedCloudSystem  Settings
        public static int PricePerLot = 300;
    }
}
