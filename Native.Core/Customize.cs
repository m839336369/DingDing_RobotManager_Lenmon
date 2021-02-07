using Native.Csharp.App;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Native.Core
{
    static class Customize
    {
        public static bool running = false;
        public class Config
        {
            public class Welcome
            {
                public bool enabled = false;
                public string words = "";
            }
            public Welcome gWelcome = new Welcome();
            public Welcome pWelcome = new Welcome();
            public Dictionary<long, bool> gList = new Dictionary<long, bool>();
            public Dictionary<long, bool> pList = new Dictionary<long, bool>();
            public Dictionary<long, bool> fList = new Dictionary<long, bool>();
            public Dictionary<string, string> gKDB = new Dictionary<string, string>();
            public Dictionary<string, string> pKDB = new Dictionary<string, string>();
            public string[] f = new string[] { };
            public bool realF = false;
            public long fg = 0;
            public int gdb = 0;
            public int gdt = 20;
            public int pdb = 0;
            public int pdt = 20;
            public bool CSV = false;
            public string CSV_SavePath;
            public bool SMTP = false;
            public string SMTP_User;
            public string SMTP_Pass;
            public string SMTP_Acieve;
            public string SMTP_Server;
            public string SMTP_Port;
            public bool SMTP_SSL;
            public int Manager_Group_Invite_Request;
            public string Manager_Group_Invite_QQ;
            public int Manager_QQ_Request;
            public int member_enter_send;
            public int member_leave_send;
        }
        public static Config config = new Config();
        internal static string configPath;
        public const string split = "\n*#*#nextRandAnswer#*#*\n";
        static readonly Random r = new Random();
        internal static async Task Msg(GroupMsgArgs e)
        {
            if (config.fList.ContainsKey(e.FromGroup))
                if (config.fList[e.FromGroup])
                    if (config.f.Length > 0)
                        for (var i = 0; i < config.f.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(config.f[i]))
                                continue;
                            var r = new Regex(config.f[i]);

                            if (r.IsMatch(e.Msg))
                            {
                                if (Customize.config.CSV)
                                {
                                    string title;
                                    if (e.Msg.Length > 20) title = e.Msg.Substring(0, 20);
                                    else title = e.Msg;
                                    Robot.Write_Key_Log(config.f[i], DateTime.Now, e.FromQQ, Robot.GetNick(e.FromQQ), e.FromGroup, Robot.GetGroupName(config.fg), e.Msg, "群聊");
                                }
                                if (Customize.config.SMTP)
                                {
                                    string title;
                                    if (e.Msg.Length > 20) title = e.Msg.Substring(0, 20);
                                    else title = e.Msg;
                                    string msg = $"关键字:{config.f[i] }\n方式:群聊\n日期:{DateTime.Now.ToString()}\nQQ号:{e.FromQQ}\nQQ昵称:{Robot.GetNick(e.FromQQ)}\n群号码:{e.FromGroup}\n群名称:{Robot.GetGroupName(config.fg)}\n消息内容:{e.Msg}";
                                    Robot.SendEmail(config.SMTP_User, config.SMTP_Pass, config.SMTP_Acieve, config.SMTP_Server, title, msg, config.SMTP_Port, config.SMTP_SSL);
                                }
                            }
                        }
        }
    }
}
