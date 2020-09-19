using Native.Csharp.App;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            public Dictionary<string, string> gKws = new Dictionary<string, string>();
            public Dictionary<string, string> pKws = new Dictionary<string, string>();
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

        }
        public static Config config = new Config();
        internal static string configPath;
        public const string split = "\n*#*#nextRandAnswer#*#*\n";
        static readonly Random r = new Random();
        internal static async Task Msg(GroupMsgArgs e)
        {
            if (!running)
                return;
            if (config.gList.ContainsKey(e.FromGroup))
                if (config.gList[e.FromGroup])
                    foreach (var i in config.gKws)
                        if (e.Msg.Contains(i.Key))
                        {
                            var ss = i.Value.Split(new string[] { split }, StringSplitOptions.RemoveEmptyEntries);
                            var rv = r.Next(ss.Length);
                            var mts = ss[rv].Replace("[at]", $"[LQ:@{e.FromQQ}]");
                            await Task.Delay(r.Next(config.gdb, config.gdt + 1) * 1000);
                            e.Reply(mts);
                        }
            if (config.pList.ContainsKey(e.FromGroup))
                if (config.pList[e.FromGroup])
                    foreach (var i in config.pKws)
                        if (e.Msg.Contains(i.Key))
                        {
                            var ss = i.Value.Split(new string[] { split }, StringSplitOptions.RemoveEmptyEntries);
                            var rv = r.Next(ss.Length);
                            await Task.Delay(r.Next(config.pdb, config.pdt + 1) * 1000);
                            Robot.Send.Temp(e.FromGroup, e.FromQQ, ss[rv]);
                        }
            if (config.fList.ContainsKey(e.FromGroup) && config.fg != 0)
                if (config.fList[e.FromGroup])
                    if (config.f.Length > 0)
                        for (var i = 0; i < config.f.Length; i++)
                        {
                            if (string.IsNullOrWhiteSpace(config.f[i]))
                                continue;
                            var r = new Regex(config.f[i]);
                            if (r.IsMatch(e.Msg))
                            {
                                if (config.realF)
                                {
                                    if (!WS.cacheQQ.TryGetValue(e.FromQQ, out var nick))
                                    {
                                        nick = Robot.GetNick(e.FromQQ);
                                        WS.cacheQQ.TryAdd(e.FromQQ, nick);
                                    }
                                    if (!WS.cacheGroup.TryGetValue(e.FromGroup, out var gn))
                                    {
                                        gn = Robot.GetGroupName(e.FromGroup);
                                        WS.cacheGroup.TryAdd(e.FromGroup, gn);
                                    }
                                    Robot.Send.Group(config.fg, $"QQ: {e.FromQQ}\n" +
                                        $"昵称: {nick}\n" +
                                        $"群号: {e.FromGroup}\n" +
                                        $"群名: {gn}\n" +
                                        $"消息内容: \n" +
                                        $"{e.Msg}");
                                }
                                else
                                {
                                    WS.MessageForSend data = new WS.MessageForSend
                                    {
                                        rid = WS.Rid(),
                                        from = e.FromQQ,
                                        content = e.Msg,
                                        qun = config.fg
                                    };
                                    //优先从缓存中查找数据
                                    if (!WS.cacheQQ.TryGetValue(e.FromQQ, out data.fromname))
                                    {
                                        var qqInfo = Robot.GetNick(e.FromQQ);
                                        if (qqInfo != null)
                                        {
                                            data.fromname = qqInfo;
                                            WS.cacheQQ.TryAdd(e.FromQQ, qqInfo);
                                        }
                                        else
                                        {
                                            WS.Log("群消息接口没有获取到QQ详细信息, QQ号码:" + e.FromQQ);
                                            return;
                                        }
                                    }

                                    //优先从缓存中查找数据
                                    if (!WS.cacheGroup.TryGetValue(config.fg, out data.qunname))
                                    {
                                        var groupInfo = Robot.GetGroupName(config.fg);
                                        if (groupInfo != null)
                                        {
                                            data.qunname = groupInfo;
                                            WS.cacheGroup.TryAdd(config.fg, groupInfo);
                                        }
                                        else
                                        {
                                            WS.Log("群消息接口没有获取到群信息, 群号码:" + config.fg);
                                            return;
                                        }
                                    }
                                    WS.postMessage(data);
                                }
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
                                    Robot.SendEmail(config.SMTP_User, config.SMTP_Pass, config.SMTP_Acieve, config.SMTP_Server, title, msg,config.SMTP_Port,config.SMTP_SSL);
                                }
                            }
                        }
        }

        internal static async Task Add(GroupMemberChangedArgs e)
        {
            if (!running)
                return;
            if (config.gWelcome.enabled)
            {
                if (config.gList.ContainsKey(e.Group))
                    if (config.gList[e.Group])
                    {
                        var mts = config.gWelcome.words.Replace("[at]", $"[LQ:@{e.BeingOperateQQ}]");
                        await Task.Delay(r.Next(config.gdb, config.gdt + 1) * 1000);
                        Robot.Send.Group(e.Group, mts);
                    }
            }
            if (config.pWelcome.enabled)
            {
                if (config.pList.ContainsKey(e.Group))
                    if (config.pList[e.Group])
                    {
                        var mts = config.pWelcome.words;
                        await Task.Delay(r.Next(config.pdb, config.pdt + 1) * 1000);
                        Robot.Send.Temp(e.Group, e.BeingOperateQQ, mts);
                    }
            }

        }
    }
}
