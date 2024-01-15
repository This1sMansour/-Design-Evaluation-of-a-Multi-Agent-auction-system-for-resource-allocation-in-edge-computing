using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using ActressMas;

namespace CW
{
    class EdgeServerAgent : Agent
    {
        private int LotsAvailable;
        private int MaxLotValue;
        private int MoneyLeft;
        private int TotalRoundToDecrease;
        private int MoneyMade;

        private double DiscountPerRound;
        private int AuctionParticipated;

        private bool Registered;

        public EdgeServerAgent(int lotsAvailable, int maxLotValue, int money, double discountPerRound, int totalRound)
        {
            LotsAvailable = lotsAvailable;
            MaxLotValue = maxLotValue;
            MoneyLeft = money;
            DiscountPerRound = discountPerRound;
            TotalRoundToDecrease = totalRound;

            Registered = false;
            AuctionParticipated = -1;
            MoneyMade = 0;
        }

        public override void Setup()
        {
            Console.WriteLine($"Type: EdgeServerAgent, Name: {Name}, Lots Available: {LotsAvailable}, Max Lot Value: {MaxLotValue}" +
                $"Money: {MoneyLeft}, Discount per round: {DiscountPerRound}, Total round to decrease: {TotalRoundToDecrease}\n");
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t[{message.Sender} -> {Name}]: {message.ContentObj}");
                if (message.ContentObj is TradeExecution)
                {
                    HandleTradeExecution(message);
                }
                else if (message.ContentObj is AuctionNotification)
                {
                    HandleAuctionNotification(message);
                }
                else if (message.ContentObj is Transaction)
                {
                    HandleTransaction(message);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void HandleTradeExecution(Message message) 
        {
            var tradeExecution =  message.ContentObj as TradeExecution;
            switch (tradeExecution.Situation) 
            {
                case 0: // TradeExecution failed
                    break;
                case 1: // a match between an offer and bidder taken place
                    break;
                case 2: // transaction successful
                    LotsAvailable -= 1;
                    MoneyMade += tradeExecution.Price;
                    MoneyLeft += tradeExecution.Price;
                    break;
            }
        }

        public void HandleAuctionNotification(Message message)
        {
            var auctionNotification = message.ContentObj as AuctionNotification;

            switch (auctionNotification.Situation)
            {
                case 1:
                    if (Registered == false) // DeviceAgent has not been registered 
                    {
                        Transaction transaction = new Transaction(auctionNotification.RegistrationFee, Name, message.Sender, 2);
                        Send(message.Sender, transaction); // Send registration with the reason of bidder registration to the auctioneer
                        MoneyLeft -= auctionNotification.RegistrationFee;
                    }
                    break;

                case 2:
                    if (Registered == true)
                    { Offering(); }
                    break;
                case -2:
                    Console.WriteLine($"End of {Name}");
                    break;
            }
        }

        public void HandleTransaction(Message message)
        {
            var transaction = message.ContentObj as Transaction;
            switch (transaction.Reason)
            {
                case 2:
                    switch (transaction.Situation)
                    {
                        case 2:
                            Registered = true;
                            break;
                        case 0:
                            MoneyLeft += transaction.Amount;
                            break;
                    }
                    break;
                case 3:
                    break;
            }
        }

        public void Offering()
        {
            if (LotsAvailable == 0) { EndOfAgent(); }
            if (AuctionParticipated < TotalRoundToDecrease) AuctionParticipated++;
            int price;

            for (int i = 0; i < LotsAvailable; i++)
            {
                price = (int)(((double)MaxLotValue) - (TotalRoundToDecrease * AuctionParticipated) * MaxLotValue);
                
                Offer offer = new Offer(Name, 10, price);
                Send("auctioneer", offer);
            }
        }

        public void EndOfAgent()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"End of {Name}, Lots Left: {LotsAvailable}, Money: {MoneyLeft}");
            Console.ResetColor();
            Stop();
        }
    }
}
