using ActressMas;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CW
{
    class AuctioneerAgent : Agent
    {
        private static int RegistrationFee;
        private static double CommissionPercentage;
        private static int AuctionSituation; // -1. Start system, -2. No more auction
        // 1. Start registration 2. Start bidding, and offering 3. End of bidding and offering, start of auction 0. End of auction

        private static List<Offer> _offers;
        private static List<Bid> _bids;
        private static List<TradeExecution> _tradeExecutionPending;
        private static List<TradeExecution> _tradeExecutionFinished;

        private int _turnsToWait;

        private List<String> BiddersRegistered;
        private List<String> OfferersRegistered;

        
        private int MoneyMade;

        public AuctioneerAgent(int registrationFee, double commissionPercentage)
        {
            RegistrationFee = registrationFee;
            CommissionPercentage = commissionPercentage;

            _bids = new List<Bid>();
            _offers = new List<Offer>();

            _tradeExecutionPending = new List<TradeExecution>();
            _tradeExecutionFinished = new List<TradeExecution>();

            BiddersRegistered = new List<String>();
            OfferersRegistered = new List<String>();

            MoneyMade = 0;
        }
        

        public override void Setup()
        {
            Console.WriteLine($"AuctioneerAgent: {Name}, Registration Fee: {RegistrationFee}, Commission Percentage: {CommissionPercentage}\n");
            AuctionSituation = -1;
            _turnsToWait = 5;
        }

        public override void Act(Message message)
        {
            try
            {
                object contentObj = message.ContentObj;
                Console.WriteLine($"\t[{message.Sender}] -> [{message.Receiver}]: {contentObj}");

                if (contentObj is Transaction)
                {
                    HandleTransaction(contentObj);
                }

                else if (contentObj is Bid && AuctionSituation == 3)
                {
                    var bid = contentObj as Bid;
                    if (BiddersRegistered.Contains(bid.Bidder)) // check if the bidder is registered
                    { 
                        if (bid != null)
                        {
                            bid.Id = _bids.Count;
                            _bids.Add(bid);
                            Console.WriteLine($"[{message.Sender} -> {message.Receiver}]: {bid}");
                        }
                    }
                }
                else if(contentObj is Offer && AuctionSituation == 3)
                {
                    var offer = contentObj as Offer;
                    if (OfferersRegistered.Contains(offer.Offerer))
                    {
                        if (offer != null)
                        {
                            offer.Id = _offers.Count;
                            _offers.Add(offer);
                            Console.WriteLine($"[{message.Sender} -> {message.Receiver}]: {offer}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void HandleTransaction(Object contentObj) 
        {
            var transaction = contentObj as Transaction;
            switch (transaction.Reason)
            {
                case 1:     // Bidder Registration 
                    if (AuctionSituation == 2 && transaction.Receiver == Name && transaction.Amount - RegistrationFee == 0 && !BiddersRegistered.Contains(transaction.Sender))
                    { // Situation should be pending, the receiver should be the auctioneer, amount should be equal to registration fee, and the bidder shouldn't be registered 
                        BiddersRegistered.Add(transaction.Sender);
                        transaction.Situation = 2;
                        Send(transaction.Sender, transaction);
                    }
                    else 
                    {
                        transaction.Situation = 0;
                        Send(transaction.Sender, transaction);
                    }
                    break;

                case 2:     // Offerer Registration 
                    if (AuctionSituation == 2 && transaction.Receiver == Name && transaction.Amount - RegistrationFee == 0 && !BiddersRegistered.Contains(transaction.Sender))
                    { // Situation should be pending, the receiver should be the auctioneer, amount should be equal to registration fee, and the offerer shouldn't be registered 
                        OfferersRegistered.Add(transaction.Sender);
                        transaction.Situation = 2;
                        Send(transaction.Sender, transaction);
                    }
                    else {
                        
                        transaction.Situation = 0;
                        Send(transaction.Sender, transaction);
                    }
                    break;

                case 3:     // Payment for Trade execution
                    // find the matchedTransaction in the list
                    TradeExecution tradeExecution = _tradeExecutionPending.FirstOrDefault(matchedTransactionSearch => 
                        matchedTransactionSearch.TradeId == transaction.TradeExecutionId);

                    if (tradeExecution.Price == transaction.Amount)
                    { // The Amount is right 
                        _tradeExecutionPending.Remove(tradeExecution);
                        tradeExecution.Situation = 2;
                        transaction.Situation = 2;

                        Send(transaction.Sender, transaction);

                        // commission percentage
                        int CommissionAmount = (int)( (double)transaction.Amount * (CommissionPercentage / 100) );
                        if (CommissionAmount == 0)
                        {
                            tradeExecution.Price -= 1;
                            transaction.Amount -= 1;
                            MoneyMade += 1;
                        }
                        else 
                        { 
                            transaction.Amount -= CommissionAmount;
                            tradeExecution.Price -= 1;
                            MoneyMade += CommissionAmount;
                        }

                        _tradeExecutionFinished.Add(tradeExecution);
                        Send(transaction.Receiver, tradeExecution);
                        Send(transaction.Sender, tradeExecution);
                    }
                    else 
                    { // It's not the correct amount
                        tradeExecution.Situation = 0;
                        Send(transaction.Sender, tradeExecution);
                    }
                    break;
            }
        }


        public override void ActDefault()
        {
            if (--_turnsToWait <= 0)
            {
                switch (AuctionSituation)
                {
                    case -2:
                        // No auction, but because there might be some transactions, the auction still be running
                        _turnsToWait += 10000;
                        break;

                    case -1:
                        // Waiting for period when the system starts for the first time, so all of the agents are up
                        AuctionSituation++;
                        break;

                    case 0:
                        // Period between the end of the previous auction, and the start of the next auction
                        AuctionSituation++;
                        break;

                    case 1: 
                        // Start of Registration 
                        Broadcast(new AuctionNotification(1, RegistrationFee, CommissionPercentage));
                        AuctionSituation++;
                        break;

                    case 2: // Start of bidding period
                        Console.WriteLine();
                        Broadcast(new AuctionNotification(2, RegistrationFee, CommissionPercentage));
                        AuctionSituation++;
                        break;

                    case 3: // End of bidding period, Start of clearing period
                        Console.WriteLine();
                        Broadcast(new AuctionNotification(3, RegistrationFee, CommissionPercentage));
                        ClearingPeriod();
                        if (AuctionSituation != -2) AuctionSituation = 0;
                        break;
                    default:
                        break;
                }
                _turnsToWait = 5;
            }
        }
        public void ClearingPeriod()
        {
            Console.WriteLine("Start of clearing period:");
            Console.WriteLine("\n\tBids Received:");
            foreach (Bid bid in _bids) Console.WriteLine(bid);
            Console.WriteLine("\n\tOffers Received:");
            foreach (Offer offer in _offers) Console.WriteLine(offer);
            if (!_offers.Any() || !_bids.Any())
            {   // there is no bids or offers
                Console.WriteLine("No more auction."); AuctionSituation = -2;
                Broadcast(new AuctionNotification(-2, RegistrationFee, CommissionPercentage));
                _bids.Clear();
                _offers.Clear();
            }
            else
            {
                int minOfferValue = _offers.Min(offer => offer.OfferValue);
                int maxBidValue = _bids.Max(bid => bid.BidValue);

                Console.WriteLine($"\nMax bid value: {maxBidValue}, Min offer value: {minOfferValue}\n");

                if (maxBidValue * 10 <= minOfferValue)
                {
                    Console.WriteLine("No more auction"); AuctionSituation = -2;
                    Broadcast(new AuctionNotification(-2, RegistrationFee, CommissionPercentage));
                }
                else
                {
                    List<TradeExecution> matchedTransactions = MatchBiddersOffers();
                    foreach (TradeExecution matchedTransaction in matchedTransactions) Console.WriteLine(matchedTransaction);
                    _bids.Clear();
                    _offers.Clear();
                }
            }
        }
        public List<TradeExecution> MatchBiddersOffers()
        {
            // Calculate the clearing price
            int clearingPrice = CalculateClearingPrice();
            Console.WriteLine($"Clearing Price: {clearingPrice}");

            // Initialize the list of matched transactions
            List<TradeExecution> tradeExecution = new List<TradeExecution>();

            // If a clearing price was found
            if (clearingPrice != -1)
            {
                // Iterate over the bids and offers
                for (int i = 0; i < Math.Min(_bids.Count, _offers.Count); i++)
                {
                    // If the bid is greater than or equal to the offer
                    if (_bids[i].BidValue >= _offers[i].OfferValue)
                    {
                        // Create a new matched transaction
                        TradeExecution transaction = new TradeExecution(_tradeExecutionPending.Count, _bids[i], _offers[i], clearingPrice);

                        // Send message regarding the informaiton about the matched transaction 
                        //SendToMany(List<string>(transaction.Bidder, transaction.Offerer), transaction);
                        //Send(transaction.Bidder, transaction);
                        Send(transaction.Bidder, transaction);
                        Send(transaction.Offerer, transaction);

                        // Add the matched transaction to the list
                        tradeExecution.Add(transaction);
                        _tradeExecutionPending.Add(transaction);

                        // Remove the bid and offer from the lists
                        _bids.RemoveAt(i);
                        _offers.RemoveAt(i);

                        i--; // Decrement the index to account for the removed items
                    }
                    else
                    {
                        break; // If the bid is less than the offer, break the loop
                    }
                }
            }
            else
            {
                Console.WriteLine("No more auction"); AuctionSituation = -2;
                Broadcast(new AuctionNotification(-2, RegistrationFee, CommissionPercentage));
            }
            return tradeExecution;
        }
        public static int CalculateClearingPrice()
        {
            // Sort the bids in descending order
            _bids = BidUtility.GetReverseSortedBids(_bids);

            // Sort the offers in ascending order
            _offers.Sort((a, b) => a.OfferValue.CompareTo(b.OfferValue));

            // Initialize the clearing price
            double? clearingPrice = null;

            // Iterate over the bids and offers
            for (int i = 0; i < Math.Min(_bids.Count, _offers.Count); i++)
            {
                // If the bid is greater than or equal to the offer
                if (_bids[i].BidValue >= _offers[i].OfferValue)
                {
                    // Set the clearing price as the average of the bid and offer
                    clearingPrice = (_bids[i].BidValue + _offers[i].OfferValue) / 2.0;
                }
                else
                {
                    // If the bid is less than the offer, break the loop
                    break;
                }
            }

            return clearingPrice.HasValue ? (int)(clearingPrice) : -1;
        }
    }
}
