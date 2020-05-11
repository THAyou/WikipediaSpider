using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
namespace WikipediaSpider
{
    public partial class Form1 : Form
    {
        public string sqlconnection = "server=.;database=ZJXJG;Uid=lookchemweb;pwd=lookchem123$%^lueweb";
        private Thread _thread = null;
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            _thread = new Thread(Run);
            _thread.Start();
        }


        private void Run()
        {
            HtmlWeb htmlWeb = new HtmlWeb();

            htmlWeb.OverrideEncoding = Encoding.UTF8;
            var html = htmlWeb.Load("http://aed88.com/product.asp");
            var ProTypesHtml = html.DocumentNode.SelectNodes(".//div[@id=\"wai\"]/div[@class=\"body\"]/div[@class=\"left\"]/ul/li");
            int i = 0;
            ProTypesHtml.ToList().ForEach(m => 
            {
                i++;
                HtmlWeb product = new HtmlWeb();
                var host = "http://aed88.com/";
                var href = m.SelectSingleNode(".//a").Attributes.GetAttributes("href");
                var Title = m.SelectSingleNode(".//a").Attributes.GetAttributes("title");
                string sql = $@"insert into ContentClass(ParId,ClassName,Sequence,Depth,ChildNum,ParPath,Belock) values(3,'{Title}',0,1,0,'',0)
update ContentClass set ParPath = '3,' + cast(@@IDENTITY as varchar) where Id =@@IDENTITY
select @@IDENTITY as Id;";
                var Id = FindAll(sql).Tables[0].Rows[0]["Id"].ToString();
                listBox1.Items.Add(i+".增加产品类型[" + Title + "]成功");
                var pro = product.Load(host + href);
                var Prohtmlnode = pro.DocumentNode.SelectNodes(".//form[@id=\"MyForm\"]/tr/td[@valign=\"top\"]");
                var j = 0;
                Prohtmlnode.ToList().ForEach(p =>
                {
                    j++;
                    HtmlWeb productDetails = new HtmlWeb();
                    var ProName = p.SelectSingleNode(".//table/tr[1]/td/a").Attributes.GetAttributes("title");
                    var ProHref = p.SelectSingleNode(".//table/tr[1]/td/a").Attributes.GetAttributes("href");
                    var img = p.SelectSingleNode(".//table/tr[1]/td/a/img");    
                    var src = img.Attributes.GetAttributes("src");
                    var FileName = src;
                    var FilePath = "/" + src.Split('/')[1] + "/" + src.Split('/')[2];
                    var SavePath = "D:\\web\\LocalUser\\www.zjxjg.com\\WWW";
                    HtmlTol.HttpDownLoadFile(host + src, SavePath + FilePath, FileName.Split('/')[3]);
                    var ProductDocument = productDetails.Load(host + ProHref);
                    var Details = ProductDocument.DocumentNode.SelectSingleNode(".//div[@class=\"nei\"]/table/tr[2]/td").InnerHtml;
                    string Prosql = $"insert into [Product_Info](ClassId,EnCn,Pic,ProName,Description,IsShow,Sequence,IsRecommend) values(${Id},0,'{FileName}','{ProName}','{Details}',1,0,0)";
                    UpdateSingle(Prosql);
                    listBox1.Items.Add($"{i}.{j}增加产品[" + ProName + "]成功");
                });
            });

            MessageBox.Show("已完成");
        }
        private T ExcuteSql<T>(string sql, Func<SqlCommand, T> func)
        {
            using (SqlConnection conn = new SqlConnection(sqlconnection))
            {
                SqlCommand command = new SqlCommand(sql, conn);
                conn.Open();
                var result= func.Invoke(command);
                conn.Close();
                return result;
            }
        }

        private bool UpdateSingle(string sql)
        {
            return this.ExcuteSql<int>(sql, c => c.ExecuteNonQuery()) == 1;
        }

        private DataSet FindAll(string sql)
        {
            return this.ExcuteSql<DataSet>(sql, c =>
              {
                  SqlDataAdapter dataAdapter = new SqlDataAdapter(c);
                  DataSet ds = new DataSet();
                  dataAdapter.Fill(ds);
                  return ds;
              }
             );
        }
    }
}
