using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ActressMas;

namespace CW
{
    class DeviceAgent : Agent
    {
        private int NumberOfTimesToIncrease;

        private int MinValuationPerLot;
        private double SurchargePerRound;

        private int AuctionParticipated;

        private int TasksLeft;
        private int MoneyLeft;

        private int MoneyOnAuction;

        private bool Registered;
        
        public DeviceAgent(int tasks, int money, int rounds, int startValuation, double surchargePerRound)
        {
            MinValuationPerLot = startValuation;
            SurchargePerRound = surchargePerRound; //Percentage to increase per round

            TasksLeft = tasks;
            MoneyLeft = money;
            NumberOfTimesToIncrease = rounds;


            Registered = false;
            AuctionParticipated = -1; 
        }

        public override void Setup()
        {
            Console.WriteLine($"Type: DeviceAgent, Name: {Name}, TotalTasks: {TasksLeft}, " +
                $"TotalMoney: {MoneyLeft}, Number of Times to Increase bidding price: {NumberOfTimesToIncrease}\n");
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
            var tradeExecution = message.ContentObj as TradeExecution;
            switch (tradeExecution.Situation)
            {
                case 1: // an offer and a bid are matched, now the deviceAgent have to fill a transaction to pay the price for the wanted lot 
                    if (MoneyLeft >= 0)
                    {
                        Transaction transaction = new Transaction(tradeExecution);
                        MoneyLeft -= transaction.Amount;
                        Send(message.Sender, transaction);
                    }
                    else { EndOfAgent(); }
                    break;

                case 2: // The deal is finished, and the lot has been allocated 
                    TasksLeft -= 1;
                    Console.WriteLine($"{Name} - Tasks Left: {TasksLeft}");
                    break;

                case 0: // The deal failed, no lot has been allocated, money returns 
                    // MoneyLeft += tradeExecution.Price;
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
                        Transaction transaction = new Transaction(auctionNotification.RegistrationFee, Name, message.Sender, 1);
                        Send(message.Sender, transaction); // Send registration with the reason of bidder registration to the auctioneer
                        MoneyLeft -= auctionNotification.RegistrationFee; 
                    }
                    break;

                case 2:
                    if (Registered == true) 
                    { bidding(); }
                    break;
                case -2:
                    SentToCloud();
                    break;
            }
        }

        public void HandleTransaction(Message message) 
        {
            var transaction = message.ContentObj as Transaction;
            
            switch (transaction.Situation)
            {
                case 2: // transaction successful 
                    switch (transaction.Reason)
                    {
                        case 1: // bidder registration transaction
                            Registered = true;
                            break;
                            
                        case 2:
                            break;

                        case 3:
                            break;
                    }
                    break; 
                case 0: // transaction un-successful 
                    if (transaction.Reason == 1 || transaction.Reason == 3) MoneyLeft += transaction.Amount;
                    else PrintErrorMessage("Transaction which shouldn't be sent is sent to device agent.");
                    break;
            }

            switch (transaction.Reason) 
            {
                case 1: // bidder registration transaction
                    switch(transaction.Situation)
                    {
                        case 2:
                            Registered = true;
                            break;
                        case 0: // transaction failed
                            MoneyLeft += transaction.Amount;
                            break;
                    }
                    break;
                case 3: // It is for the case, if the transaction for the matched, but it's won't be called, because it will sent the tradeExecution unless it is failed, which the money will return 
                    Console.WriteLine("It won't be called");

                    break;

            }
        }

        public void bidding()
        {
            int price;

            MoneyOnAuction = 0;
            if (AuctionParticipated < NumberOfTimesToIncrease) AuctionParticipated++; //Increase as much that the agent thinks it is necessary 

            for (int i = 0; i < TasksLeft; i++)
            {
                price = (int)(((double)MinValuationPerLot) * (1 + (SurchargePerRound * AuctionParticipated)));
                if ((price + MoneyOnAuction) <= MoneyLeft)
                {
                    Bid bid = new Bid(Name, 10, MinValuationPerLot);
                    
                    MoneyOnAuction += price;
                    Send("auctioneer", bid);
                }
            }
        }

        public void SentToCloud()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"{Name}, Sending {TasksLeft} tasks to the cloud");
            Console.ResetColor();
            EndOfAgent();
        }

        public override void ActDefault()
        {
            if (TasksLeft == 0 || MoneyLeft <= 0) 
            {
                EndOfAgent();
            }
        }

        public static void PrintErrorMessage(string message) 
        { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine(message); Console.ResetColor(); }

        public void EndOfAgent() 
        { 
            Console.ForegroundColor = ConsoleColor.Magenta; 
            Console.WriteLine($"End of {Name}, Tasks Left: {TasksLeft}, Money: {MoneyLeft}"); 
            Console.ResetColor(); 
            Stop();
        }
    }
}
