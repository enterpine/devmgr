using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace devmgr.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        public ActionResult Index()
        {
            string strCategories;
            string strDataCol;

            string strSql = "select zhaoBiaoUnit as countItem,COUNT(0) as countVal from sys_user group by missiontype";


            return View();
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
    }
}