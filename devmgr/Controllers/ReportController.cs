using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Text;
using Newtonsoft.Json;

namespace devmgr.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        public ActionResult Index()
        {
            string strCategories="";
            string strDataCol="";

            string strSql = "select b.cvalue as countItem,a.cnt as countVal from (select missiontype,count(id) cnt from FLOW_MISSION group by missiontype) a join dd_missiontype b on a.missiontype=b.code";
            DataSet ds = SqlHelper.ExecuteDataset(strSql);
            ChartsBind(ds, "countItem", "countVal",ref strCategories, ref strDataCol);
            strDataCol = ColumnDataToPieData(strCategories, strDataCol);
            ViewData["hcdata"] = ObjectToJson(strDataCol);


            return View();
        }
        public static string ObjectToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        static public string ColumnDataToPieData(string strCategories, string strDataCol)
        {
            string[] spCata = strCategories.Trim(']').Trim('[').Split(',');//  将X轴目录存入字符串数组
            for (int i = 0; i < spCata.Length; i++)
            {
                //依次将名字写入字符串,放到第i个"{"后
                string cataname = spCata[i];
                string insertStr = string.Format("name:{0},", cataname);
                int ind = FindIndex(strDataCol, '{', i + 1);//返回第i个指定字符的位置
                if (ind != -1)
                {
                    strDataCol = strDataCol.Insert(ind + 1, insertStr);
                }
            }
            return strDataCol;
        }
        //查找字符串中第j个指定字符的位置
        static public int FindIndex(string str, char c, int j)
        {
            //(string str,char c,int j)在字符串str中，寻找第j个C字符的位置,返回-1则没有
            int tempi = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == c)
                {
                    tempi++;
                    if (tempi == j)
                    {
                        return i;//返回索引
                    }
                }
            }
            return -1;
        }
        public static string ChartsBind(DataSet ds, string countItem, string countVal, ref string strCategories, ref string strDataCol)
        {
            DataTable dt = ds.Tables[0];
            string strData = "[";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var iRows = dt.Rows;
                strData += string.Format("['{0}',{1}],", iRows[i][countItem], iRows[i][countVal]);
            }
            strData = strData.TrimEnd(',') + "]";

            //柱状图 数据
            strCategories = "[";
            strDataCol = "[";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var iRows = dt.Rows;
                strCategories += "'" + iRows[i][countItem] + "',";
                strDataCol += "{y: " + iRows[i][countVal] + " },";
            }
            strCategories = strCategories.TrimEnd(',') + "]";
            strDataCol = strDataCol.TrimEnd(',') + "]";

            return strData;
        }
    }
}