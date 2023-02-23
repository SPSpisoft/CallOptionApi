using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CallOptionApi_V3.App_Cls
{
    public class AppVariable
    {
        //private const string ServerName = "Data Source=.;";
        //public static string ConStr = ServerName + "Database=SpsCallOption;User ID=db_calloption_user;Password=COSpS79278668;";
        public static string myUserName = "saharkhizland.org";
        public static string myPassword = "Un!v3r5.S@H@rkh!z1@|\\|d";

        private const string ServerName = "Data Source=.;";
        public static string ConStr = ServerName + "Database=SpsCallOption;User ID=db_user;Password=COSpS79278668;";

        public static string Tbl_TradeDayOption = "TradeDayOption";
        public static string Tbl_TradeDayOption_Tmp = "TradeDayOption_Last";
        public static string Tbl_Swings = "TblSwings";

        //-------------------------------------------------------------------------------------------------------
        public static string MyStr_Succes = "Successful",
                             MyStr_ReplaceFail = "Replace fail",
                             MyStr_InvalidValues = "Invalid values",
                             MyStr_InsertFail = "Insert fail",
                             MyStr_DeleteFail = "Delete fail",
                             MyStr_UpdateFail = "Update fail";
    }
}