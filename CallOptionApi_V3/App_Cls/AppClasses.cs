using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CallOptionApi_V3.App_Cls
{
    public class AppClasses
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class TradeDayOption
        {
            public string LVal18AFC { get; set; }
            public double PriceFirst { get; set; }
            public int DEven { get; set; }
            public long InsCode { get; set; }
            public string LVal30 { get; set; }
            public double PClosing { get; set; }
            public double PDrCotVal { get; set; }
            public double ZTotTran { get; set; }
            public double QTotTran5J { get; set; }
            public double QTotCap { get; set; }
            public double PriceChange { get; set; }
            public double PriceMin { get; set; }
            public double PriceMax { get; set; }
            public double PriceYesterday { get; set; }
            public int Last { get; set; }
            public int HEven { get; set; }
            public int DayCount { get; set; }
            public int PMeOf { get; set; }
            public int PMeDem { get; set; }
            public string InstrumentID { get; set; }
            public int BeginDate { get; set; }
            public int EndDate { get; set; }
            public double StrikePrice { get; set; }
            public double UAInsCode { get; set; }
            public int BuyOP { get; set; }
            public bool Flag { get; set; }
            public object CSocCSAC { get; set; }
            public object LSoc30 { get; set; }
            public object CIsin { get; set; }
            public object LVal18 { get; set; }
            public object NInsCode { get; set; }
            public object HeadPDrCotVal { get; set; }
            public object HeadPriceChange { get; set; }
            public object BlackScholes { get; set; }
        }

        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
        public class TradeLastDayOptionData
        {
            public string LVal18AFC { get; set; }
            public double PriceFirst { get; set; }
            public int DEven { get; set; }
            public long InsCode { get; set; }
            public string LVal30 { get; set; }
            public double PClosing { get; set; }
            public double PDrCotVal { get; set; }
            public double ZTotTran { get; set; }
            public double QTotTran5J { get; set; }
            public double QTotCap { get; set; }
            public double PriceChange { get; set; }
            public double PriceMin { get; set; }
            public double PriceMax { get; set; }
            public double PriceYesterday { get; set; }
            public long Last { get; set; }
            public int HEven { get; set; }
            public int DayCount { get; set; }
            public int PMeOf { get; set; }
            public int PMeDem { get; set; }
            public string InstrumentID { get; set; }
            public int BeginDate { get; set; }
            public int EndDate { get; set; }
            public double StrikePrice { get; set; }
            public double UAInsCode { get; set; }
            public long BuyOP { get; set; }
            public bool Flag { get; set; }
            public string CSocCSAC { get; set; }
            public string LSoc30 { get; set; }
            public string CIsin { get; set; }
            public string LVal18 { get; set; }
            public string NInsCode { get; set; }
            public double? HeadPDrCotVal { get; set; }
            public double? HeadPriceChange { get; set; }
            public double? BlackScholes { get; set; }
        }

        public class InstTrade
        {
            public int DEven { get; set; }
            public long InsCode { get; set; }
            public double PClosing { get; set; }
            public double Swing { get; set; }
        }
    }
}