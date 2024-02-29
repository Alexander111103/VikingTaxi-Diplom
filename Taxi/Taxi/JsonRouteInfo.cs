using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi
{
    public class JsonRouteInfo
    {
        public JsonRouteInfo(string distance, string duration, string durationInTraffic, string startShort, string finishShort, string startLong, string finishLong, string startCoorders, string finishCoorders) 
        { 
            Distance = distance.Replace(" ", " ");
            Duration = duration.Replace(" ", " ");
            DurationInTraffic = durationInTraffic.Replace(" ", " ");
            StartShort = startShort.Replace(" ", " ");
            FinishShort = finishShort.Replace(" ", " ");
            StartLong = startLong.Replace(" ", " ");
            FinishLong = finishLong.Replace(" ", " ");
            StartCoorders = startCoorders.Replace(" ", " ");
            FinishCoorders = finishCoorders.Replace(" ", " ");
        }

        public string Distance { get; private set; }
        public string Duration { get; private set; }
        public string DurationInTraffic { get; private set; }
        public string StartShort { get; private set; }
        public string FinishShort { get; private set; }
        public string StartLong { get; private set; }
        public string FinishLong { get; private set; }
        public string StartCoorders { get; private set; }
        public string FinishCoorders { get; private set; }
    }
}
