using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlBulkTools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Web;

namespace CallOptionApi_V3.App_Cls
{
    public class AppFuncs
    {

        private void checkSwing(int incCode)
        {
            DBFuncs.GetCountRecord(AppVariable.ConStr, AppVariable.Tbl_Swings, "IncCode = " + incCode);
        }

        private void getLastSwing(int incCode)
        {
            DBFuncs.GetCountRecord(AppVariable.ConStr, AppVariable.Tbl_Swings, "IncCode = " + incCode);
        }

        internal static void insertSwing(long incCode, int dateFrom, int dateTo)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.InstTrade(AppVariable.myUserName, AppVariable.myPassword, incCode, dateFrom, dateTo);

            DataTable dt = ds.Tables[0];

            JArray jarr = DBFuncs.DataTableToJSONArray(dt);
            List<AppClasses.InstTrade> lst = jarr.ToObject<List<AppClasses.InstTrade>>().OrderBy(c => c.DEven).ToList();
            double lastPClosing = 0;
            for (int i = 0; i < lst.Count; i++)
            {
                if (i > 0)
                {
                    lst[i].Swing = 100 * Math.Log(lst[i].PClosing / lastPClosing);
                }
                lastPClosing = lst[i].PClosing;
            }
            lst.RemoveAll(x => x.Swing > 10 || x.Swing < -10);

            bulkUpdteInsert_InstTrade(AppVariable.ConStr, lst);

        }

        internal static bool checkSwing(long incCode, int currentDate)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.InstTrade(AppVariable.myUserName, AppVariable.myPassword, incCode, currentDate, currentDate);

            decimal PClosing_Get = ds.Tables[0].Rows[0].Field<decimal>("PClosing");

            decimal? PClosing_Set = DBFuncs.GetDecimalFromTDB(AppVariable.ConStr, AppVariable.Tbl_Swings, "PClosing", "IncCode = " + incCode + " and DEven = " + currentDate);

            if (PClosing_Set != null && PClosing_Set == PClosing_Get)
                return true;
            else
                return false;
        }

        static void bulkUpdteInsert_InstTrade(string conStr, IEnumerable<AppClasses.InstTrade> data)
        {
            var bulk = new BulkOperations();

            var ret = bulk.Setup<AppClasses.InstTrade>(x => x.ForCollection(data))
                .WithTable(AppVariable.Tbl_Swings)
                .AddAllColumns()
                .BulkInsertOrUpdate()
                .DeleteWhenNotMatched(true)
                .MatchTargetOn(x => x.InsCode)
                .MatchTargetOn(x => x.DEven);

            bulk.CommitTransaction(new SqlConnection(conStr));
        }

        /// <summary>
        /// انحراف معیار
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        internal static double CalculateStandardDeviation(IEnumerable<double> values)
        {
            double standardDeviation = 0;

            if (values.Any())
            {
                // Compute the average.     
                double avg = values.Average();

                // Perform the Sum of (value-avg)_2_2.      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));

                // Put it all together.      
                standardDeviation = Math.Sqrt((sum) / (values.Count() - 1));
            }

            return standardDeviation;
        }

        /// <summary>
        /// Summary description for BlackSholes.
        /// </summary>
        //public class BlackSholes
        //{
        //public BlackSholes()
        //{
        //    //
        //    // TODO: Add constructor logic here
        //    //
        //}
        /* The Black and Scholes (1973) Stock option formula
         * C# Implementation
         * uses the C# Math.PI field rather than a constant as in the C++ implementaion
         * the value of Pi is 3.14159265358979323846

          S= Stock price
            X=Strike price
            T=Years to maturity
            r= Risk-free rate
            v=Volatility
        */
        public static double BlackScholes(string CallPutFlag, double StockPrice, double StrikePrice,
            double Time, double Rate, double Sigma)
        {
            double d1 = 0.0;
            double d2 = 0.0;
            double dBlackScholes = 0.0;

            d1 = (Math.Log(StockPrice / StrikePrice) + (Rate + Sigma * Sigma / 2.0) * Time) / (Sigma * Math.Sqrt(Time));
            d2 = d1 - Sigma * Math.Sqrt(Time);
            if (CallPutFlag == "c")
            {
                dBlackScholes = StockPrice * CND(d1) - StrikePrice * Math.Exp(-Rate * Time) * CND(d2);
            }
            else if (CallPutFlag == "p")
            {
                dBlackScholes = StrikePrice * Math.Exp(-Rate * Time) * CND(-d2) - StockPrice * CND(-d1);
            }
            return dBlackScholes;
        }
        public static double CND(double X)
        {
            double L = 0.0;
            double K = 0.0;
            double dCND = 0.0;
            const double a1 = 0.31938153;
            const double a2 = -0.356563782;
            const double a3 = 1.781477937;
            const double a4 = -1.821255978;
            const double a5 = 1.330274429;
            L = Math.Abs(X);
            K = 1.0 / (1.0 + 0.2316419 * L);
            dCND = 1.0 - 1.0 / Math.Sqrt(2 * Convert.ToDouble(Math.PI.ToString())) *
                Math.Exp(-L * L / 2.0) * (a1 * K + a2 * K * K + a3 * Math.Pow(K, 3.0) +
                a4 * Math.Pow(K, 4.0) + a5 * Math.Pow(K, 5.0));

            if (X < 0)
            {
                return 1.0 - dCND;
            }
            else
            {
                return dCND;
            }
        }
        //}


    }
}