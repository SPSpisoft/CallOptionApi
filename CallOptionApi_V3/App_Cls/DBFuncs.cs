using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace CallOptionApi_V3.App_Cls
{
    public class DBFuncs
    {

        internal static string SyncDataInsert(PropertyInfo[] propertyList, JObject JValues)
        {
            string mFields = null;
            string mValues = null;

            foreach (PropertyInfo p in propertyList)
            {
                // >>>>>>>>>>>>>> when property is class
                if (p.Name.Equals("Branches") || p.Name.Equals("...") || p.Name.Equals("..."))
                { // Classes
                    if (JValues.GetValue(p.Name).HasValues)
                    {
                        //if (p.Name.Equals("Branches")) BranchesSet(JValues.GetValue(p.Name).ToObject<ClsBranch>());
                        //if (p.Name.Equals("...")) AddressSet(JValues.GetValue(p.Name).ToObject<ClsAddress>());
                    }
                }
                else
                {
                    if (mFields != null) mFields = mFields + ",";
                    if (mValues != null) mValues = mValues + ",";

                    mFields = mFields + p.Name;
                    //var c = JValues.GetValue(p.Name);

                    if (p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?))
                    {
                        //if ((p.Name.Equals("CrtTime") || p.Name.Equals("UpdTime")) && JValues.GetValue(p.Name) == null)
                        //    mValues = mValues + " N'" + DateTime.Now + "'";
                        if (JValues.GetValue(p.Name) == null || String.IsNullOrEmpty(JValues.GetValue(p.Name).ToString()) || JValues.GetValue(p.Name).ToString().Equals(new DateTime().ToString()))
                        {
                            if (p.Name.Equals("CrtTime") || p.Name.Equals("UpdTime"))
                                mValues = mValues + " N'" + DateTime.Now + "'";
                            else if (p.PropertyType == typeof(DateTime?))
                                mValues = mValues + " NULL ";
                            else
                                mValues = mValues + " N'" + DateTime.MinValue + "'";
                        }
                        else
                        {
                            //var vvv = JValues.GetValue(p.Name).ToString();
                            mValues = mValues + " N'" + JValues.GetValue(p.Name).ToString() + "'";
                        }
                    }
                    else if (JValues.GetValue(p.Name) == null)
                        mValues = mValues + " NULL ";
                    //mValues = mValues + DBNull.Value;

                    else if (p.PropertyType == typeof(String))
                    {
                        if (JValues.GetValue(p.Name) != null)
                            mValues = mValues + "N'" + JValues.GetValue(p.Name).ToString() + "'";
                        else
                            mValues = mValues + " NULL ";
                        //mValues = mValues + DBNull.Value;
                    }
                    else if (p.PropertyType == typeof(Decimal) || p.PropertyType == typeof(decimal))
                        mValues = mValues + JValues.GetValue(p.Name).ToString().Replace('/', '.');
                    else if (p.PropertyType == typeof(Boolean) || p.PropertyType == typeof(bool))
                    {
                        if (JValues.GetValue(p.Name) == null || !(bool)JValues.GetValue(p.Name))
                            mValues = mValues + 0;
                        else
                            mValues = mValues + 1;
                    }
                    else
                    {
                        //var bb = p.PropertyType;
                        //var tt = p.PropertyType.GetTypeInfo();

                        mValues = mValues + JValues.GetValue(p.Name).ToString();



                        //Type t = typeof(p);
                        //MethodInfo mi = t.GetMethod(p);
                        //Type retval = mi.ReturnType;
                        //Console.WriteLine("Return value type ... {0}", retval);
                        //Type answer = Nullable.GetUnderlyingType(retval);
                        //Console.WriteLine("Underlying type ..... {0}", answer);

                    }
                }
            }

            return "(" + mFields + ") VALUES (" + mValues + ")";
        }

        internal static string SyncDataSet(PropertyInfo[] propertyList, JObject JValues)
        {
            string mSet = null;

            foreach (PropertyInfo p in propertyList)
            {
                // >>>>>>>>>>>>>> when property is class
                if (p.Name.Equals("Branches") || p.Name.Equals("...") || p.Name.Equals("..."))
                { // Classes
                    if (JValues.GetValue(p.Name).HasValues)
                    {
                        //if (p.Name.Equals("Branches")) BranchesSet(JValues.GetValue(p.Name).ToObject<ClsBranch>());
                        //if (p.Name.Equals("...")) ...(JValues.GetValue(p.Name).ToObject<...>());
                    }
                }
                else
                {
                    if (mSet != null) mSet = mSet + ",";


                    if (p.PropertyType == typeof(DateTime))
                    {
                        if (p.Name.Equals("UpdTime"))
                            mSet = mSet + p.Name + " = N'" + DateTime.Today + "'";
                        else if (JValues.GetValue(p.Name).ToString().Equals(new DateTime().ToString()))
                            mSet = mSet + p.Name + " = NULL";
                        else
                            mSet = mSet + p.Name + " = N'" + JValues.GetValue(p.Name).ToString() + "'";
                    }
                    else
                    {
                        if (p.PropertyType == typeof(Boolean))
                        {
                            if (JValues.GetValue(p.Name) == null || !(bool)JValues.GetValue(p.Name))
                                mSet = mSet + p.Name + " = " + 0;
                            else
                                mSet = mSet + p.Name + " = " + 1;
                        }
                        else if (JValues.GetValue(p.Name) == null)
                            mSet = mSet + p.Name + " = NULL";

                        else if (p.PropertyType == typeof(String) || p.PropertyType == typeof(DateTime))
                            mSet = mSet + p.Name + " = N'" + JValues.GetValue(p.Name).ToString() + "'";
                        else if (p.PropertyType == typeof(Decimal))
                            mSet = mSet + p.Name + " = " + JValues.GetValue(p.Name).ToString().Replace('/', '.');
                        else if (p.PropertyType == typeof(Boolean))
                        {
                            if (JValues.GetValue(p.Name) == null || !(bool)JValues.GetValue(p.Name))
                                mSet = mSet + p.Name + " = " + 0;
                            else
                                mSet = mSet + p.Name + " = " + 1;
                        }
                        else
                            mSet = mSet + p.Name + " = " + JValues.GetValue(p.Name).ToString();
                    }
                }
            }

            return mSet;
        }

        public static DataTable dtChangeNullToEmpty(DataTable dataTable)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (DataColumn col in dataTable.Columns)
                {
                    if (row.IsNull(col) && col.DataType == typeof(string))
                        row.SetField(col, String.Empty);
                }
            }

            return dataTable;
        }

        public static bool TableIsExist(string ConStr, string TableName)
        {
            SqlConnection Conn = new SqlConnection(ConStr);
            string cmdText = @"IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='" + TableName + "') SELECT 1 ELSE SELECT 0";
            Conn.Open();
            SqlCommand TblCheck = new SqlCommand(cmdText, Conn);
            int x = Convert.ToInt32(TblCheck.ExecuteScalar());
            Conn.Close();
            if (x == 1)
                return true;
            else
                return false;
        }

        public static bool UpdateWhere(string ConStr, string TableName, string SetString, string WhereString)
        {
            string strUpdate = "UPDATE " + TableName + " SET " + SetString + " Where " + WhereString;
            var _Connection = new SqlConnection(ConStr);
            SqlCommand query = new SqlCommand(strUpdate, _Connection);
            return RunQuery(_Connection, query);
        }

        public static bool InsertRecord(string ConStr, string TableName, string mInsert)
        {
            String strInsert = "INSERT INTO " + TableName + " " + mInsert;
            var _Connection = new SqlConnection(ConStr);
            SqlCommand query = new SqlCommand(strInsert, _Connection);
            return RunQuery(_Connection, query);
        }

        public static int InsertRecordId(string ConStr, string TableName, string mInsert)
        {
            String strInsert = "INSERT INTO " + TableName + mInsert + ";SELECT CAST(scope_identity() AS int)";
            var _Connection = new SqlConnection(ConStr);
            SqlCommand query = new SqlCommand(strInsert, _Connection);
            _Connection.Open();
            int sQuery = (int)query.ExecuteScalar();
            _Connection.Close();
            return sQuery;
        }

        public static string GetFieldType(string ConStr, string TableName, string mField)
        {
            string ret = null;
            string cmd = "SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + TableName + "' AND COLUMN_NAME = '" + mField + "'";
            SqlConnection _Connection = new SqlConnection(ConStr);
            SqlCommand query = new SqlCommand(cmd, _Connection);
            _Connection.Open();
            SqlDataReader reader = query.ExecuteReader();
            while (reader.Read())
                ret = reader.GetValue(0).ToString();
            _Connection.Close();

            return ret;
        }


        //public class SyncNic_old
        //{
        //    public class DefaultValue { public const string SYS = "DefaultValue_SYS"; }
        //    public enum FType { CHK, INS, UPD }

        //    public string Table { get; set; }
        //    public FType Type { get; set; }
        //    public string Condition { get; set; }
        //    public List<InsRec> Records { get; set; }

        //    public SyncNic_old()
        //    {
        //        this.Records = new List<InsRec>();
        //    }

        //    public InsRec Set(string mName, object mVal)
        //    {
        //        InsRec insRec = new InsRec();
        //        insRec.FldName = mName;
        //        insRec.FldValue = mVal;
        //        //this.Records.Add(insRec);
        //        return insRec;
        //    }

        //    public InsRec Set(string mName, object mVal, object dVal)
        //    {
        //        InsRec insRec = new InsRec();
        //        insRec.FldName = mName;
        //        insRec.FldValue = mVal;
        //        insRec.DefaultValue = dVal;
        //        //this.Records.Add(insRec);
        //        return insRec;
        //    }

        //    public InsRec Set(string mName, object mVal, object dVal, string chkRepeat)
        //    {
        //        InsRec insRec = new InsRec();
        //        insRec.FldName = mName;
        //        insRec.FldValue = mVal;
        //        insRec.DefaultValue = dVal;
        //        insRec.ChkRptCnd = chkRepeat;
        //        //this.Records.Add(insRec);
        //        return insRec;
        //    }

        //    public class InsRec
        //    {
        //        public string ChkRptCnd { get; set; }
        //        public string FldName { get; set; }
        //        public object FldValue { get; set; }
        //        public object DefaultValue { get; set; }
        //    }

        //    public static int UpdateRecord(string ConStr, SyncNic syncNic, string fWhere)
        //    {
        //        string TableName = syncNic.Table;
        //        List<InsRec> iss = syncNic.Records;

        //        List<InsRec> fvList = new List<InsRec>();
        //        foreach (InsRec _Record in iss)
        //        {
        //            InsRec fv = new InsRec();
        //            object value = _Record.FldValue;
        //            if ((value == null && _Record.DefaultValue != null) || (value != null && value.GetType() == typeof(DateTime) && (DateTime)value == DateTime.MinValue))
        //            {
        //                if (_Record.DefaultValue.ToString() == DefaultValue.SYS.ToString())
        //                {
        //                    string fType = GetFieldType(ConStr, TableName, _Record.FldName).ToLower();
        //                    if (fType.Equals("bit")) value = false;
        //                    else if (fType.Equals("datetime")) value = DateTime.Now;
        //                    //... and other type..
        //                }
        //                else
        //                    value = _Record.DefaultValue;
        //            }
        //            fv.ChkRptCnd = "@" + _Record.FldName;
        //            fv.FldName = _Record.FldName + "=" + fv.ChkRptCnd;
        //            fv.FldValue = value;
        //            fvList.Add(fv);

        //            if (!String.IsNullOrEmpty(_Record.ChkRptCnd) && value != null)
        //            {
        //                string strVal = value.ToString();
        //                if (value.GetType() == typeof(string)) strVal = "'" + _Record.FldValue + "'";
        //                if (ChkConditionExist(ConStr, TableName, _Record.FldName + "=" + strVal + " AND " + _Record.ChkRptCnd))
        //                    return -100;
        //            }
        //        }
        //        List<string> lFields = fvList.Select(e => e.FldName).ToList();
        //        string strFields = String.Join(", ", lFields);

        //        String strUpdate = "UPDATE " + TableName + " SET " + strFields + " WHERE " + fWhere + " Select @@ROWCOUNT";
        //        using (SqlConnection mConn = new SqlConnection(ConStr))
        //        {
        //            int _ret = -1;
        //            mConn.Open();
        //            {
        //                SqlCommand cmd = mConn.CreateCommand();
        //                cmd.CommandText = strUpdate;
        //                foreach (InsRec fv in fvList)
        //                    cmd.Parameters.AddWithValue(fv.ChkRptCnd, fv.FldValue);
        //                try
        //                {
        //                    _ret = Convert.ToInt32(cmd.ExecuteScalar());
        //                }
        //                catch (Exception e)
        //                {
        //                    DBFuncs.InsertLog("Catch Update " + e.ToString());
        //                }
        //            }
        //            mConn.Close();
        //            return _ret;
        //        }
        //    }

        //    public static int InsertRecord(string ConStr, SyncNic syncNic)
        //    {
        //        string TableName = syncNic.Table;
        //        List<InsRec> iss = syncNic.Records;

        //        List<InsRec> fvList = new List<InsRec>();
        //        foreach (InsRec _Record in iss)
        //        {
        //            InsRec fv = new InsRec();
        //            object value = _Record.FldValue;
        //            if ((value == null && _Record.DefaultValue != null) || (value != null && value.GetType() == typeof(DateTime) && (DateTime)value == DateTime.MinValue))
        //            {
        //                if (_Record.DefaultValue.ToString() == DefaultValue.SYS.ToString())
        //                {
        //                    string fType = GetFieldType(ConStr, TableName, _Record.FldName).ToLower();
        //                    if (fType.Equals("bit")) value = false;
        //                    else if (fType.Equals("datetime")) value = DateTime.Now;
        //                    //... and other type..
        //                }
        //                else
        //                    value = _Record.DefaultValue;
        //            }
        //            fv.FldName = _Record.FldName;
        //            fv.ChkRptCnd = "@" + _Record.FldName;
        //            fv.FldValue = value;
        //            fvList.Add(fv);

        //            if (!String.IsNullOrEmpty(_Record.ChkRptCnd) && value != null)
        //            {
        //                string strVal = value.ToString();
        //                if (value.GetType() == typeof(string)) strVal = "'" + _Record.FldValue + "'";
        //                if (ChkConditionExist(ConStr, TableName, _Record.FldName + "=" + strVal + " AND " + _Record.ChkRptCnd))
        //                    return -100;
        //            }
        //        }
        //        List<string> lFields = fvList.Select(e => e.FldName).ToList();
        //        List<string> vFields = fvList.Select(e => e.ChkRptCnd).ToList();
        //        string strFields = String.Join(", ", lFields);
        //        string strValues = String.Join(", ", vFields);

        //        String strInsert = "INSERT INTO " + TableName + " " + "(" + strFields + ") VALUES(" + strValues + ");SELECT CAST(scope_identity() AS int)";
        //        using (SqlConnection mConn = new SqlConnection(ConStr))
        //        {
        //            int _ret = -1;
        //            mConn.Open();
        //            {
        //                SqlCommand cmd = mConn.CreateCommand();
        //                cmd.CommandText = strInsert;
        //                foreach (InsRec fv in fvList)
        //                    cmd.Parameters.AddWithValue(fv.ChkRptCnd, fv.FldValue);

        //                try
        //                {
        //                    _ret = Convert.ToInt32(cmd.ExecuteScalar());
        //                }
        //                catch (Exception e)
        //                {
        //                    DBFuncs.InsertLog("Catch Insert " + e.ToString());
        //                }
        //            }

        //            mConn.Close();
        //            return _ret;
        //        }
        //    }

        //    public static int FindRecord(string ConStr, SyncNic syncNic)
        //    {
        //        string TableName = syncNic.Table;
        //        List<InsRec> iss = syncNic.Records;

        //        string setStr = "";
        //        foreach (InsRec _Record in iss)
        //        {

        //            if (!String.IsNullOrEmpty(setStr)) setStr = setStr + " AND ";

        //            string strAdd = "";
        //            if (_Record.FldValue == null)
        //                strAdd = "null";
        //            else
        //            {
        //                if (_Record.FldValue.GetType() == typeof(string))
        //                    strAdd = strAdd + "'" + _Record.FldValue + "'";
        //                else if (_Record.FldValue.GetType() == typeof(bool))
        //                {
        //                    if (_Record.FldValue.Equals(true))
        //                        strAdd = strAdd + 1;
        //                    else
        //                        strAdd = strAdd + 0;
        //                }
        //                else
        //                    strAdd = strAdd + _Record.FldValue;
        //            }
        //            setStr = setStr + _Record.FldName + " = " + strAdd;

        //            if (!String.IsNullOrEmpty(_Record.ChkRptCnd) && ChkConditionExist(ConStr, TableName, _Record.FldName + "=" + strAdd + " AND " + _Record.ChkRptCnd))
        //                return -100;
        //        }

        //        string strSelect = "SELECT COUNT(*) FROM " + TableName + " WHERE " + setStr;
        //        var _Connection = new SqlConnection(ConStr);
        //        SqlCommand query = new SqlCommand(strSelect, _Connection);
        //        _Connection.Open();
        //        int sQuery = (int)query.ExecuteScalar();
        //        _Connection.Close();
        //        return sQuery;
        //    }

        //    public static DataTable GetRecords(string ConStr, SyncNic syncNic)
        //    {
        //        string TableName = syncNic.Table;
        //        List<InsRec> iss = syncNic.Records;

        //        string setStr = "";
        //        foreach (InsRec _Record in iss)
        //        {

        //            if (!String.IsNullOrEmpty(setStr)) setStr = setStr + " AND ";

        //            string strAdd = "";
        //            if (_Record.FldValue == null)
        //                strAdd = "null";
        //            else
        //            {
        //                if (_Record.FldValue.GetType() == typeof(string))
        //                    strAdd = strAdd + "'" + _Record.FldValue + "'";
        //                else if (_Record.FldValue.GetType() == typeof(bool))
        //                {
        //                    if (_Record.FldValue.Equals(true))
        //                        strAdd = strAdd + 1;
        //                    else
        //                        strAdd = strAdd + 0;
        //                }
        //                else
        //                    strAdd = strAdd + _Record.FldValue;
        //            }
        //            setStr = setStr + _Record.FldName + " = " + strAdd;

        //            //if (!String.IsNullOrEmpty(_Record.ChkRptCnd) && ChkConditionExist(ConStr, TableName, _Record.FldName + "=" + strAdd + " AND " + _Record.ChkRptCnd))
        //            //    return -100;
        //        }

        //        string _SqlCommand = "SELECT * FROM " + TableName + " WHERE " + setStr;

        //        DataTable _TableName = new DataTable();

        //        using (SqlConnection con = new SqlConnection(ConStr))
        //        {
        //            using (SqlCommand cmd = new SqlCommand(_SqlCommand))
        //            {
        //                using (SqlDataAdapter sda = new SqlDataAdapter())
        //                {
        //                    cmd.Connection = con;
        //                    sda.SelectCommand = cmd;

        //                    sda.Fill(_TableName);
        //                    con.Close();

        //                    return _TableName;
        //                }
        //            }
        //        }
        //    }

        //    public string GetInsStr(SyncNic iss)
        //    {
        //        string strFields = "";
        //        string strValues = "";
        //        foreach (InsRec _Record in iss.Records)
        //        {
        //            if (!String.IsNullOrEmpty(strFields)) strFields = strFields + ",";
        //            if (!String.IsNullOrEmpty(strValues)) strValues = strValues + ",";

        //            strFields = strFields + _Record.FldName;

        //            if (_Record.FldValue.GetType() == typeof(string))
        //                strValues = strValues + "'" + _Record.FldValue + "'";
        //            else if (_Record.FldValue.GetType() == typeof(bool))
        //            {
        //                if (_Record.FldValue.Equals(true))
        //                    strValues = strValues + 1;
        //                else
        //                    strValues = strValues + 0;
        //            }
        //            else
        //                strValues = strValues + _Record.FldValue;
        //        }

        //        return "(" + strFields + ") VALUES(" + strValues + ")";
        //    }

        //}

        public class SyncNic
        {
            public class DefaultValue { public const string SYS = "DefaultValue_SYS"; }
            public class RptChkCondition { public const string CNull = "1=1"; }
            public enum FType { CHK, INS, UPD }

            public string Table { get; set; }
            public FType Type { get; set; }
            public string Condition { get; set; }
            public List<InsRec> Records { get; set; }

            public SyncNic()
            {
                this.Records = new List<InsRec>();
            }

            public void Set(string mName, object mVal)
            {
                InsRec insRec = new InsRec();
                insRec.FldName = mName;
                insRec.FldValue = mVal;
                this.Records.Add(insRec);
            }

            public void Set(string mName, object mVal, object dVal)
            {
                InsRec insRec = new InsRec();
                insRec.FldName = mName;
                insRec.FldValue = mVal;
                insRec.DefaultValue = dVal;
                this.Records.Add(insRec);
            }

            public void Set(string mName, object mVal, object dVal, string chkRepeat)
            {
                InsRec insRec = new InsRec();
                insRec.FldName = mName;
                insRec.FldValue = mVal;
                insRec.DefaultValue = dVal;
                insRec.ChkRptCnd = chkRepeat;
                this.Records.Add(insRec);
            }

            public class InsRec
            {
                public string ChkRptCnd { get; set; }
                public string FldName { get; set; }
                public FType FldType { get; set; }
                public object FldValue { get; set; }
                public object DefaultValue { get; set; }

                public InsRec()
                {
                }

                public InsRec(string fName, object fVal)
                {
                    this.FldName = fName;
                    this.FldValue = fVal;
                    this.DefaultValue = null;
                    this.ChkRptCnd = null;
                }

                public InsRec(FType fType, string fName, object fVal)
                {
                    this.FldType = fType;
                    this.FldName = fName;
                    this.FldValue = fVal;
                    this.DefaultValue = null;
                    this.ChkRptCnd = null;
                }

                public InsRec(FType fType, string fName, object fVal, object dVal)
                {
                    this.FldType = fType;
                    this.FldName = fName;
                    this.FldValue = fVal;
                    this.DefaultValue = dVal;
                    this.ChkRptCnd = null;
                }

                public InsRec(string fName, object fVal, object dVal)
                {
                    this.FldName = fName;
                    this.FldValue = fVal;
                    this.DefaultValue = dVal;
                    this.ChkRptCnd = null;
                }

                public InsRec(string fName, object fVal, object dVal, string chkRpt)
                {
                    this.FldName = fName;
                    this.FldValue = fVal;
                    this.DefaultValue = dVal;
                    this.ChkRptCnd = chkRpt;
                }
            }

            public static int SetRecord(string ConStr, SyncNic syncNic)
            {
                string TableName = syncNic.Table;
                List<InsRec> iss = syncNic.Records;

                List<InsRec> fvList_Upd = new List<InsRec>();
                List<InsRec> fvList_Chk = new List<InsRec>();
                List<InsRec> fvList_Ins = new List<InsRec>();

                foreach (InsRec _Record in iss)
                {
                    InsRec fv = new InsRec();
                    InsRec fv_ins = new InsRec();
                    object value = _Record.FldValue;
                    if ((value == null && _Record.DefaultValue != null) || (value != null && value.GetType() == typeof(DateTime) && (DateTime)value == DateTime.MinValue))
                    {
                        if (_Record.DefaultValue.ToString() == DefaultValue.SYS.ToString())
                        {
                            string fType = GetFieldType(ConStr, TableName, _Record.FldName).ToLower();
                            if (fType.Equals("bit")) value = false;
                            else if (fType.Equals("datetime")) value = DateTime.Now;
                            else value = DBNull.Value;
                            //... and other type..
                        }
                        else
                            value = _Record.DefaultValue;
                    }
                    fv.ChkRptCnd = "@" + _Record.FldName;
                    fv.FldName = _Record.FldName + "=" + fv.ChkRptCnd;
                    fv.FldValue = value;

                    if (_Record.FldType == FType.CHK)
                        fvList_Chk.Add(fv);
                    else
                        fvList_Upd.Add(fv);

                    fv_ins.FldName = _Record.FldName;
                    fv_ins.ChkRptCnd = "@" + _Record.FldName;
                    fv_ins.FldValue = value;
                    fvList_Ins.Add(fv_ins);


                    if (!String.IsNullOrEmpty(_Record.ChkRptCnd) && value != null)
                    {
                        string strVal = value.ToString();
                        if (value.GetType() == typeof(string)) strVal = "'" + _Record.FldValue + "'";
                        if (ChkConditionExist(ConStr, TableName, _Record.FldName + "=" + strVal + " AND " + _Record.ChkRptCnd))
                            return -100;
                    }
                }
                List<string> lFields = fvList_Upd.Select(e => e.FldName).ToList();
                string strFields = String.Join(", ", lFields);

                List<string> lFields_Chk = fvList_Chk.Select(e => e.FldName).ToList();
                string strFields_Chk = String.Join(" AND ", lFields_Chk);

                List<string> lFields_INS = fvList_Ins.Select(e => e.FldName).ToList();
                List<string> vFields_INS = fvList_Ins.Select(e => e.ChkRptCnd).ToList();
                string strFields_INS = String.Join(", ", lFields_INS);
                string strValues_INS = String.Join(", ", vFields_INS);

                string strSelect = "SELECT COUNT(*) FROM " + TableName + " WHERE " + strFields_Chk;
                int _retFind = 0;

                using (SqlConnection mConn = new SqlConnection(ConStr))
                {
                    mConn.Open();
                    {
                        SqlCommand cmd = mConn.CreateCommand();
                        cmd.CommandText = strSelect;
                        foreach (InsRec fv in fvList_Chk)
                            cmd.Parameters.AddWithValue(fv.ChkRptCnd, fv.FldValue);

                        _retFind = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    mConn.Close();
                }

                if (_retFind > 0)
                {
                    String strUpdate = "UPDATE " + TableName + " SET " + strFields + " WHERE " + strFields_Chk + " Select @@ROWCOUNT";
                    using (SqlConnection mConn = new SqlConnection(ConStr))
                    {
                        int _ret = -1;
                        mConn.Open();
                        {
                            SqlCommand cmd = mConn.CreateCommand();
                            cmd.CommandText = strUpdate;
                            foreach (InsRec fv in fvList_Upd)
                                cmd.Parameters.AddWithValue(fv.ChkRptCnd, fv.FldValue);

                            foreach (InsRec fv in fvList_Chk)
                                cmd.Parameters.AddWithValue(fv.ChkRptCnd, fv.FldValue);

                            _ret = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        mConn.Close();
                        return _ret;
                    }
                }
                else
                {
                    String strInsert = "INSERT INTO " + TableName + " " + "(" + strFields_INS + ") VALUES(" + strValues_INS + ");SELECT CAST(scope_identity() AS int)";
                    using (SqlConnection mConn = new SqlConnection(ConStr))
                    {
                        int _ret = -1;
                        mConn.Open();
                        {
                            SqlCommand cmd = mConn.CreateCommand();
                            cmd.CommandText = strInsert;
                            foreach (InsRec fv in fvList_Ins)
                                cmd.Parameters.AddWithValue(fv.ChkRptCnd, fv.FldValue);

                            _ret = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        mConn.Close();
                        return _ret;
                    }
                }
            }

            public static int UpdateRecord(string ConStr, SyncNic syncNic)
            {
                string TableName = syncNic.Table;
                List<InsRec> iss = syncNic.Records;

                List<InsRec> fvList_Upd = new List<InsRec>();
                List<InsRec> fvList_Chk = new List<InsRec>();
                //List<InsRec> fvList_Ins = new List<InsRec>();

                foreach (InsRec _Record in iss)
                {
                    InsRec fv = new InsRec();
                    InsRec fv_ins = new InsRec();
                    object value = _Record.FldValue;
                    if ((value == null && _Record.DefaultValue != null) || (value != null && value.GetType() == typeof(DateTime) && (DateTime)value == DateTime.MinValue))
                    {
                        if (_Record.DefaultValue.ToString() == DefaultValue.SYS.ToString())
                        {
                            string fType = GetFieldType(ConStr, TableName, _Record.FldName).ToLower();
                            if (fType.Equals("bit")) value = false;
                            else if (fType.Equals("datetime"))
                                value = DateTime.Now;
                            else value = DBNull.Value;
                            //... and other type..
                        }
                        else
                            value = _Record.DefaultValue;
                    }
                    fv.ChkRptCnd = "@" + _Record.FldName;
                    fv.FldName = _Record.FldName + "=" + fv.ChkRptCnd;
                    fv.FldValue = value;

                    if (_Record.FldType == FType.CHK)
                        fvList_Chk.Add(fv);
                    else
                        fvList_Upd.Add(fv);

                    fv_ins.FldName = _Record.FldName;
                    fv_ins.ChkRptCnd = "@" + _Record.FldName;
                    fv_ins.FldValue = value;
                    //fvList_Ins.Add(fv_ins);


                    if (!String.IsNullOrEmpty(_Record.ChkRptCnd) && value != null)
                    {
                        string strVal = value.ToString();
                        if (value.GetType() == typeof(string)) strVal = "'" + _Record.FldValue + "'";
                        if (ChkConditionExist(ConStr, TableName, _Record.FldName + "=" + strVal + " AND " + _Record.ChkRptCnd))
                            return -100;
                    }
                }
                List<string> lFields = fvList_Upd.Select(e => e.FldName).ToList();
                string strFields = String.Join(", ", lFields);

                List<string> lFields_Chk = fvList_Chk.Select(e => e.FldName).ToList();
                string strFields_Chk = String.Join(" AND ", lFields_Chk);

                //List<string> lFields_INS = fvList_Ins.Select(e => e.FldName).ToList();
                //List<string> vFields_INS = fvList_Ins.Select(e => e.ChkRptCnd).ToList();
                //string strFields_INS = String.Join(", ", lFields_INS);
                //string strValues_INS = String.Join(", ", vFields_INS);

                string strSelect = "SELECT COUNT(*) FROM " + TableName + " WHERE " + strFields_Chk;
                int _retFind = 0;

                using (SqlConnection mConn = new SqlConnection(ConStr))
                {
                    mConn.Open();
                    {
                        SqlCommand cmd = mConn.CreateCommand();
                        cmd.CommandText = strSelect;
                        foreach (InsRec fv in fvList_Chk)
                            cmd.Parameters.AddWithValue(fv.ChkRptCnd, fv.FldValue);

                        _retFind = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    mConn.Close();
                }

                if (_retFind > 0)
                {
                    String strUpdate = "UPDATE " + TableName + " SET " + strFields + " WHERE " + strFields_Chk + " Select @@ROWCOUNT";
                    using (SqlConnection mConn = new SqlConnection(ConStr))
                    {
                        int _ret = -1;
                        mConn.Open();
                        {
                            SqlCommand cmd = mConn.CreateCommand();
                            cmd.CommandText = strUpdate;
                            foreach (InsRec fv in fvList_Upd)
                                cmd.Parameters.AddWithValue(fv.ChkRptCnd, fv.FldValue);

                            foreach (InsRec fv in fvList_Chk)
                                cmd.Parameters.AddWithValue(fv.ChkRptCnd, fv.FldValue);

                            _ret = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        mConn.Close();
                        return _ret;
                    }
                }
                else
                {
                    return -9;
                }
            }

            public static int UpdateRecord(string ConStr, SyncNic syncNic, string fWhere)
            {
                string TableName = syncNic.Table;
                List<InsRec> iss = syncNic.Records;

                List<InsRec> fvList = new List<InsRec>();
                foreach (InsRec _Record in iss)
                {
                    InsRec fv = new InsRec();
                    object value = _Record.FldValue;
                    if ((value == null && _Record.DefaultValue != null) || (value != null && value.GetType() == typeof(DateTime) && (DateTime)value == DateTime.MinValue))
                    {
                        if (_Record.DefaultValue.ToString() == DefaultValue.SYS.ToString())
                        {
                            string fType = GetFieldType(ConStr, TableName, _Record.FldName).ToLower();
                            if (fType.Equals("bit")) value = false;
                            else if (fType.Equals("datetime")) value = DateTime.Now;
                            else value = DBNull.Value;
                            //... and other type..
                        }
                        else
                            value = _Record.DefaultValue;
                    }
                    fv.ChkRptCnd = "@" + _Record.FldName;
                    fv.FldName = _Record.FldName + "=" + fv.ChkRptCnd;
                    fv.FldValue = value;
                    fvList.Add(fv);

                    if (!String.IsNullOrEmpty(_Record.ChkRptCnd) && value != null)
                    {
                        string strVal = value.ToString();
                        if (value.GetType() == typeof(string)) strVal = "'" + _Record.FldValue + "'";
                        if (ChkConditionExist(ConStr, TableName, _Record.FldName + "=" + strVal + " AND " + _Record.ChkRptCnd))
                            return -100;
                    }
                }
                List<string> lFields = fvList.Select(e => e.FldName).ToList();
                string strFields = String.Join(", ", lFields);

                String strUpdate = "UPDATE " + TableName + " SET " + strFields + " WHERE " + fWhere + " Select @@ROWCOUNT";
                using (SqlConnection mConn = new SqlConnection(ConStr))
                {
                    int _ret = -1;
                    mConn.Open();
                    {
                        SqlCommand cmd = mConn.CreateCommand();
                        cmd.CommandText = strUpdate;
                        foreach (InsRec fv in fvList)
                            cmd.Parameters.AddWithValue(fv.ChkRptCnd, fv.FldValue);

                        _ret = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    mConn.Close();
                    return _ret;
                }
            }

            public static int InsertRecord(string ConStr, SyncNic syncNic)
            {
                string TableName = syncNic.Table;
                List<InsRec> iss = syncNic.Records;

                List<InsRec> fvList = new List<InsRec>();
                foreach (InsRec _Record in iss)
                {
                    InsRec fv = new InsRec();
                    object value = _Record.FldValue;
                    if ((value == null && _Record.DefaultValue != null) || (value != null && value.GetType() == typeof(DateTime) && (DateTime)value == DateTime.MinValue))
                    {
                        if (_Record.DefaultValue.ToString() == DefaultValue.SYS.ToString())
                        {
                            string fType = GetFieldType(ConStr, TableName, _Record.FldName).ToLower();
                            if (fType.Equals("bit")) value = false;
                            else if (fType.Equals("datetime")) value = DateTime.Now;
                            else value = DBNull.Value;
                            //... and other type..
                        }
                        else
                            value = _Record.DefaultValue;
                    }
                    fv.FldName = _Record.FldName;
                    fv.ChkRptCnd = "@" + _Record.FldName;
                    fv.FldValue = value;
                    fvList.Add(fv);

                    if (!String.IsNullOrEmpty(_Record.ChkRptCnd) && value != null)
                    {
                        string strVal = value.ToString();
                        if (value.GetType() == typeof(string)) strVal = "'" + _Record.FldValue + "'";
                        if (ChkConditionExist(ConStr, TableName, _Record.FldName + "=" + strVal + " AND " + _Record.ChkRptCnd))
                            return -100;
                    }
                }
                List<string> lFields = fvList.Select(e => e.FldName).ToList();
                List<string> vFields = fvList.Select(e => e.ChkRptCnd).ToList();
                string strFields = String.Join(" , ", lFields);
                string strValues = String.Join(" , ", vFields);

                String strInsert = "INSERT INTO " + TableName + " " + "(" + strFields + ") VALUES(" + strValues + ");SELECT CAST(scope_identity() AS int)";
                using (SqlConnection mConn = new SqlConnection(ConStr))
                {
                    int _ret = -1;
                    mConn.Open();
                    {
                        SqlCommand cmd = mConn.CreateCommand();
                        cmd.CommandText = strInsert;
                        cmd.Prepare();
                        foreach (InsRec fv in fvList)
                        {
                            cmd.Parameters.AddWithValue(fv.ChkRptCnd, fv.FldValue);
                        }

                        _ret = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    mConn.Close();
                    return _ret;
                }
            }

            public static int FindRecord(string ConStr, SyncNic syncNic)
            {
                string TableName = syncNic.Table;
                List<InsRec> iss = syncNic.Records;

                string setStr = "";
                foreach (InsRec _Record in iss)
                {

                    if (!String.IsNullOrEmpty(setStr)) setStr = setStr + " AND ";

                    string strAdd = "";
                    if (_Record.FldValue == null)
                        strAdd = "null";
                    else
                    {
                        if (_Record.FldValue.GetType() == typeof(string))
                            strAdd = strAdd + "'" + _Record.FldValue + "'";
                        else if (_Record.FldValue.GetType() == typeof(bool))
                        {
                            if (_Record.FldValue.Equals(true))
                                strAdd = strAdd + 1;
                            else
                                strAdd = strAdd + 0;
                        }
                        else
                            strAdd = strAdd + _Record.FldValue;
                    }
                    setStr = setStr + _Record.FldName + " = " + strAdd;

                    if (!String.IsNullOrEmpty(_Record.ChkRptCnd) && ChkConditionExist(ConStr, TableName, _Record.FldName + "=" + strAdd + " AND " + _Record.ChkRptCnd))
                        return -100;
                }

                string strSelect = "SELECT COUNT(*) FROM " + TableName + " WHERE " + setStr;
                var _Connection = new SqlConnection(ConStr);
                SqlCommand query = new SqlCommand(strSelect, _Connection);
                _Connection.Open();
                int sQuery = (int)query.ExecuteScalar();
                _Connection.Close();
                return sQuery;
            }

            public static DataTable GetRecords(string ConStr, SyncNic syncNic)
            {
                string TableName = syncNic.Table;
                List<InsRec> iss = syncNic.Records;

                string setStr = "";
                foreach (InsRec _Record in iss)
                {

                    if (!String.IsNullOrEmpty(setStr)) setStr = setStr + " AND ";

                    string strAdd = "";
                    if (_Record.FldValue == null)
                        strAdd = "null";
                    else
                    {
                        if (_Record.FldValue.GetType() == typeof(string))
                            strAdd = strAdd + "'" + _Record.FldValue + "'";
                        else if (_Record.FldValue.GetType() == typeof(bool))
                        {
                            if (_Record.FldValue.Equals(true))
                                strAdd = strAdd + 1;
                            else
                                strAdd = strAdd + 0;
                        }
                        else
                            strAdd = strAdd + _Record.FldValue;
                    }
                    setStr = setStr + _Record.FldName + " = " + strAdd;

                    //if (!String.IsNullOrEmpty(_Record.ChkRptCnd) && ChkConditionExist(ConStr, TableName, _Record.FldName + "=" + strAdd + " AND " + _Record.ChkRptCnd))
                    //    return -100;
                }

                string _SqlCommand = "SELECT * FROM " + TableName + " WHERE " + setStr;

                DataTable _TableName = new DataTable();

                using (SqlConnection con = new SqlConnection(ConStr))
                {
                    using (SqlCommand cmd = new SqlCommand(_SqlCommand))
                    {
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            cmd.Connection = con;
                            sda.SelectCommand = cmd;

                            sda.Fill(_TableName);
                            con.Close();

                            return _TableName;
                        }
                    }
                }
            }

            public string GetInsStr(SyncNic iss)
            {
                string strFields = "";
                string strValues = "";
                foreach (InsRec _Record in iss.Records)
                {
                    if (!String.IsNullOrEmpty(strFields)) strFields = strFields + ",";
                    if (!String.IsNullOrEmpty(strValues)) strValues = strValues + ",";

                    strFields = strFields + _Record.FldName;

                    if (_Record.FldValue.GetType() == typeof(string))
                        strValues = strValues + "'" + _Record.FldValue + "'";
                    else if (_Record.FldValue.GetType() == typeof(bool))
                    {
                        if (_Record.FldValue.Equals(true))
                            strValues = strValues + 1;
                        else
                            strValues = strValues + 0;
                    }
                    else
                        strValues = strValues + _Record.FldValue;
                }

                return "(" + strFields + ") VALUES(" + strValues + ")";
            }

        }

        public static bool DeleteRecord(string ConStr, string TableName, string mWhere)
        {
            String strDelete = "DELETE FROM " + TableName + " WHERE " + mWhere;
            var _Connection = new SqlConnection(ConStr);
            SqlCommand query = new SqlCommand(strDelete, _Connection);
            return RunQuery(_Connection, query);
        }

        public static bool DeleteRecId(string ConStr, string TableName, int iID)
        {
            String strDelete = "DELETE FROM " + TableName + " WHERE NetRec = " + iID;
            var _Connection = new SqlConnection(ConStr);
            SqlCommand query = new SqlCommand(strDelete, _Connection);
            return RunQuery(_Connection, query);
        }

        public static DataTable GetRecords(string ConStr, string TableName, string mFields, string mCondition, string mOrder)
        {
            DataTable _Tbl = new DataTable();
            string sFields = "*";
            if (!String.IsNullOrEmpty(mFields))
                sFields = mFields;
            string sCondition = "";
            if (!String.IsNullOrEmpty(mCondition))
                sCondition = " WHERE " + mCondition;
            string sOrder = "";
            if (!String.IsNullOrEmpty(mOrder))
                sOrder = " ORDER BY " + mOrder;
            String strInsert = "SELECT " + sFields + " FROM " + TableName + sCondition + sOrder;
            SqlConnection con = new SqlConnection(ConStr);
            SqlCommand cmd = new SqlCommand(strInsert, con);
            con.Open();
            using (SqlDataAdapter sda = new SqlDataAdapter())
            {
                cmd.Connection = con;
                sda.SelectCommand = cmd;
                sda.Fill(_Tbl);
                con.Close();
                return dtChangeNullToEmpty(_Tbl);
            }
        }

        public string GetStrFromTDB(string ConStr, string TableName, string mFields, string mCondition)
        {
            DataTable mDataTable = GetRecords(ConStr, TableName, mFields, mCondition, null);
            if (mDataTable.Rows.Count > 0)
                return mDataTable.Rows[0].Field<string>(mFields);
            else
                return null;
        }

        internal static decimal? GetDecimalFromTDB(string ConStr, string TableName, string mFields, string mCondition)
        {
            DataTable mDataTable = GetRecords(ConStr, TableName, mFields, mCondition, null);
            if (mDataTable.Rows.Count > 0)
                return mDataTable.Rows[0].Field<decimal>(mFields);
            else
                return null;
        }

        internal static List<double> GetFieldList_Double(string ConStr, string TableName, string mFields, string mCondition, string mOrder)
        {
            List<double> ret = new List<double>();

            DataTable mDataTable = GetRecords(ConStr, TableName, mFields, mCondition, mOrder);
            if (mDataTable.Rows.Count > 0)
            {
                foreach(DataRow dr in mDataTable.Rows)
                {
                    ret.Add(dr.Field<double>(mFields));
                }
            }

            return ret;
        }

        internal static List<double> GetFieldList_DecimalAsDouble(string ConStr, string TableName, string mFields, string mCondition, string mOrder)
        {
            List<double> ret = new List<double>();
            List<int> retInt = new List<int>();

            DataTable mDataTable = GetRecords(ConStr, TableName, null, mCondition, mOrder);


            if (mDataTable.Rows.Count > 0)
            {
                foreach (DataRow dr in mDataTable.Rows)
                {
                    ret.Add(Convert.ToDouble(dr.Field<decimal>(mFields)));
                    retInt.Add(dr.Field<int>("DEven"));
                }
            }

            var v66 = retInt.AsEnumerable().Take(22);

            return ret;
        }


        //????????????? Edit complete
        public static void InsertLog(SqlConnection _Connection, string sLOg, Exception exception)
        {
            if (exception != null)
            {
                //// Get stack trace for the exception with source file information
                //var st = new StackTrace(exception, true);
                //// Get the top stack frame
                //var frame = st.GetFrame(0);
                //// Get the line number from the stack frame
                //var line = frame.GetFileLineNumber();

                StackTrace trace = new StackTrace(exception, true);
                StackFrame stackFrame = trace.GetFrame(trace.FrameCount - 1);
                string fileName = stackFrame.GetFileName();
                string methodName = stackFrame.GetMethod().Name;
                int lineNo = stackFrame.GetFileLineNumber();

                sLOg = "EXP:: " + fileName + "\\n: " + methodName + "\\n -line: " + lineNo + " \\n >>> " + sLOg;
            }

            string sql = "INSERT INTO LogList (LogTime, LogStr) VALUES (@LogTime, @LogStr)";
            using (SqlConnection mConn = _Connection)
            {
                mConn.Open();
                {
                    SqlCommand cmd = mConn.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@LogStr", sLOg);
                    cmd.Parameters.AddWithValue("@LogTime", DateTime.Now);
                    //_ret = (int)cmd.ExecuteScalar();
                    cmd.ExecuteNonQuery();
                }
                mConn.Close();
            }
        }

        private static bool RunQuery(SqlConnection _Connection, SqlCommand query)
        {
            int sQuery = -1;
            try
            {
                _Connection.Open();
                sQuery = query.ExecuteNonQuery();
                _Connection.Close();
            }
            catch (Exception e)
            {
                _Connection.Close();
                InsertLog(_Connection, "RunQuery " + query + " >>>> " + e.ToString(), e);
                //MessageBox.Show(ex.Message);
            }

            if (sQuery > 0)
                return true;
            else
            {
                _Connection.Close();
                InsertLog(_Connection, "RunQuery_Null " + query.ToString(), null);
                return false;
            }
        }

        public static bool FindRecord(string ConStr, string TableName, string mCondition)
        {
            bool ret = false;
            String strInsert = "SELECT COUNT(*) FROM " + TableName + " WHERE " + mCondition;
            var _Connection = new SqlConnection(ConStr);
            SqlCommand query = new SqlCommand(strInsert, _Connection);
            _Connection.Open();
            int count = Convert.ToInt32(query.ExecuteScalar());
            //int count = (int)query.ExecuteScalar();
            _Connection.Close();
            if (count > 0)
                ret = true;
            return ret;
        }

        public static string QeryGetStr(string ConStr, string TableName, string mCondition, string mField, string defaultValue)
        {
            DataRow dr = GetRecord(ConStr, TableName, mCondition, false);
            if (dr != null)
            {
                string ret = dr.Field<string>(mField);
                if (!String.IsNullOrEmpty(ret)) return ret;
            }
            return defaultValue;
        }

        //internal static void QueryGetInt(string conStr, string tbl_Store, string v1, int v2)
        //{
        //    throw new NotImplementedException();
        //}
        /// <summary>
        /// Get Int Value Field
        /// </summary>
        /// <param name="ConStr"></param>
        /// <param name="TableName"></param>
        /// <param name="mCondition"></param>
        /// <param name="mField"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        internal static int QueryGetInt(string ConStr, string TableName, string mCondition, string mField, int defaultValue)
        {
            int ret = defaultValue;
            string sql = "Select " + mField + " From " + TableName + " where " + mCondition;
            SqlConnection con = new SqlConnection(ConStr);
            SqlCommand cmd = new SqlCommand(sql, con);
            con.Open();
            SqlDataReader rd = cmd.ExecuteReader();
            if (rd.HasRows)
            {
                rd.Read();
                ret = rd.GetInt32(0);
            }
            con.Close();
            return ret;
        }

        /// <summary>
        /// Get single Records with condition
        /// </summary>
        /// <param name="ConStr"></param>
        /// <param name="TableName"></param>
        /// <param name="mCondition"></param>
        /// <returns></returns>
        internal static DataRow GetRecord(string ConStr, string TableName, string mCondition, bool toEmpty)
        {
            DataTable _Tbl = new DataTable();
            String strSelect = "SELECT * FROM " + TableName + " WHERE " + mCondition;
            SqlConnection con = new SqlConnection(ConStr);
            SqlCommand cmd = new SqlCommand(strSelect, con);
            con.Open();
            using (SqlDataAdapter sda = new SqlDataAdapter())
            {
                cmd.Connection = con;
                sda.SelectCommand = cmd;
                sda.Fill(_Tbl);
                con.Close();
                if (_Tbl == null || _Tbl.Rows.Count == 0)
                    return null;
                else
                {
                    if (toEmpty)
                        return dtChangeNullToEmpty(_Tbl).Rows[0];
                    else
                        return _Tbl.Rows[0];
                }
            }
        }

        public static int GetCountRecord(string ConStr, string TableName, string mCondition)
        {
            int ret = -1;
            String sql = "SELECT COUNT(*) FROM " + TableName;
            if (!String.IsNullOrEmpty(mCondition)) sql = sql + " WHERE " + mCondition;
            SqlConnection con = new SqlConnection(ConStr);
            SqlCommand comd = new SqlCommand(sql, con);
            con.Open();
            ret = (int)(comd.ExecuteScalar());
            con.Close();
            return ret;
        }

        public static bool ChkConditionExist(string ConStr, string TableName, string Condition)
        {
            bool _ret = false;

            string strSelect = "SELECT * FROM " + TableName + " Where " + Condition;

            var _Connection = new SqlConnection(ConStr);
            var _DataAdapter = new SqlDataAdapter(strSelect, _Connection);
            var _DataTable = new DataTable();

            _DataAdapter.Fill(_DataTable);
            int _CountRow = _DataTable.Rows.Count;

            if (_CountRow > 0)
            {
                _ret = true;
            }
            return _ret;
        }

        public string ImageToStr(string mPath)
        {
            using (Image image = Image.FromFile(mPath))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
        }

        public string ImageFieldStr(Image image)
        {
            using (MemoryStream m = new MemoryStream())
            {
                image.Save(m, image.RawFormat);
                byte[] imageBytes = m.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        public static DataTable GetRecord(string ConStr, string TableName, string mCondition)
        {
            DataTable _Tbl = new DataTable();
            String strInsert = "SELECT * FROM " + TableName + " WHERE " + mCondition;
            SqlConnection con = new SqlConnection(ConStr);
            SqlCommand cmd = new SqlCommand(strInsert, con);
            con.Open();
            using (SqlDataAdapter sda = new SqlDataAdapter())
            {
                cmd.Connection = con;
                sda.SelectCommand = cmd;
                sda.Fill(_Tbl);
                con.Close();
                return dtChangeNullToEmpty(_Tbl);
            }
        }

        /// <summary>
        /// GetDataTableQuery
        /// </summary>
        /// <param name="_ConStr"></param>
        /// <param name="_SqlCommand"></param>
        /// <returns></returns>
        public static DataTable GetDataTableQuery(string _ConStr, string _SqlCommand, bool _SetNull2Empty)
        {
            DataTable _TableName = new DataTable();

            using (SqlConnection con = new SqlConnection(_ConStr))
            {
                using (SqlCommand cmd = new SqlCommand(_SqlCommand))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;

                        sda.Fill(_TableName);
                        con.Close();

                        if (_SetNull2Empty)
                            return dtChangeNullToEmpty(_TableName);
                        else
                            return _TableName;
                    }
                }
            }
        }

        public static byte[] Resize2Max50Kbytes(byte[] byteImageIn)
        {
            byte[] currentByteImageArray = byteImageIn;
            double scale = 1f;

            //if (!IsValidImage(byteImageIn))
            //{
            //    return null;
            //}

            MemoryStream inputMemoryStream = new MemoryStream(byteImageIn);
            Image fullsizeImage = Image.FromStream(inputMemoryStream);

            while (currentByteImageArray.Length > 50000)
            {
                Bitmap fullSizeBitmap = new Bitmap(fullsizeImage, new Size((int)(fullsizeImage.Width * scale), (int)(fullsizeImage.Height * scale)));
                MemoryStream resultStream = new MemoryStream();

                fullSizeBitmap.Save(resultStream, fullsizeImage.RawFormat);

                currentByteImageArray = resultStream.ToArray();
                resultStream.Dispose();
                resultStream.Close();

                scale -= 0.05f;
            }

            return currentByteImageArray;
        }


        /// <summary>
        /// Return data table from a string query
        /// </summary>
        /// <param name="_ConStr"></param>
        /// <param name="_Query"></param>
        /// <returns></returns>
        public static DataTable FuncGetDT(string _ConStr, string _Query, bool _OldSql)
        {
            DataTable _Tbl = new DataTable();
            using (SqlConnection con = new SqlConnection(_ConStr))
            {
                using (SqlCommand cmd = new SqlCommand(_Query))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter())
                    {
                        cmd.Connection = con;
                        sda.SelectCommand = cmd;
                        sda.Fill(_Tbl);
                        con.Close();
                        return dtChangeNullToEmpty(_Tbl);
                    }
                }
            }
        }

        /// <summary>
        /// nested array class to JARRAY
        /// </summary>
        /// <typeparam name="TC"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static JArray ListToJSONArray<TC>(List<TC> table)
        {
            if (table == null)
                return null;
            else
            {
                JArray jArray = new JArray();
                for (int i = 0; i < table.Count; i++)
                {
                    var Dt = table[i];

                    String json = JsonConvert.SerializeObject(Dt, Formatting.Indented);
                    int len = json.Length;
                    jArray.Add(JObject.Parse(json));
                }

                return jArray;
            }
        }

        /// <summary>
        /// nested class to JObject
        /// </summary>
        /// <typeparam name="TC"></typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static JObject ClassToJSONObject<TC>(TC table)
        {
            if (table == null)
                return null;
            else
            {
                //JArray jArray = new JArray();
                //for (int i = 0; i < table.Count; i++)
                //{
                var Dt = table;

                String json = JsonConvert.SerializeObject(Dt, Formatting.Indented);
                int len = json.Length;
                return JObject.Parse(json);
                //}

                //return jArray;
            }
        }

        /// <summary>
        /// DataTable to List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        public static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                    {
                        //if ((pro.PropertyType == typeof(DateTime) || pro.PropertyType == typeof(DateTime?)) && !dr.IsNull(column.ColumnName))
                        //{
                        //    // yyyy - MM - dd HH: mm: ss
                        //    //      CultureInfo culture = new CultureInfo("de-DE");
                        //    //DateTime mm = Convert.ToDateTime(dr[column.ColumnName], culture);
                        //    //try
                        //    //{
                        //    //    DateTime convertedDate = DateTime.ParseExact(dr[column.ColumnName].ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        //    //    //convertedDate.ToString("MM/dd/yyyy HH:mm:ss")
                        //    //    //String mms = mm.ToShortTimeString();
                        //    //    pro.SetValue(obj, convertedDate, null);
                        //    //}catch(Exception e)

                        //    //convertedDate.ToString("MM/dd/yyyy HH:mm:ss")
                        //    //String mms = mm.ToShortTimeString();

                        //    //DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        //    DateTime mDate = (DateTime)dr[column.ColumnName];
                        //    //CultureInfo culture = new CultureInfo("en-US");
                        //    DateTime mm = Convert.ToDateTime(mDate.ToString("yyyy-MM-dd HH:mm"));

                        //    pro.SetValue(obj, mm, null);
                        //}
                        //else
                        {
                            var v = pro.GetType();
                            //    pro.SetValue(obj, dr[column.ColumnName], null);
                            //pro.SetValue(objT, row[pro.Name], null);
                            pro.SetValue(obj, dr.IsNull(column.ColumnName) ? null : dr[column.ColumnName], null);
                        }
                    }
                    else
                        continue;
                }
            }
            return obj;
        }

        /// <summary>
        /// Convert DataTable to JArray
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static JArray DataTableToJSONArray(DataTable table)
        {
            if (table == null)
                return null;
            else
            {
                JArray jArray = new JArray();
                foreach (DataRow _RevStoreRow in table.Rows)
                {
                    DataTable Dt = table.Clone();
                    Dt.Rows.Add(_RevStoreRow.ItemArray);
                    String json = JsonConvert.SerializeObject(Dt, Newtonsoft.Json.Formatting.Indented);
                    int len = json.Length;
                    jArray.Add(JObject.Parse(json.Substring(1, len - 2)));
                }

                return jArray;
            }
        }

        /// <summary>
        /// DataRow to JObject
        /// </summary>
        /// <param name="table"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static JObject DataRowToJSONObject(DataTable table, DataRow row, Boolean CoverNulls)
        {
            DataTable Dt = table.Clone();
            if (row == null)
                Dt.Rows.Add();
            else
                Dt.Rows.Add(row.ItemArray);
            if (CoverNulls)
            {
                Dt = dtChangeNullToEmpty(Dt);
                Dt = dtChangeNullToZero(Dt);
                Dt = dtChangeNullToFalse(Dt);
            }

            String json = JsonConvert.SerializeObject(Dt, Formatting.Indented);

            int len = json.Length;
            return JObject.Parse(json.Substring(1, len - 2));
        }

        public static DataTable dtChangeEmptyTpNull(DataTable dataTable)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (DataColumn col in dataTable.Columns)
                {
                    if (!row.IsNull(col) && col.DataType == typeof(string) && row.Field<string>(col).Equals(""))
                        row.SetField(col, DBNull.Value);
                }
            }

            return dataTable;
        }

        /// <summary>
        /// Convert Null String int/decimal if DataTable to Empty values
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static DataTable dtChangeNullToZero(DataTable dataTable)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (DataColumn col in dataTable.Columns)
                {
                    if (row.IsNull(col) && (col.DataType == typeof(decimal) || col.DataType == typeof(int)))
                    {
                        row.SetField(col, 0);
                    }
                }
            }

            return dataTable;
        }

        /// <summary>
        /// Convert Null String int if DataTable to Empty values
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static DataTable dtChangeNullToFalse(DataTable dataTable)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (DataColumn col in dataTable.Columns)
                {
                    if (row.IsNull(col) && (col.DataType == typeof(bool)))
                    {
                        row.SetField(col, 0);
                    }
                }
            }

            return dataTable;
        }

        internal static string CheckStringDate(string sdate)
        {
            string ret = null;
            if (sdate.Length == 8)
            {
                try
                {
                    int Y = Int32.Parse(sdate.Substring(0, 4));
                    int M = Int32.Parse(sdate.Substring(4, 2));
                    int D = Int32.Parse(sdate.Substring(6, 2));
                    if (Y > 1800)
                    {
                        if ((M == 1 || M == 3 || M == 5 || M == 7 || M == 8 || M == 10 || M == 12) && D <= 31)
                            ret = "M";
                        if (M == 2 && D <= 29)
                            ret = "M";
                        if ((M == 4 || M == 6 || M == 9 || M == 11) && D <= 30)
                            ret = "M";
                    }
                    else
                    {
                        if (M <= 6 && D <= 31)
                            ret = "S";
                        if (M > 6 && M <= 12 && D <= 30)
                            ret = "S";
                    }
                }
                catch { }
            }
            return ret;
        }

        // stackoverflow.com/a/3294698/162671
        public static DateTime GetNetworkTime()
        {
            //default Windows time server
            const string ntpServer = "time.windows.com";

            // NTP message size - 16 bytes of the digest (RFC 2030)
            var ntpData = new byte[48];

            //Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            var addresses = Dns.GetHostEntry(ntpServer).AddressList;

            //The UDP port number assigned to NTP is 123
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            //NTP uses UDP

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(ipEndPoint);

                //Stops code hang if NTP is blocked
                socket.ReceiveTimeout = 3000;

                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();
            }

            //Offset to get to the "Transmit Timestamp" field (time at which the reply 
            //departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;

            //Get the seconds part
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

            //Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

            //Convert From big-endian to little-endian
            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            //**UTC** time
            var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

            return networkDateTime.ToLocalTime();
        }

        internal static int ConvertInvoiceDate(DateTime dt)
        {
            return int.Parse(dt.ToString("yyyyMMdd"));
        }

        public static async System.Threading.Tasks.Task<string> GetDateTime()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("https://api.keybit.ir/time/");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        public static async Task<String> GetAsync(string uri)
        {
            var httpClient = new HttpClient();

            //var response = httpClient.GetAsync("https://api.keybit.ir/time/").Result;
            var response = httpClient.GetAsync("http://worldtimeapi.org/api/timezone/Asia/Tehran").Result;
            //result.Content.ReadAsAsync(JObject);

            //return await Task.Run(() => JObject.Parse("s"));

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }

        static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }
    }
}