using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Globalization;
using System.Data.SqlClient;
using System.Configuration;
using CallOptionApi_V3.App_Cls;
using System.Transactions;
using SqlBulkTools;


namespace CallOptionApi_V3.Controllers
{
    public class DataController : ApiController
    {

        JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings { DateFormatString = "yyyy-MM-dd'T'HH:mm:ss+03:30" };

        private DateTime currentDate = DateTime.Now;

        [System.Web.Http.HttpGet]
        [Route("api/TradeLastDayOptionData")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetTradeLastDayOptionData(byte flow, bool baseUpdate = false)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();
            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            DataSet ds_TradeLastDay_Master = tsePublicV2SoapClient.TradeLastDay(AppVariable.myUserName, AppVariable.myPassword, flow);

            DataSet ds_Option = tsePublicV2SoapClient.Option(AppVariable.myUserName, AppVariable.myPassword);
            DataSet ds_BestLimits = tsePublicV2SoapClient.BestLimitsAllIns(AppVariable.myUserName, AppVariable.myPassword, flow);
            DataSet ds_Instrument = tsePublicV2SoapClient.Instrument(AppVariable.myUserName, AppVariable.myPassword, flow);

            DataSet ds_Company_1 = tsePublicV2SoapClient.Company(AppVariable.myUserName, AppVariable.myPassword, 1);
            DataSet ds_Company_2 = tsePublicV2SoapClient.Company(AppVariable.myUserName, AppVariable.myPassword, 2);

            DataSet ds_TradeLastDay_1 = tsePublicV2SoapClient.TradeLastDay(AppVariable.myUserName, AppVariable.myPassword, 1);
            DataSet ds_TradeLastDay_2 = tsePublicV2SoapClient.TradeLastDay(AppVariable.myUserName, AppVariable.myPassword, 2);

            try
            {
                currentDate = DBFuncs.GetNetworkTime();
            }
            catch (Exception e)
            {
            }

            //var vv = DBFuncs.GetAsync("").Result;

            if (ds_TradeLastDay_Master != null)
            {
                ds_TradeLastDay_Master.Tables[0].Columns.Add("DayCount", typeof(System.Int32));

                ds_TradeLastDay_Master.Tables[0].Columns.Add("PMeOf", typeof(System.Int32));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("PMeDem", typeof(System.Int32));

                ds_TradeLastDay_Master.Tables[0].Columns.Add("InstrumentID", typeof(System.String));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("BeginDate", typeof(System.Int32));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("EndDate", typeof(System.Int32));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("StrikePrice", typeof(System.Double));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("UAInsCode", typeof(System.Double));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("BuyOP", typeof(System.Int32));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("Flag", typeof(System.Boolean));

                ds_TradeLastDay_Master.Tables[0].Columns.Add("CSocCSAC", typeof(System.String));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("LSoc30", typeof(System.String));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("CIsin", typeof(System.String));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("LVal18", typeof(System.String));

                ds_TradeLastDay_Master.Tables[0].Columns.Add("NInsCode", typeof(System.String));

                ds_TradeLastDay_Master.Tables[0].Columns.Add("HeadPDrCotVal", typeof(System.Double));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("HeadPriceChange", typeof(System.Double));

                ds_TradeLastDay_Master.Tables[0].Columns.Add("BlackScholes", typeof(System.Double));

                //int currentDay = int.Parse(cc.ToString("yyyyMMdd"));
                foreach (DataRow row in ds_TradeLastDay_Master.Tables[0].Rows)
                {

                    var dr_BestLimits = ds_BestLimits.Tables[0].Select($"InsCode = {row["InsCode"]} and number = 1");
                    if (dr_BestLimits.Count() > 0)
                    {
                        row["PMeDem"] = dr_BestLimits[0]["PMeDem"];
                        row["PMeOf"] = dr_BestLimits[0]["PMeOf"];
                    }

                    if (baseUpdate)
                    {
                        var dr_Instrument = ds_Instrument.Tables[0].Select($"InsCode = {row["InsCode"]}");
                        if (dr_Instrument.Count() > 0)
                        {
                            row["CSocCSAC"] = dr_Instrument[0]["CSocCSAC"];
                            row["LSoc30"] = dr_Instrument[0]["LSoc30"];
                            row["CIsin"] = dr_Instrument[0]["CIsin"];
                            row["LVal18"] = dr_Instrument[0]["LVal18"];

                            var dr_Company_1 = ds_Company_1.Tables[0].Select($"LVal30 = '" + row["LSoc30"] + "'");
                            if (dr_Company_1.Count() > 0)
                            {
                                row["CIsin"] = dr_Company_1[0]["CIsin"];
                                row["NInsCode"] = dr_Company_1[0]["NInsCode"];

                                var dr_TradeLastDay_1 = ds_TradeLastDay_1.Tables[0].Select($"InsCode = {row["NInsCode"]}");
                                if (dr_TradeLastDay_1.Count() > 0)
                                {
                                    row["HeadPDrCotVal"] = dr_TradeLastDay_1[0]["PDrCotVal"];
                                    row["HeadPriceChange"] = dr_TradeLastDay_1[0]["PriceChange"];
                                }
                            }
                            else {
                                var dr_Company_2 = ds_Company_2.Tables[0].Select($"LVal30 = '" + row["LSoc30"] + "'");
                                if (dr_Company_2.Count() > 0)
                                {
                                    row["CIsin"] = dr_Company_2[0]["CIsin"];
                                    row["NInsCode"] = dr_Company_2[0]["NInsCode"];

                                    var dr_TradeLastDay_2 = ds_TradeLastDay_2.Tables[0].Select($"InsCode = {row["NInsCode"]}");
                                    if (dr_TradeLastDay_2.Count() > 0)
                                    {
                                        row["HeadPDrCotVal"] = dr_TradeLastDay_2[0]["PDrCotVal"];
                                        row["HeadPriceChange"] = dr_TradeLastDay_2[0]["PriceChange"];
                                    }
                                }
                            }
                        }


                        //row["BlackScholes"] = FncCls.BlackScholes("c", Double.Parse(row["HeadPriceChange"].ToString()), Double.Parse(row["HeadPriceChange"].ToString())
                        //    , Double.Parse(row["HeadPriceChange"].ToString()), 1, 2);
                    }

                    var dr_Option = ds_Option.Tables[0].Select($"InsCode = {row["InsCode"]}");
                    if (dr_Option.Count() > 0)
                    {
                        row["InstrumentID"] = dr_Option[0]["InstrumentID"];
                        row["UAInsCode"] = dr_Option[0]["UAInsCode"];
                        row["BeginDate"] = dr_Option[0]["BeginDate"];
                        row["EndDate"] = dr_Option[0]["EndDate"];
                        row["StrikePrice"] = dr_Option[0]["StrikePrice"];
                        row["BuyOP"] = dr_Option[0]["BuyOP"];
                        row["Flag"] = 1;
                        row["BlackScholes"] = 1.0;

                        DateTime dt_EndDate;
                        if (DateTime.TryParseExact(row["EndDate"].ToString(), "yyyyMMdd",
                                                  CultureInfo.InvariantCulture,
                                                  DateTimeStyles.None, out dt_EndDate))
                        {
                            row["DayCount"] = (dt_EndDate - currentDate).TotalDays;
                        }
                    }
                    else
                    {
                        row["Flag"] = 0;
                    }
                }

                DataTable dt_fetch = ds_TradeLastDay_Master.Tables[0].AsEnumerable().Where(
                    row => row.Field<bool>("Flag")).CopyToDataTable();

                //string tempTableTxtCmd = GetTempTableCreateCmd(dt, "temp_"+AppVariable.Tbl_TradeDayOption);
                //ExecuteCmd(tempTableTxtCmd, new SqlConnection(AppVariable.ConStr));
                //string toTemp = GetOriginalTblToTempTableUpdateCmd(dt, AppVariable.Tbl_TradeDayOption, tempTableTxtCmd);
                //ExecuteCmd(toTemp, new SqlConnection(AppVariable.ConStr));
                //var dropTempTableCmd = $"DROP TABLE " + "temp_"+ AppVariable.Tbl_TradeDayOption;
                //ExecuteCmd(dropTempTableCmd, new SqlConnection(AppVariable.ConStr));
                bulkUpdteInsert_Check(AppVariable.ConStr, TradeLastDay_dtToIEnumerable(dt_fetch));

                if (countRecords(AppVariable.ConStr) > 0)
                {
                    bulkUpdteInsert(AppVariable.ConStr, TradeLastDay_dtToIEnumerable(dt_fetch));

                    //checkSwing();
                }

                DataTable dt = DBFuncs.GetRecords(AppVariable.ConStr, AppVariable.Tbl_TradeDayOption, null, null, null);

                foreach (DataRow dr in dt.Rows)
                {
                    var t1 = dr["NInsCode"];
                    var t2 = dr["LastSwingSet"];

                    if (dr["NInsCode"] != null && dr["NInsCode"].ToString().Length > 0 &&
                        ((dr["LastSwingSet"] == null || dr["LastSwingSet"].ToString().Length == 0)
                         || (dr.Field<DateTime>("LastSwingSet") != DateTime.Now )))
                    {
                        //long mInsCode = 7745894403636165;
                        //AppFuncs.insertSwing(mInsCode, 20000101, 20220119);
                        string sInsCode = dr.Field<string>("NInsCode");

                        double E22, E66, E132, E245;

                        EnumerableRowCollection<DataRow> filteredRows = dt.AsEnumerable().Where(r => r.Field<String>("NInsCode") == sInsCode &&
                        r["LastSwingSet"] != null && r["LastSwingSet"].ToString().Length > 0 &&
                        r.Field<DateTime>("LastSwingSet") == DateTime.Now);
                        if(filteredRows.Count() > 0)
                        {
                            E22 = filteredRows.First<DataRow>().Field<double>("N22");
                            E66 = filteredRows.First<DataRow>().Field<double>("N66");
                            E132 = filteredRows.First<DataRow>().Field<double>("N132");
                            E245 = filteredRows.First<DataRow>().Field<double>("N245");
                        }
                        else // calculate swings
                        {
                            long mInsCode = long.Parse(sInsCode);

                            AppFuncs.insertSwing(mInsCode, 20000101, DBFuncs.ConvertInvoiceDate(DateTime.Now));

                            List<double> lstSwings = DBFuncs.GetFieldList_DecimalAsDouble(AppVariable.ConStr, AppVariable.Tbl_Swings, "Swing", "InsCode = '+" + mInsCode + "'", "DEven Desc");

                            var v1 = lstSwings.AsEnumerable().Take(22);
                            var v2 = lstSwings.AsEnumerable().Take(66);
                            var v3 = lstSwings.AsEnumerable().Take(132);
                            var v4 = lstSwings.AsEnumerable().Take(245);

                            E22 = AppFuncs.CalculateStandardDeviation(lstSwings.AsEnumerable().Take(22)) * Math.Sqrt(245);
                            E66 = AppFuncs.CalculateStandardDeviation(lstSwings.AsEnumerable().Take(66)) * Math.Sqrt(245);
                            E132 = AppFuncs.CalculateStandardDeviation(lstSwings.AsEnumerable().Take(132)) * Math.Sqrt(245);
                            E245 = AppFuncs.CalculateStandardDeviation(lstSwings.AsEnumerable().Take(245)) * Math.Sqrt(245);
                        }

                        double BS = AppFuncs.BlackScholes("c",
                            Decimal.ToDouble(dr.Field<decimal>("HeadPDrCotVal")),
                            Decimal.ToDouble(dr.Field<decimal>("PDrCotVal")),
                            dr.Field<int>("DayCount")/365, 25,
                            Decimal.ToDouble(dr.Field<decimal>("N245")));

                        DBFuncs.SyncNic S = new DBFuncs.SyncNic();
                        S.Table = AppVariable.Tbl_TradeDayOption;
                        S.Records.Add(new DBFuncs.SyncNic.InsRec(DBFuncs.SyncNic.FType.UPD, "N22", E22));
                        S.Records.Add(new DBFuncs.SyncNic.InsRec(DBFuncs.SyncNic.FType.UPD, "N66", E66));
                        S.Records.Add(new DBFuncs.SyncNic.InsRec(DBFuncs.SyncNic.FType.UPD, "N132", E132));
                        S.Records.Add(new DBFuncs.SyncNic.InsRec(DBFuncs.SyncNic.FType.UPD, "N245", E245));
                        S.Records.Add(new DBFuncs.SyncNic.InsRec(DBFuncs.SyncNic.FType.UPD, "LastSwingSet", DateTime.Now));
                        S.Records.Add(new DBFuncs.SyncNic.InsRec(DBFuncs.SyncNic.FType.UPD, "BlackScholes", BS));
                        S.Records.Add(new DBFuncs.SyncNic.InsRec(DBFuncs.SyncNic.FType.CHK, "NInsCode", sInsCode));
                        DBFuncs.SyncNic.UpdateRecord(AppVariable.ConStr, S);
                    }

                    //if (dr.Field<> 0)
                }



                //DataTable lstTradeDay = DBFuncs.GetRecords(AppVariable.ConStr, AppVariable.Tbl_TradeDayOption, null, "NInsCode = '+" + mInsCode + "'", null);


            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }



        public static IEnumerable<AppClasses.TradeLastDayOptionData> TradeLastDay_dtToIEnumerable(System.Data.DataTable dataTable)
        {
            var retList = new List<AppClasses.TradeLastDayOptionData>();


            //foreach (DataColumn dc in dataTable.Columns)
            //{
            //    bulkcopy.ColumnMappings.Add(dc.Caption, dc.Caption);
            //}

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = dataTable.Rows[i];

                var temp = new AppClasses.TradeLastDayOptionData()
                {
                    InsCode = Convert.ToInt64(row["InsCode"]),
                    BeginDate = Convert.ToInt32(row["BeginDate"]),
                    BuyOP = Convert.ToInt64(row["BuyOP"]),
                    DayCount = Convert.ToInt32(row["DayCount"]),
                    DEven = Convert.ToInt32(row["DEven"]),
                    EndDate = Convert.ToInt32(row["EndDate"]),
                    HEven = Convert.ToInt32(row["HEven"]),
                    Last = Convert.ToInt64(row["Last"]),
                    PMeDem = Convert.ToInt32(row["PMeDem"]),
                    PMeOf = Convert.ToInt32(row["PMeOf"]),

                    Flag = Convert.ToBoolean(row["Flag"]),

                    BlackScholes = row["BlackScholes"].GetType() == typeof(double) ? Convert.ToDouble(row["BlackScholes"]) : 0,
                    HeadPDrCotVal = row["HeadPDrCotVal"].GetType() == typeof(double) ? Convert.ToDouble(row["HeadPDrCotVal"]) : 0,
                    HeadPriceChange = row["HeadPriceChange"].GetType() == typeof(double) ? Convert.ToDouble(row["HeadPriceChange"]) : 0,

                    PClosing = Convert.ToDouble(row["PClosing"]),
                    PDrCotVal = Convert.ToDouble(row["PDrCotVal"]),
                    PriceChange = Convert.ToDouble(row["PriceChange"]),
                    PriceFirst = Convert.ToDouble(row["PriceFirst"]),
                    PriceMax = Convert.ToDouble(row["PriceMax"]),
                    PriceMin = Convert.ToDouble(row["PriceMin"]),
                    PriceYesterday = Convert.ToDouble(row["PriceYesterday"]),
                    QTotCap = Convert.ToDouble(row["QTotCap"]),
                    QTotTran5J = Convert.ToDouble(row["QTotTran5J"]),
                    StrikePrice = Convert.ToDouble(row["StrikePrice"]),
                    UAInsCode = Convert.ToDouble(row["UAInsCode"]),
                    ZTotTran = Convert.ToDouble(row["ZTotTran"]),

                    //N22 = Convert.ToDouble(row["N22"]),
                    //ZTotTran = Convert.ToDouble(row["ZTotTran"]),
                    //ZTotTran = Convert.ToDouble(row["ZTotTran"]),
                    //ZTotTran = Convert.ToDouble(row["ZTotTran"]),

                    CIsin = (row["CIsin"]).ToString(),
                    CSocCSAC = (row["CSocCSAC"]).ToString(),
                    InstrumentID = (row["InstrumentID"]).ToString(),
                    LSoc30 = (row["LSoc30"]).ToString(),
                    LVal18 = (row["LVal18"]).ToString(),
                    LVal18AFC = (row["LVal18AFC"]).ToString(),
                    LVal30 = (row["LVal30"]).ToString(),
                    NInsCode = (row["NInsCode"]).ToString(),

                };

                retList.Add(temp);
            }

            return retList;
        }


        //static void bulkUpdteInsert(string conStr, IEnumerable<AppClasses.TradeLastDayOptionData> data)
        //{
        //    var bulk = new BulkOperations();

        //    using (TransactionScope trans = new TransactionScope())
        //    {
        //        using (SqlConnection conn = new SqlConnection(conStr))
        //        {
        //            var b = bulk.Setup<AppClasses.TradeLastDayOptionData>()
        //                .ForCollection(data)
        //                .WithTable(AppVariable.Tbl_TradeDayOption)
        //                .AddAllColumns()
        //                .BulkInsertOrUpdate()
        //                .MatchTargetOn(x => x.InsCode)
        //                .Commit(conn);
        //        }

        //        trans.Complete();
        //    }
        //}

        // 4.5
        //static void bulkUpdteInsert1(string conStr, IEnumerable<AppClasses.TradeLastDayOptionData> data)
        //{
        //    var bulk = new BulkOperations();

        //    var b = bulk.Setup().ForCollection(data)
        //     .WithTable(AppVariable.Tbl_TradeDayOption)
        //     .AddAllColumns()
        //     .BulkInsertOrUpdate()
        //     .DeleteWhenNotMatched(true)
        //     //.AddColumn(x => x.BeginDate)
        //     //.AddColumn(x => x.BlackScholes)
        //     //.AddColumn(x => x.Description)
        //     //.BulkInsertOrUpdate().
        //     .MatchTargetOn(x => x.InsCode).Commit(new SqlConnection(conStr));

        //    //bulk.CommitTransaction(new SqlConnection(conStr));
        //}

        static void bulkUpdteInsert(string conStr, IEnumerable<AppClasses.TradeLastDayOptionData> data)
        {
            var bulk = new BulkOperations();


            var ret = bulk.Setup<AppClasses.TradeLastDayOptionData>(x => x.ForCollection(data))
                .WithTable(AppVariable.Tbl_TradeDayOption)
                .AddAllColumns()
                .BulkInsertOrUpdate()
                .DeleteWhenNotMatched(true)
                .MatchTargetOn(x => x.InsCode);

            bulk.CommitTransaction(new SqlConnection(conStr));
        }

        static void bulkUpdteInsert_Check(string conStr, IEnumerable<AppClasses.TradeLastDayOptionData> data)
        {
            var bulk = new BulkOperations();
            bulk.Setup<AppClasses.TradeLastDayOptionData>(x => x.ForCollection(data))
                .WithTable(AppVariable.Tbl_TradeDayOption_Tmp)
                .AddAllColumns()
                .BulkInsertOrUpdate()
                .DeleteWhenNotMatched(true)
                .MatchTargetOn(x => x.InsCode);

            bulk.CommitTransaction(new SqlConnection(conStr));


        }

        static int countRecords(string conStr)
        {
            int affectedRows = 0;
            AppClasses.TradeLastDayOptionData tradeLastDayOptionData = new AppClasses.TradeLastDayOptionData();
            List<string> columnsList = new List<string>();
            foreach (var field in tradeLastDayOptionData.GetType().GetProperties())
            {
                columnsList.Add(field.Name);
            }
            var commCount = CompareSet(columnsList, AppVariable.Tbl_TradeDayOption, AppVariable.Tbl_TradeDayOption_Tmp);

            using (SqlConnection conn = new SqlConnection(conStr))
            {
                conn.Open();
                //var dtCols = _helper.GetDatabaseSchema(conn, _schema, _tableName);
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        SqlCommand command = conn.CreateCommand();
                        command.Connection = conn;
                        command.Transaction = transaction;
                        ////command.CommandTimeout = _sqlTimeout;

                        //Creating temp table on database
                        command.CommandText = commCount;
                        object o = command.ExecuteScalar();
                        if (o != null)
                        {
                            affectedRows = Int32.Parse(o.ToString());
                        }

                        transaction.Commit();

                    }

                    catch (SqlException e)
                    {
                        transaction.Rollback();
                        throw;
                    }

                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            return affectedRows;

            //var affectedRows = commandCount.ExecuteNonQuery();
        }

        internal static string CompareSet(List<string> columns, string sourceAlias, string targetAlias, string identityColumn = null)
        {
            var command = new StringBuilder();
            var paramsSeparated = new List<string>();

            command.Append("SELECT COUNT(*) FROM (SELECT * FROM " + sourceAlias + " a Where NOT EXISTS (SELECT 'x' FROM " + targetAlias + " b WHERE ");

            foreach (var column in columns.ToList())
            {
                if (identityColumn != null && column != identityColumn || identityColumn == null)
                {
                    paramsSeparated.Add("[a]" + "." + "[" + column + "]" + " = " + "[b]" + "." + "[" + column + "]");
                }
            }

            command.Append(string.Join(" and ", paramsSeparated) + " ");
            paramsSeparated.Clear();
            paramsSeparated = new List<string>();

            command.Append(") union (SELECT * FROM " + targetAlias + " a Where NOT EXISTS (SELECT 'x' FROM " + sourceAlias + " b WHERE ");

            foreach (var column in columns.ToList())
            {
                if (identityColumn != null && column != identityColumn || identityColumn == null)
                {
                    paramsSeparated.Add("[a]" + "." + "[" + column + "]" + " = " + "[b]" + "." + "[" + column + "]");
                }
            }

            command.Append(string.Join(" and ", paramsSeparated) + " ");

            command.Append("))) tbl");

            return command.ToString();
        }

        //*************************************
        //static void s_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        //{
        //    Console.WriteLine("-- Copied {0} rows.", e.RowsCopied);
        //}

        //string GetTempTableCreateCmd(DataTable dataTable, string tempTable)
        //{
        //    StringBuilder columnTxt = new StringBuilder();
        //    columnTxt.Append($"CREATE TABLE {tempTable}(");
        //    int columnCount = dataTable.Columns.Count;
        //    for (int i = 0; i < columnCount; i++)
        //    {
        //        string dataType = dataTable.Columns[i].DataType == Type.GetType("System.String") ? "VARCHAR(100) " : dataTable.Columns[i].DataType.ToString();
        //        string colum = $"{dataTable.Columns[i]} {dataType}";
        //        columnTxt.Append($"{colum}");
        //        if (i != columnCount - 1)
        //            columnTxt.Append(", ");
        //    }
        //    columnTxt.Append(");");
        //    return columnTxt.ToString();
        //}


        string GetOriginalTblToTempTableUpdateCmd(DataTable dataTable, string originalTable, string tempTable)
        {
            StringBuilder updateTblCmd = new StringBuilder();
            updateTblCmd.Append("UPDATE ORGI SET ");

            for (int i = 1; i < dataTable.Columns.Count; i++)
            {
                updateTblCmd.Append($"ORGI.{dataTable.Columns[i]} = TEMP.{dataTable.Columns[i]}");

                if (i != dataTable.Columns.Count - 1)
                    updateTblCmd.Append(", ");
            }

            updateTblCmd.Append($" FROM {tempTable} TEMP INNER JOIN {originalTable} ORGI ON ORGI.{dataTable.Columns[0]} = TEMP.{dataTable.Columns[0]}");

            return updateTblCmd.ToString();
        }

        private void ExecuteCmd(string cmdTxt, SqlConnection connection)
        {
            using (SqlCommand cmd = new SqlCommand(cmdTxt, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }
        //*********************************

        [System.Web.Http.HttpGet]
        [Route("api/TradeLastDayOption")]
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetTradeLastDayOption(byte flow, bool baseUpdate = false)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();
            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            DataSet ds_TradeLastDay_Master = tsePublicV2SoapClient.TradeLastDay(AppVariable.myUserName, AppVariable.myPassword, flow);

            DataSet ds_Option = tsePublicV2SoapClient.Option(AppVariable.myUserName, AppVariable.myPassword);
            DataSet ds_BestLimits = tsePublicV2SoapClient.BestLimitsAllIns(AppVariable.myUserName, AppVariable.myPassword, flow);
            DataSet ds_Instrument = tsePublicV2SoapClient.Instrument(AppVariable.myUserName, AppVariable.myPassword, flow);

            //DataSet ds_Instrument_1 = tsePublicV2SoapClient.Instrument(AppVariable.myUserName, AppVariable.myPassword, 1);
            //DataSet ds_Instrument_2 = tsePublicV2SoapClient.Instrument(AppVariable.myUserName, AppVariable.myPassword, 2);

            DataSet ds_Company_1 = tsePublicV2SoapClient.Company(AppVariable.myUserName, AppVariable.myPassword, 1);
            DataSet ds_Company_2 = tsePublicV2SoapClient.Company(AppVariable.myUserName, AppVariable.myPassword, 2);

            DataSet ds_TradeLastDay_1 = tsePublicV2SoapClient.TradeLastDay(AppVariable.myUserName, AppVariable.myPassword, 1);
            DataSet ds_TradeLastDay_2 = tsePublicV2SoapClient.TradeLastDay(AppVariable.myUserName, AppVariable.myPassword, 2);

            try
            {
                currentDate = DBFuncs.GetNetworkTime();
            }
            catch (Exception e)
            {
            }

            //var vv = DBFuncs.GetAsync("").Result;

            if (ds_TradeLastDay_Master != null)
            {
                ds_TradeLastDay_Master.Tables[0].Columns.Add("DayCount", typeof(System.Int32));

                ds_TradeLastDay_Master.Tables[0].Columns.Add("PMeOf", typeof(System.Int32));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("PMeDem", typeof(System.Int32));

                ds_TradeLastDay_Master.Tables[0].Columns.Add("InstrumentID", typeof(System.String));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("BeginDate", typeof(System.Int32));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("EndDate", typeof(System.Int32));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("StrikePrice", typeof(System.Double));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("UAInsCode", typeof(System.Double));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("BuyOP", typeof(System.Int32));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("Flag", typeof(System.Boolean));

                ds_TradeLastDay_Master.Tables[0].Columns.Add("CSocCSAC", typeof(System.String));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("LSoc30", typeof(System.String));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("CIsin", typeof(System.String));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("LVal18", typeof(System.String));

                ds_TradeLastDay_Master.Tables[0].Columns.Add("NInsCode", typeof(System.String));

                ds_TradeLastDay_Master.Tables[0].Columns.Add("HeadPDrCotVal", typeof(System.Double));
                ds_TradeLastDay_Master.Tables[0].Columns.Add("HeadPriceChange", typeof(System.Double));

                ds_TradeLastDay_Master.Tables[0].Columns.Add("BlackScholes", typeof(System.Double));

                //int currentDay = int.Parse(cc.ToString("yyyyMMdd"));
                foreach (DataRow row in ds_TradeLastDay_Master.Tables[0].Rows)
                {

                    var dr_BestLimits = ds_BestLimits.Tables[0].Select($"InsCode = {row["InsCode"]} and number = 1");
                    if (dr_BestLimits.Count() > 0)
                    {
                        row["PMeDem"] = dr_BestLimits[0]["PMeDem"];
                        row["PMeOf"] = dr_BestLimits[0]["PMeOf"];
                    }

                    if (baseUpdate)
                    {
                        var dr_Instrument = ds_Instrument.Tables[0].Select($"InsCode = {row["InsCode"]}");
                        if (dr_Instrument.Count() > 0)
                        {
                            row["CSocCSAC"] = dr_Instrument[0]["CSocCSAC"];
                            row["LSoc30"] = dr_Instrument[0]["LSoc30"];
                            row["CIsin"] = dr_Instrument[0]["CIsin"];
                            row["LVal18"] = dr_Instrument[0]["LVal18"];

                            var dr_Company_1 = ds_Company_1.Tables[0].Select($"LVal30 = '" + row["LSoc30"] + "'");
                            if (dr_Company_1.Count() > 0)
                            {
                                row["CIsin"] = dr_Company_1[0]["CIsin"];
                                row["NInsCode"] = dr_Company_1[0]["NInsCode"];

                                var dr_TradeLastDay_1 = ds_TradeLastDay_1.Tables[0].Select($"InsCode = {row["NInsCode"]}");
                                if (dr_TradeLastDay_1.Count() > 0)
                                {
                                    row["HeadPDrCotVal"] = dr_TradeLastDay_1[0]["PDrCotVal"];
                                    row["HeadPriceChange"] = dr_TradeLastDay_1[0]["PriceChange"];
                                }
                            }
                            else {
                                var dr_Company_2 = ds_Company_2.Tables[0].Select($"LVal30 = '" + row["LSoc30"] + "'");
                                if (dr_Company_2.Count() > 0)
                                {
                                    row["CIsin"] = dr_Company_2[0]["CIsin"];
                                    row["NInsCode"] = dr_Company_2[0]["NInsCode"];

                                    var dr_TradeLastDay_2 = ds_TradeLastDay_2.Tables[0].Select($"InsCode = {row["NInsCode"]}");
                                    if (dr_TradeLastDay_2.Count() > 0)
                                    {
                                        row["HeadPDrCotVal"] = dr_TradeLastDay_2[0]["PDrCotVal"];
                                        row["HeadPriceChange"] = dr_TradeLastDay_2[0]["PriceChange"];
                                    }
                                }
                            }
                            row["BlackScholes"] = 0;
                        }

                        row["BlackScholes"] = AppFuncs.BlackScholes("c", Double.Parse(row["HeadPriceChange"].ToString()), Double.Parse(row["HeadPriceChange"].ToString())
                            , Double.Parse(row["HeadPriceChange"].ToString()), 1, 2);
                    }

                    var dr_Option = ds_Option.Tables[0].Select($"InsCode = {row["InsCode"]}");
                    if (dr_Option.Count() > 0)
                    {
                        row["InstrumentID"] = dr_Option[0]["InstrumentID"];
                        row["UAInsCode"] = dr_Option[0]["UAInsCode"];
                        row["BeginDate"] = dr_Option[0]["BeginDate"];
                        row["EndDate"] = dr_Option[0]["EndDate"];
                        row["StrikePrice"] = dr_Option[0]["StrikePrice"];
                        row["BuyOP"] = dr_Option[0]["BuyOP"];
                        row["Flag"] = true;

                        DateTime dt_EndDate;
                        if (DateTime.TryParseExact(row["EndDate"].ToString(), "yyyyMMdd",
                                                  CultureInfo.InvariantCulture,
                                                  DateTimeStyles.None, out dt_EndDate))
                        {
                            row["DayCount"] = (dt_EndDate - currentDate).TotalDays;
                        }
                    }
                    else
                    {
                        row["Flag"] = false;
                    }
                    //ds.Tables[0].Rows[0]["Name"] = "AAAAAAAAA";
                }
                var JABanners = DBFuncs.DataTableToJSONArray(ds_TradeLastDay_Master.Tables[0].AsEnumerable().Where(row => row.Field<bool>("Flag")).CopyToDataTable());
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/OptionName")]
        public HttpResponseMessage GetOptionName()
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet _ds = tsePublicV2SoapClient.Company(AppVariable.myUserName, AppVariable.myPassword, 1);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Credentials", "true");
            response.Headers.Add("Access-Control-Allow-Headers", "Origin,Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token,locale");
            response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");

            //JArray JACompany;
            DataTable DTCompany = _ds.Tables[0];

            //if (_ds != null)
            //{
            //    JACompany = DataTableToJSONArray(_ds.Tables[0]);
            //    DTCompany = _ds.Tables[0];
            //}

            DataSet ds = tsePublicV2SoapClient.Option(AppVariable.myUserName, AppVariable.myPassword);

            if (ds != null)
            {
                ds.Tables[0].Columns.Add("Name", typeof(System.String));
                ds.Tables[0].Columns.Add("Flag", typeof(System.String));

                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    var dr = DTCompany.Select($"NInsCode = {row["UAInsCode"]}");
                    //DTCompany.Select("CIsin LIKE '%" + row["InstrumentID"] + "%'");
                    if (dr.Count() > 0)
                    {
                        row["Name"] = dr[0]["LVal30"];
                        row["Flag"] = dr[0]["LVal18AFC"];
                    }
                    else
                    {
                        row["Name"] = "NotFound";
                        row["Flag"] = "NotFound";
                    }
                    //ds.Tables[0].Rows[0]["Name"] = "AAAAAAAAA";
                }
                //                .AsEnumerable()
                //.Where(r => r.Field<String>("CREATOR").Contains(searchstring))
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/Option")]
        public HttpResponseMessage GetOption()
        {
            TsePublicV2SoapClient.TsePublicV2SoapClient tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.Option(AppVariable.myUserName, AppVariable.myPassword);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/Board")]
        public HttpResponseMessage GetBoard()
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.Board(AppVariable.myUserName, AppVariable.myPassword);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/AdjPrceAllByCIsin")]
        public HttpResponseMessage GetAdjPrceAllByCIsin(string CIsin)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataTable ds = tsePublicV2SoapClient.AdjPrceAllByCIsin(AppVariable.myUserName, AppVariable.myPassword, CIsin);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/Instrument")]
        public HttpResponseMessage GetInstrument(byte Flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.Instrument(AppVariable.myUserName, AppVariable.myPassword, Flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/Instrument2")]
        public HttpResponseMessage GetInstrument2(byte Flow = 3, int cnt = 7, string iid = "IKCO")
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.Instrument(AppVariable.myUserName, AppVariable.myPassword, Flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);


            if (ds != null)
            {
                for (int i = ds.Tables[0].Columns.Count - 1; i > cnt; i--)
                {
                    ds.Tables[0].Columns.RemoveAt(i);
                }
                DataTable mm = new DataTable();
                mm = ds.Tables[0].AsEnumerable().Where(dr => dr.Field<String>("CSocCSAC") == iid).CopyToDataTable();

                var JABanners = DBFuncs.DataTableToJSONArray(mm);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/Instrument3")]
        public HttpResponseMessage GetInstrument3(byte Flow = 3)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.Instrument(AppVariable.myUserName, AppVariable.myPassword, Flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);


            if (ds != null)
            {
                //var bb = ds.Tables[0].Columns["Valid"].DataType;
                //for (int i = ds.Tables[0].Columns.Count - 1; i > cnt; i--)
                //{
                //    ds.Tables[0].Columns.RemoveAt(i);
                //}
                DataTable mm = new DataTable();
                mm = ds.Tables[0].AsEnumerable().Where(dr => dr.Field<Byte>("Valid") > 0
               && (dr.Field<Int32>("Yval") == 311 || dr.Field<Int32>("Yval") == 312)
                ).CopyToDataTable();

                var JABanners = DBFuncs.DataTableToJSONArray(mm);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/AdjPrice")]
        public HttpResponseMessage GetAdjPrice(byte Flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.AdjPrice(AppVariable.myUserName, AppVariable.myPassword, Flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/Auction")]
        public HttpResponseMessage GetAuction(long From)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataTable dt = tsePublicV2SoapClient.Auction(AppVariable.myUserName, AppVariable.myPassword, From);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (dt != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(dt);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/ClientType")]
        public HttpResponseMessage GetClientType()
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataTable dt = tsePublicV2SoapClient.ClientType(AppVariable.myUserName, AppVariable.myPassword);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (dt != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(dt);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/ClientTypeByDate")]
        public HttpResponseMessage GetClientTypeByDate(int date)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataTable dt = tsePublicV2SoapClient.ClientTypeByDate(AppVariable.myUserName, AppVariable.myPassword, date);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (dt != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(dt);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/ClientTypeByInsCode")]
        public HttpResponseMessage GetClientTypeByInsCode(long incCode)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataTable dt = tsePublicV2SoapClient.ClientTypeByInsCode(AppVariable.myUserName, AppVariable.myPassword, incCode);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (dt != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(dt);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/FutureInformation")]
        public HttpResponseMessage GetFutureInformation(int dEven)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataTable dt = tsePublicV2SoapClient.FutureInformation(AppVariable.myUserName, AppVariable.myPassword, dEven);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (dt != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(dt);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/BestLimitOneIns")]
        public HttpResponseMessage GetBestLimitOneIns(long IncCode)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.BestLimitOneIns(AppVariable.myUserName, AppVariable.myPassword, IncCode);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/BestLimitsAllIns")]
        public HttpResponseMessage GetBestLimitsAllIns(byte Flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.BestLimitsAllIns(AppVariable.myUserName, AppVariable.myPassword, Flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/Company")]
        public HttpResponseMessage GetCompany(byte Flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.Company(AppVariable.myUserName, AppVariable.myPassword, Flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/EnergyFuture")]
        public HttpResponseMessage GetEnergyFuture()
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.EnergyFuture(AppVariable.myUserName, AppVariable.myPassword);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/EnergyFutureCurrency")]
        public HttpResponseMessage GetEnergyFutureCurrency()
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.EnergyFutureCurrency(AppVariable.myUserName, AppVariable.myPassword);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/EnergyFutureCurrencyRate")]
        public HttpResponseMessage GetEnergyFutureCurrencyRate()
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.EnergyFutureCurrencyRate(AppVariable.myUserName, AppVariable.myPassword);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/EnergyFutureMOP")]
        public HttpResponseMessage GetEnergyFutureMOP()
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.EnergyFutureMOP(AppVariable.myUserName, AppVariable.myPassword);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/IndexB1LastDayLastData")]
        public HttpResponseMessage GetIndexB1LastDayLastData(byte Flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.IndexB1LastDayLastData(AppVariable.myUserName, AppVariable.myPassword, Flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/IndexB1LastDayOneInst")]
        public HttpResponseMessage GetIndexB1LastDayOneInst(long idxCode, byte Flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.IndexB1LastDayOneInst(AppVariable.myUserName, AppVariable.myPassword, idxCode, Flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/IndexB2")]
        public HttpResponseMessage GetIndexB2(int dEven)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.IndexB2(AppVariable.myUserName, AppVariable.myPassword, dEven);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/IndexInstrument")]
        public HttpResponseMessage GetIndexInstrument(long idxCode, byte Flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.IndexInstrument(AppVariable.myUserName, AppVariable.myPassword, idxCode, Flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/InstAffect")]
        public HttpResponseMessage GetInstAffect()
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.InstAffect(AppVariable.myUserName, AppVariable.myPassword);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/InstAffectByFlow")]
        public HttpResponseMessage GetInstAffectByFlow(byte flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.InstAffectByFlow(AppVariable.myUserName, AppVariable.myPassword, flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/InstrumentFilterByDate")]
        public HttpResponseMessage GetInstrumentFilterByDate(int dEven, byte flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.InstrumentFilterByDate(AppVariable.myUserName, AppVariable.myPassword, dEven, flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/InstrumentsState")]
        public HttpResponseMessage GetInstrumentsState(byte flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.InstrumentsState(AppVariable.myUserName, AppVariable.myPassword, flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/InstrumentsStateChange")]
        public HttpResponseMessage GetInstrumentsStateChange(int dEven, long incCode)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.InstrumentsStateChange(AppVariable.myUserName, AppVariable.myPassword, dEven, incCode);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/InstTrade")]
        public HttpResponseMessage GetInstTrade(long incCode, int dateFrom, int dateTo)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.InstTrade(AppVariable.myUserName, AppVariable.myPassword, incCode, dateFrom, dateTo);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/InstWithBestLimit")]
        public HttpResponseMessage GetInstWithBestLimit(byte flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.InstWithBestLimit(AppVariable.myUserName, AppVariable.myPassword, flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/MarketActivityDaily")]
        public HttpResponseMessage GetMarketActivityDaily(int dateFrom, int dateTo)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.MarketActivityDaily(AppVariable.myUserName, AppVariable.myPassword, dateFrom, dateTo);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/MarketActivityLast")]
        public HttpResponseMessage GetMarketActivityLast(byte flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.MarketActivityLast(AppVariable.myUserName, AppVariable.myPassword, flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/MarketValue")]
        public IHttpActionResult GetMarketValue()
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            decimal? val = null;
            try
            {
                val = tsePublicV2SoapClient.MarketValue(AppVariable.myUserName, AppVariable.myPassword);
            }
            catch (Exception e)
            {

            }

            if (val != null)
            {
                return Ok(val);
            }
            else
            {
                return NotFound();
            }
        }

        [System.Web.Http.HttpGet]
        [Route("api/MarketValueByDate")]
        public IHttpActionResult GetMarketValueByDate(int dEven)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            decimal? val = null;
            try
            {
                val = tsePublicV2SoapClient.MarketValueByDate(AppVariable.myUserName, AppVariable.myPassword, dEven);
            }
            catch (Exception e)
            {

            }

            if (val != null)
            {
                return Ok(val);
            }
            else
            {
                return NotFound();
            }
        }

        [System.Web.Http.HttpGet]
        [Route("api/Msg")]
        public HttpResponseMessage GetMsg()
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.Msg(AppVariable.myUserName, AppVariable.myPassword);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/NSCStart")]
        public HttpResponseMessage GetNSCStart()
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.NSCStart(AppVariable.myUserName, AppVariable.myPassword);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/PowerInstrument")]
        public HttpResponseMessage GetPowerInstrument()
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.PowerInstrument(AppVariable.myUserName, AppVariable.myPassword);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/PowerInstrumentHistory")]
        public HttpResponseMessage GetPowerInstrumentHistory(long from)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.PowerInstrumentHistory(AppVariable.myUserName, AppVariable.myPassword, from);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/Sector")]
        public HttpResponseMessage GetSector()
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.Sector(AppVariable.myUserName, AppVariable.myPassword);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/SectorState")]
        public HttpResponseMessage GetSectorState(int dEven)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.SectorState(AppVariable.myUserName, AppVariable.myPassword, dEven);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/ShareChange")]
        public HttpResponseMessage GetShareChange(byte flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.ShareChange(AppVariable.myUserName, AppVariable.myPassword, flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/StaticThresholds")]
        public HttpResponseMessage GetStaticThresholds(byte flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.StaticThresholds(AppVariable.myUserName, AppVariable.myPassword, flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/SubSector")]
        public HttpResponseMessage GetSubSector()
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.SubSector(AppVariable.myUserName, AppVariable.myPassword);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/TOP")]
        public HttpResponseMessage GetTOP(byte flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.TOP(AppVariable.myUserName, AppVariable.myPassword, flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/TradeLastDay")]
        public HttpResponseMessage GetTradeLastDay(byte flow)


        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.TradeLastDay(AppVariable.myUserName, AppVariable.myPassword, flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/TradeLastDayAll")]
        public HttpResponseMessage GetTradeLastDayAll(byte flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.TradeLastDayAll(AppVariable.myUserName, AppVariable.myPassword, flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/TradeOneDay")]
        public HttpResponseMessage GetTradeOneDay(int selDate, byte flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.TradeOneDay(AppVariable.myUserName, AppVariable.myPassword, selDate, flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

        [System.Web.Http.HttpGet]
        [Route("api/TradeOneDayAll")]
        public HttpResponseMessage GetTradeOneDayAll(int selDate, byte flow)
        {
            var tsePublicV2SoapClient = new TsePublicV2SoapClient.TsePublicV2SoapClient();

            DataSet ds = tsePublicV2SoapClient.TradeOneDayAll(AppVariable.myUserName, AppVariable.myPassword, selDate, flow);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);

            if (ds != null)
            {
                var JABanners = DBFuncs.DataTableToJSONArray(ds.Tables[0]);
                string listString = JsonConvert.SerializeObject(JABanners, Newtonsoft.Json.Formatting.Indented);
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(listString, Encoding.UTF8, "application/json");
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent, "No Data found");
            }
            return response;
        }

    }
}
