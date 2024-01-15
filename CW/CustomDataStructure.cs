using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CW
{
    class CustomDataStructure
    {

    }

    public class Transaction
    {
        public int Amount { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public int Reason { get; set; } // 1. Bidder Registration 2. Offerer Registration 3. Matched Transaction  
        public int Situation { get; set; } // 0. Failed, 1. Pending, 2. Successful

        public int TradeExecutionId;

        private string[] transactionReasons = new string[]
        {
            "Auction Registration Bidder",
            "Auction Registration Offerer",
            "Matched Transaction",
        };

        private string[] transactionSituation = new string[]
        {
            "Failed", "Pending", "Successful"
        };

        public Transaction(int amount, string sender, string receiver, int reason)
        { // transaction made by bidder or offerer for paying the registration fee
            Amount = amount;
            Sender = sender;
            Receiver = receiver;
            Reason = reason;
            Situation = 1;
            TradeExecutionId = -1;
        }
        public Transaction(int amount, string sender, string receiver, int reason, int situation)
        {
            Amount = amount;
            Sender = sender;
            Receiver = receiver;
            Reason = reason;
            Situation = situation;
            TradeExecutionId = -1;
        }

        public Transaction(int amount, string sender, string receiver, int reason, int situation, int matchedTransactionId)
        {
            Amount = amount;
            Sender = sender;
            Receiver = receiver;
            Reason = reason;
            Situation = situation;
            TradeExecutionId = matchedTransactionId;
        }

        public Transaction(TradeExecution tradeExecution)
        {
            TradeExecutionId = tradeExecution.TradeId;
            Sender = tradeExecution.Bidder;
            Receiver = tradeExecution.Offerer;
            Amount = tradeExecution.Price;
            Reason = 3;
            Situation= 1;
        }
        public override string ToString()
        {
            if (Reason == 3) return $"Amount: {Amount}, Sender: {Sender}, Receiver: {Receiver}, " +
                    $"Reason: {transactionReasons[Reason - 1]}, Trade Execution Id: {TradeExecutionId} Situation: {transactionSituation[Situation]}";

            return $"Amount: {Amount}, Sender: {Sender}, Receiver: {Receiver}, Reason: {transactionReasons[Reason-1]}, Situation: {transactionSituation[Situation]}";
        }
    }

    public class AuctionNotification
    {
        public int Situation { get; set; } // -1. Start system, -2. No more auction
        // 1. Start registration 2. Start bidding, and offering 3. End of bidding and offering, start of auction 0. End of auction
        public int RegistrationFee { get; set; }
        public double CommissionPercentage { get; set; }

        private string[] auctionNotificationSituation = new string[]
        {
            "No more auction", // -2
            "Start System", // -1
            "End of auction", // 0
            "Start registration", // 1
            "Start of bidding phase", // 2
            "End of bidding period, Start of Clearing phase" // 3
        };
        public AuctionNotification(int situation, int registrationFee, double commissionPercentage)
        {
            Situation = situation;
            RegistrationFee = registrationFee;
            CommissionPercentage = commissionPercentage;
        }

        public override string ToString()
        {
            return $"Situation: {auctionNotificationSituation[Situation + 2]}, Registration Fee: {RegistrationFee}, Commission Percentage: {CommissionPercentage}";
        }
    }

    public class Offer
    {
        public int Id { get; set; }
        public string Offerer { get; set; }
        public int Lot { get; set; }
        public int OfferValue { get; set; }

        public Offer(string offerer, int lot, int offerValue)
        {
            //Id = id; id is set up by the AuctioneerAgent
            Offerer = offerer;
            Lot = lot;
            OfferValue = offerValue;
        }

        public Offer(int id, string offerer, int lot, int offerValue)
        {
            Id = id;
            Offerer = offerer;
            Lot = lot;
            OfferValue = offerValue;
        }

        public void PrintOfferDetails()
        {
            Console.WriteLine($"Offer ID: {Id}");
            Console.WriteLine($"Offerer: {Offerer}");
            Console.WriteLine($"Lot: {Lot}");
            Console.WriteLine($"Offer Value: {OfferValue}");
        }

        public override string ToString()
        {
            return $"Offer: ID={Id}, Bidder={Offerer}, Lot={Lot}, BidValue={OfferValue}";
        }
    }

    public class Bid
    {
        public int Id { get; set; }
        public string Bidder { get; set; }
        public int Lot { get; set; }
        public int BidValue { get; set; }

        public Bid(string bidder, int lot, int bidValue) // bid done by bidder
        {
            //Id = id; id is set up by the AuctioneerAgent
            Bidder = bidder;
            Lot = lot;
            BidValue = bidValue;
        }

        public Bid(int id, string bidder, int lot, int bidValue) // bid saved in the auction system
        {
            Id = id;
            Bidder = bidder;
            Lot = lot;
            BidValue = bidValue;
        }

        public void PrintBidDetails()
        {
            Console.WriteLine($"Bid ID: {Id}");
            Console.WriteLine($"Bidder: {Bidder}");
            Console.WriteLine($"Lot: {Lot}");
            Console.WriteLine($"Bid Value: {BidValue}");
        }
        public override string ToString()
        {
            return $"Bid: ID={Id}, Bidder={Bidder}, Lot={Lot}, BidValue={BidValue}";
        }
    } 


    public class TradeExecution
    {
        public int TradeId { get; set; }
        public int BidId { get; set; }
        public int OfferId { get; set; }
        public string Bidder { get; set; }
        public string Offerer { get; set; }
        public int Lot { get; set; }
        public int Price { get; set; }
        public int Situation { get; set; } // 1. pending(for transaction) 2. Finished 0. Failed 
        
        private string[] tradeExecutionSituation = new string[] 
        {
            "Failed, check is done, and it results are false",
            "Pending for transaction",
            "Finished",
            "Transaction done, allocation taken place, send for checking",
            "Finished, , check is done, and it results are true"
        };
        public TradeExecution(int transactionId, Bid bid, Offer offer, int price)
        {
            TradeId = transactionId;
            Lot = bid.Lot;
            Price = price;
            Situation = 1;

            BidId = bid.Id;
            Bidder = bid.Bidder;

            OfferId = offer.Id;
            Offerer = offer.Offerer;
            
        }
        public override string ToString()
        {
            return $"MatchedTransaction: ID={TradeId}, Lot={Lot}, Price={Price}, Situation={tradeExecutionSituation[Situation]}\n" +
                $"\tBidder={Bidder}, BidId={BidId}\n" +
                $"\tOfferer={Offerer}, OfferId={OfferId}\n";
        }
    }


    public class BidUtility
    {
        public static Bid GetMaxBid(List<Bid> bids)
        {
            if (bids == null || bids.Count == 0)
            {
                return null; 
            }

            Bid maxBid = bids[0];

            foreach (var bid in bids)
            {
                if (bid.BidValue > maxBid.BidValue)
                {
                    maxBid = bid;
                }
            }

            return maxBid;
        }

        public static List<Bid> GetReverseSortedBids(List<Bid> bids)
        {
            if (bids == null || bids.Count == 0)
            {
                return new List<Bid>();
            }

            return bids.OrderByDescending(b => b.BidValue).ToList();
        }
    }
}
