﻿using JobShujuZhuaquConsoleApplication.Data;
using NHibernate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JobShujuZhuaquConsoleApplication
{
    public class ZhiweiZhuaquqi
    {

        public void Zhuaqu(int pageCount, string urlFormat)
        {
            WebClient client = new WebClient();
            client.Encoding = Encoding.GetEncoding("GB2312");
            int page = 1;
            while(page <= pageCount)
            {
                string url = string.Format(urlFormat, page);
                byte[] data = client.DownloadData(url);

                string body = Encoding.GetEncoding("GB2312").GetString(data);
                List<string> zhiweiList = this.GetZhiweiContent(body);
                foreach (string zhiwei in zhiweiList)
                {
                    this.Save(zhiwei);
                }
                page ++;
            }

        }
        public List<string> GetZhiweiContent(string body)
        {
            List<string> zhiweiList = new List<string>();
            int index = 0;
            while (true)
            {
                index = body.IndexOf("<div class=\"el\">");
                if(index == -1)
                {
                    break;
                }
                body = body.Substring(index);
                index = body.IndexOf("</div>");
                string zhiwei = body.Substring(0, index);
                zhiweiList.Add(zhiwei);
                body = body.Substring(index);
            }

            return zhiweiList;
        }

        public void Save(string zhiweiContent)
        {
            string zhiweiName = this.GetZhiweiName(zhiweiContent);
            string gongsiName = this.GetGongsiName(zhiweiContent);
            ZhiweiDataModel model = this.GetZhiwei(zhiweiName, gongsiName);
            if (model == null)
            {
                model = new ZhiweiDataModel();
                model.Id = Guid.NewGuid().ToString();
                model.Name = this.GetZhiweiName(zhiweiContent);
                model.GongsiName = this.GetGongsiName(zhiweiContent);
                model.Lianjie = this.GetZhiweiLianjie(zhiweiContent);
                model.Mingxi = this.GetZhiweiMingxi(model.Lianjie);
                model.CreatedTime = DateTime.Now;
                model.GongzuoDidian = this.GetGongzuoDidian(zhiweiContent);
                model.ZuidiGongzi = this.GetZuidiGongzi(zhiweiContent);
                model.ZuigaoGongzi = this.GetZuigaoGongzi(zhiweiContent);
                model.FabuTime = this.GetFabuTime(zhiweiContent);

                using (ISession session = NHibernateHelper.OpenSession())
                {
                    session.Save(model);
                    session.Flush();
                }
            }
            else
            {
                model.GongzuoDidian = this.GetGongzuoDidian(zhiweiContent);
                model.ZuidiGongzi = this.GetZuidiGongzi(zhiweiContent);
                model.ZuigaoGongzi = this.GetZuigaoGongzi(zhiweiContent);
                model.FabuTime = this.GetFabuTime(zhiweiContent);
                model.Lianjie = this.GetZhiweiLianjie(zhiweiContent);

                using (ISession session = NHibernateHelper.OpenSession())
                {
                    session.Update(model);
                    session.Flush();
                }
            }
        }

        private ZhiweiDataModel GetZhiwei(string zhiweiName, string gongsiName)
        {
            ZhiweiDataModel model = null;
            using (ISession session = NHibernateHelper.OpenSession())
            {
                model = session.QueryOver<ZhiweiDataModel>()
                    .Where(z => z.Name == zhiweiName && z.GongsiName == gongsiName)
                    .List()
                    .FirstOrDefault();
            }

            return model;
        }

        private string GetZhiweiName(string content)
        {
            content = content.Substring(content.IndexOf("<a"));
            content = content.Substring(content.IndexOf(">"));
            string name = content.Substring(1, content.IndexOf("<") - 1).Trim();

            return name;
        }

        private string GetGongsiName(string content)
        {
            content = content.Substring(content.IndexOf("class=\"t2\""));
            content = content.Substring(content.IndexOf("<a"));
            content = content.Substring(content.IndexOf(">"));
            string gongsiName = content.Substring(1, content.IndexOf("<") - 1).Trim();

            return gongsiName;
        }

        private string GetGongzuoDidian(string content)
        {
            content = content.Substring(content.IndexOf("class=\"t3\""));
            content = content.Substring(content.IndexOf(">"));
            string didian = content.Substring(1, content.IndexOf("<") - 1).Trim();

            return didian;
        }

        private int GetZuidiGongzi(string content)
        {
            content = content.Substring(content.IndexOf("class=\"t4\""));
            content = content.Substring(content.IndexOf(">"));
            content = content.Substring(1, content.IndexOf("<") - 1);
            string gongziString = content.Split('-')[0].Trim();
            int gongzi = 0;
            int.TryParse(gongziString, out gongzi);

            return gongzi;
        }

        private int GetZuigaoGongzi(string content)
        {
            content = content.Substring(content.IndexOf("class=\"t4\""));
            content = content.Substring(content.IndexOf(">"));
            content = content.Substring(1, content.IndexOf("<") - 1);
            int gongzi = 0;
            if (content.IndexOf("-") != -1)
            {
                string gongziString = content.Split('-')[1].Trim().TrimEnd("/月".ToArray());
                int.TryParse(gongziString, out gongzi);
            }

            return gongzi;
        }

        private DateTime GetFabuTime(string content)
        {
            content = content.Substring(content.IndexOf("class=\"t5\""));
            content = content.Substring(content.IndexOf(">"));
            content = content.Substring(1, content.IndexOf("<") - 1);
            DateTime date = new DateTime(2016, 1, 1);
            DateTime.TryParse("2016-" + content.Trim(), out date);

            return date;
        }

        private string GetZhiweiLianjie(string content)
        {
            Thread.Sleep(1000);

            content = content.Substring(content.IndexOf("http"));
            string url = content.Substring(0, content.IndexOf("\""));

            return url;
        }

        private string GetZhiweiMingxi(string url)
        {
            Thread.Sleep(1000);

            WebClient client = new WebClient();
            client.Encoding = Encoding.GetEncoding("GB2312");
            byte[] data = client.DownloadData(url);

            string body = Encoding.GetEncoding("GB2312").GetString(data);
            return body;
        }
    }
}