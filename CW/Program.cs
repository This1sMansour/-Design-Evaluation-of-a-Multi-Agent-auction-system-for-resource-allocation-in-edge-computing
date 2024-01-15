using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ActressMas;

namespace CW
{
    class Program
    {
        static void Main(string[] args)
        {
            var env = new EnvironmentMas();

            var rand = new Random();

            for (int i = 1; i <= Settings.NoDeviceAgents; i++)
            {
                double RequiredLotsForTasks = Settings.MinRequiredLotsForTask + rand.Next(Settings.MaxRequiredLotsForTask - Settings.MinRequiredLotsForTask);

                double MoneyPerSlot = Settings.MinMoneyPerLot + rand.Next(Settings.MaxMoneyPerLot - Settings.MinMoneyPerLot);
                double SurchargePerRound = Settings.MinSurchargePerRound + rand.Next(Settings.MaxSurchargePerRound - Settings.MinSurchargePerRound);
                SurchargePerRound = SurchargePerRound/ 100;
                double TotalRoundBid = Settings.MinTotalRound + rand.Next(Settings.MaxTotalRound - Settings.MinTotalRound);

                double TotalMoney = RequiredLotsForTasks * (MoneyPerSlot * (1+SurchargePerRound)) * (TotalRoundBid + 1);
                
                var deviceAgent = new DeviceAgent((int)RequiredLotsForTasks, (int)TotalMoney, (int)TotalRoundBid, (int)MoneyPerSlot, SurchargePerRound);
                env.Add(deviceAgent, $"DeviceAgent-No.{i}");
            }

            for (int i = 1; i <= Settings.NoEdgeServerAgents; i++)
            {
                int LotToSell = Settings.MinNoLots + rand.Next(Settings.MaxNoLots - Settings.MinNoLots);
                int BasePricePerLot = Settings.MinBasePricePerLot + rand.Next(Settings.MaxBasePricePerLot - Settings.MinBasePricePerLot);
                double DiscountPerRound = Settings.MinDiscountPerRound + rand.Next(Settings.MaxDiscountPerRound - Settings.MinDiscountPerRound);
                DiscountPerRound = DiscountPerRound / 100;

                int Money = Settings.MinMoney + rand.Next(Settings.MaxMoney - Settings.MinMoney);
                int TotalRound = Settings.MinTotalRound + rand.Next(Settings.MaxTotalRound - Settings.MinTotalRound);

                var edgeServerAgent = new EdgeServerAgent(LotToSell, BasePricePerLot, Money, DiscountPerRound, TotalRound);
                env.Add(edgeServerAgent, $"EdgeServerAgent-No.{i}");
            }

            var auctioneerAgent = new AuctioneerAgent(Settings.RegistrationFee, Settings.CommissionPercentage);
            env.Add(auctioneerAgent, "auctioneer");

            env.Start();
            
            Console.ReadLine();

        }
    }
}
