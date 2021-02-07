using Clansty.tianlang;
using Native.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Native.Csharp.App
{
    static class Events
    {
        internal static void Exit()
        {

        }
        internal static void Enable()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Customize.configPath = AppDomain.CurrentDomain.BaseDirectory + "demo";
            if (File.Exists(Customize.configPath))
                try
                {
                    Customize.config = JsonConvert.DeserializeObject<Customize.Config>(File.ReadAllText(Customize.configPath, Encoding.UTF8));
                }
                catch
                {
                    Customize.config = new Customize.Config();
                }
            AppInfo.self = long.Parse(Robot.GetQQ());
            DB.Group();
        }
        
        internal static void Disable()
        {
        }
        internal static void Msg(GroupMsgArgs e)
        {
            if (e.Msg.StartsWith("群盯盯存图") && e.Msg != "群盯盯存图")
            {
                Thread th = new Thread(new ThreadStart(delegate ()
                {
                    var f = new Form3(true, e.Msg.GetRight("群盯盯存图"));
                    Application.Run(f);
                }));
                th.TrySetApartmentState(ApartmentState.STA);
                th.Start();
            }
            try
            {
                string message = DB.HandleMsg(e.Msg);
                if (string.IsNullOrWhiteSpace(message))
                {
                    return;
                }
                if (DB.RunStatus)
                {
                    DB.MessageForSend data = new DB.MessageForSend
                    {
                        rid = DB.Rid(),
                        from = e.FromQQ,
                        content = message,
                        qun = e.FromGroup
                    };
                    //优先从缓存中查找数据
                    if (!DB.cacheQQ.TryGetValue(e.FromQQ, out data.fromname))
                    {
                        var qqInfo = Robot.GetNick(e.FromQQ);
                        if (qqInfo != null)
                        {
                            data.fromname = qqInfo;
                            DB.cacheQQ.TryAdd(e.FromQQ, qqInfo);
                        }
                        else
                        {
                            DB.Log("群消息接口没有获取到QQ详细信息, QQ号码:" + e.FromQQ);
                            return;
                        }
                    }

                    //优先从缓存中查找数据
                    if (!DB.cacheGroup.TryGetValue(e.FromGroup, out data.qunname))
                    {
                        var groupInfo = Robot.GetGroupName(e.FromGroup);
                        if (groupInfo != null)
                        {
                            data.qunname = groupInfo;
                            DB.cacheGroup.TryAdd(e.FromGroup, groupInfo);
                        }
                        else
                        {
                            DB.Log("群消息接口没有获取到群信息, 群号码:" + e.FromGroup);
                            return;
                        }
                    }
                    Customize.Msg(e);
                }
                else
                {
                    DB.Log("状态异常");
                }
            }
            catch (Exception ex)
            {
                DB.Log("处理群消息时发生未知错误, 错误信息:" + ex.Message);
            }
        }
    }
}
