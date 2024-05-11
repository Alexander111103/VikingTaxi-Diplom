using System;
using System.Collections.Generic;

namespace Taxi
{
    public class JsonOrder
    {
        public JsonOrder(string id_order, string date_order, string timeStart_order, string timeFinish_order, string distance_order, string duration_order, string durationInTraffic_order, string startShort_order, string finishShort_order, string startLong_order, string finishLong_order, string startCoorders_order, string finishCoorders_order, string status_order, string priority_order, string price_order, string rate_order, string paymentType_order, string user_order, string taxiDriver_order, string timeInSearch_order, string timeInWaitDriver_order, string timeInDrive_order, string rating_order) 
        {
            Id = Convert.ToInt32(id_order);
            Date = date_order;
            TimeStart = timeStart_order;
            TimeFinish = timeFinish_order;
            Distance = distance_order;
            Duration = duration_order;
            DurationInTraffic = durationInTraffic_order;
            StartShort = startShort_order;
            FinishShort = finishShort_order;
            StartLong = startLong_order;
            FinishLong = finishLong_order;
            StartCoorders = startCoorders_order;
            FinishCoorders = finishCoorders_order;
            Status = status_order;
            Priority = priority_order;
            Price = price_order;
            Rate = rate_order;
            PaymentType = paymentType_order;
            UserId = Convert.ToInt32(user_order);
            TaxiDriverId = Convert.ToInt32(taxiDriver_order);
            TimeInSearch = timeInSearch_order;
            TimeInWaitDriver = timeInWaitDriver_order;
            TimeInDrive = timeInDrive_order;
            Rating = Convert.ToInt32(rating_order);
        }

        public int Id { get; private set; }
        public string Date { get; private set; }
        public string TimeStart { get; private set; }
        public string TimeFinish { get; private set; }
        public string Distance { get; private set; }
        public string Duration { get; private set; }
        public string DurationInTraffic { get; private set; }
        public string StartShort { get; private set; }
        public string FinishShort { get; private set; }
        public string StartLong { get; private set; }
        public string FinishLong { get; private set; }
        public string StartCoorders { get; private set; }
        public string FinishCoorders { get; private set; }
        public string Status { get; private set; }
        public string Priority { get; private set; }
        public string Price { get; private set; }
        public string Rate { get; private set; }
        public string PaymentType { get; private set; }
        public int UserId { get; private set; }
        public int TaxiDriverId { get; private set; }
        public string TimeInSearch { get; private set; }
        public string TimeInWaitDriver { get; private set; }
        public string TimeInDrive { get; private set; }
        public int Rating { get; private set; }
    }

    public class JsonOrders
    {
        public JsonOrders(List<JsonOrder> orders)
        {
            Orders = orders;
        }

        public List<JsonOrder> Orders { get; private set; }
    }
}
