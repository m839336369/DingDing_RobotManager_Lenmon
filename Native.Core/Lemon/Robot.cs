using Native.Core;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace Native.Csharp.App
{
    static class Robot
    {
        static int ac = 0;
        [DllExport("AppInfo", CallingConvention.StdCall)]
        static string _AppInfo() => new JavaScriptSerializer().Serialize(new AppInfo());
        [DllExport(CallingConvention.StdCall)]
        static int Initialize(int acc)
        {
            ac = acc;
            return 0;
        }
        [DllExport(CallingConvention.StdCall)]
        static int _eventExit()
        {
            Events.Exit();
            return 0;
        }
        [DllExport(CallingConvention.StdCall)]
        static int _eventEnable()
        {
            Events.Enable();
            return 0;
        }
        [DllExport(CallingConvention.StdCall)]
        static int _eventDisable()
        {
            Events.Disable();
            return 0;
        }
        [DllExport(CallingConvention.StdCall)]
        static int _eventRecvMsg(long recvQQ, string msgId, string msgse, int t, int type, long g, long sq, string msg)
        {
            if (recvQQ == sq)
                return 0;
            switch (type)
            {
                case 1:
                case 4:
                    //私聊消息
                    Events.Msg(new FriendMsgArgs(sq, msg));
                    break;
                case 2:
                    //群消息
                    Events.Msg(new GroupMsgArgs(sq, g, msg));
                    break;
            }
            return 0;
        }
        [DllExport(CallingConvention.StdCall)]
        static int _eventRequest_AddFriend(long qqid, int st, int q, string msg)
        {
            Events.AddFriend(new RequestAddFriendArgs(q, msg));
            return 0;
        }
        [DllExport(CallingConvention.StdCall)]
        static int _eventSystem_GroupMemberIncrease(long a, int b, int c, long d, long e, long f)
        {
            Events.GroupAddMember(new GroupMemberChangedArgs(d, e, f));
            return 0;
        }
        [DllExport(CallingConvention.StdCall)]
        static int _eventSystem_GroupMemberDecrease(long a, int b, int c, long d, long e, long f)
        {
            Events.GroupLeftMember(new GroupMemberChangedArgs(d, e, f));
            return 0;
        }
        [DllExport(CallingConvention.StdCall)]
        static int _eventRequest_AddGroup(long a, int b, string c, int d, long e, long f, long g, string h, int i)
        {
            if (b == 104 || b == 102)
            {
                Events.JoinGroupRequest(new RequestAddGroupArgs(c, e, f, i, h));
            }
            if (b == 105)
            {
                Events.InviteGroupRequest(new RequestAddGroupArgs(c, e, f, i, h));
            }
            return 0;
        }
        [DllExport(CallingConvention.StdCall)]
        static int _eventTipsMsg(long a, int b, long c, long d, long e, string f)
        {
            if (b == 112)
            {
                Events.GroupCardChanged(new GroupCardChangedArgs(c, d, f));
            }
            return 0;
        }
        [DllExport(CallingConvention.StdCall)]
        static int _menu()
        {
            new Form1().Show();
            return 0;
        }

        public static class Send
        {
            public static void Group(long group, string msg)
            {
                Api_sendGroupMsg(ac, App.AppInfo.self, group, msg);
            }
            [DllImport("LqrHelper.dll")]
            extern static void Api_sendGroupMsg(int a, long b, long c, string d);
            public static void Temp(long group, long qq, string msg)
            {
                Api_sendTransieMsg(ac, App.AppInfo.self, group, msg, qq);
            }
            [DllImport("LqrHelper.dll")]
            extern static void Api_sendTransieMsg(int a, long b, long c, string d, long e);
            public static void Friend(long qq, string msg)
            {
                Api_sendPrivateMsg(ac, App.AppInfo.self, qq, msg);
            }
            [DllImport("LqrHelper.dll")]
            extern static void Api_sendPrivateMsg(int a, long b, long c, string d);
        }

        public static void GroupKickMember(long group, long qq)
        {
            Api_KickGroupMember(Native.Csharp.App.AppInfo.self, group, qq, false);
        }
        [DllImport("LqrHelper.dll")]
        extern static void Api_KickGroupMember(long a, long b, long c, bool d);
        public static string GetGroupMemberCard(long group, long qq)
        {
            //&nbsp;
            var r = Marshal.PtrToStringAnsi(Api_GetGroupMemberCard(App.AppInfo.self, group, qq));
            r = r.Replace("&nbsp;", " ");//nbsp 和空格是有区别的
            r = HttpUtility.HtmlDecode(r);
            return r.Replace("\\/", "/");
        }

        [DllImport("LqrHelper.dll")]
        extern static IntPtr Api_GetGroupMemberCard(long a, long b, long c);
        public static void SetGroupMemberCard(long group, long qq, string card)
        {
            Api_SetGroupMemberCard(Native.Csharp.App.AppInfo.self, group, qq, card);
        }
        [DllImport("LqrHelper.dll")]
        extern static void Api_SetGroupMemberCard(long a, long b, long c, string d);
        public static JObject GetGroupMembers(long group)
        {
            var json = Marshal.PtrToStringAnsi(Api_GetGroupMemberList(AppInfo.self, group));
            return JObject.Parse(json);
        }
        [DllImport("LqrHelper.dll")]
        extern static IntPtr Api_GetGroupMemberList(long a, long b);
        public static string GetNick(long qq)
        {
            try
            {
                WebClient client = new WebClient();
                client.Encoding = Encoding.UTF8;
                var json = client.DownloadString($"https://api.vvhan.com/api/qq?qq={qq}");
                client.Dispose();
                var jo = JObject.Parse(json);
                return jo.Value<string>("name");
            }
            catch
            {
                try
                {
                    WebClient client = new WebClient();
                    client.Encoding = Encoding.UTF8;
                    var json = client.DownloadString($"https://api.hmister.cn/qq/?type=nick&qq={qq}");
                    client.Dispose();
                    var jo = JObject.Parse(json);
                    if (jo.Value<int>("code") == 200)
                        return jo.Value<string>("data");
                    else throw new Exception();
                }
                catch
                {
                    try
                    {
                        WebClient client = new WebClient();
                        client.Encoding = Encoding.UTF8;
                        var json = client.DownloadString($"https://api.qqsuu.cn/api/qq?qq={qq}");
                        client.Dispose();
                        var jo = JObject.Parse(json);
                        if (jo.Value<int>("code") == 1)
                            return jo.Value<string>("name");
                        else throw new Exception();
                    }
                    catch
                    {
                        return qq.ToString();
                    }
                }
            }
        }
        [DllImport("LqrHelper.dll")]
        extern static IntPtr Api_GetOnLineList();
        public static string GetQQ()
        {
            return Marshal.PtrToStringAnsi(Api_GetOnLineList());
        }
        [DllImport("LqrHelper.dll")]
        extern static IntPtr Api_GetGroupInfo(long a, long b);
        public static string GetGroupName(long group)
        {
            var r = Marshal.PtrToStringAnsi(Api_GetGroupInfo(AppInfo.self, group));
            var jo = JObject.Parse(r);
            return jo.Value<string>("gName");
        }
        [DllImport("LqrHelper.dll")]
        extern static void Api_OutPutLog(int a, string b, string c);
        public static void Print(string type, string msg) => Api_OutPutLog(ac, type, msg);
        [DllImport("LqrHelper.dll")]
        extern static IntPtr Api_GetGroupList(int b, long j);
        public static JToken GetGroupList()
        {
            var r = Marshal.PtrToStringAnsi(Api_GetGroupList(ac, AppInfo.self));
            return JToken.Parse(r);
        }
        [DllImport("LqrHelper.dll")]
        extern static IntPtr Api_GetFriendList(int b, long j);
        public static JToken GetFriendsList()
        {
            var r = Marshal.PtrToStringAnsi(Api_GetFriendList(ac, AppInfo.self));
            return JToken.Parse(r);
        }
        public static void Write_Key_Log(string key, DateTime dateTime, long QQ, string nickName, long QQ_Group, string Group_Name, string msg, string style)
        {
            string sFileName = Customize.config.CSV_SavePath;
            if (!File.Exists(sFileName))
            //验证文件是否存在，有则追加，无则创建
            {
                File.AppendAllText(sFileName, "关键字,方式,日期,QQ号,QQ昵称,群号码,群名称,消息内容\n");
            }
            File.AppendAllText(sFileName, $"{key},{style},{dateTime},{QQ},{nickName},{QQ_Group},{Group_Name},{msg}\n");
        }
        /*
        string userEmail,  //发件人邮箱
        string userPswd,   //邮箱帐号密码
        string toEmail,    //收件人邮箱
        string mailServer, //邮件服务器
        string subject,    //邮件标题
        string mailBody,   //邮件内容
        string[] attachFiles //邮件附件
        */
        public static void SendEmail(string userEmail, string userPswd, string toEmail, string mailServer, string subject, string mailBody)
        {
            //邮箱帐号的登录名
            string username = userEmail.Substring(0, userEmail.IndexOf('@'));
            //邮件发送者
            MailAddress from = new MailAddress(userEmail);
            //邮件接收者
            MailAddress to = new MailAddress(toEmail);
            MailMessage mailobj = new MailMessage(from, to);
            // 添加发送和抄送
            // mailobj.To.Add("");
            // mailobj.CC.Add("");

            //邮件标题
            mailobj.Subject = subject;
            //邮件内容
            mailobj.Body = mailBody;
            //邮件不是html格式
            mailobj.IsBodyHtml = false;
            //邮件编码格式
            mailobj.BodyEncoding = System.Text.Encoding.GetEncoding("GB2312");
            //邮件优先级
            mailobj.Priority = MailPriority.High;

            //Initializes a new instance of the System.Net.Mail.SmtpClient class
            //that sends e-mail by using the specified SMTP server.
            SmtpClient smtp = new SmtpClient(mailServer);
            //或者用：
            //SmtpClient smtp = new SmtpClient();
            //smtp.Host = mailServer;

            //不使用默认凭据访问服务器
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(username, userPswd);
            //使用network发送到smtp服务器
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            try
            {
                //开始发送邮件
                smtp.Send(mailobj);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
