using Clansty.tianlang;
using Native.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
            Customize.configPath = AppDomain.CurrentDomain.BaseDirectory + "enlovoconfig";
            if (File.Exists(Customize.configPath))
                try
                {
                    Customize.config = JsonConvert.DeserializeObject<Customize.Config>(File.ReadAllText(Customize.configPath, Encoding.UTF8));
                }
                catch
                {
                    Customize.config = new Customize.Config();
                }
            Task.Run(WS.Start);
        }
        
        internal static void Disable()
        {
        }
        internal static void Msg(FriendMsgArgs e)
        {
            if (e.Msg.StartsWith("群盯盯存图") && e.Msg != "群盯盯存图")
            {
                Thread th = new Thread(new ThreadStart(delegate ()
                {
                    Application.Run(new Form3(false, e.Msg.GetRight("群盯盯存图")));
                }));
                th.TrySetApartmentState(ApartmentState.STA);
                th.Start();
            }
            try
            {
                string message = WS.HandleMsg(e.Msg);
                if (string.IsNullOrWhiteSpace(message))
                {
                    return;
                }
                if (WS.webSocketRunStatus)
                {
                    WS.MessageForSend data = new WS.MessageForSend();
                    data.rid = WS.Rid();
                    data.from = e.FromQQ;
                    data.content = message;

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
                            WS.Log("好友消息接口没有获取到QQ详细信息, QQ号码:" + e.FromQQ);
                            return;
                        }
                    }
                    WS.postMessage(data);

                }
                else
                {
                    WS.Log("WebSocket状态异常");
                }
            }
            catch (Exception ex)
            {
                WS.Log("处理好友消息时发生未知错误, 错误信息:" + ex.Message);
            }
        }
        internal static void GroupCardChanged(GroupCardChangedArgs e)
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
                Customize.Msg(e);
                string message = WS.HandleMsg(e.Msg);
                if (string.IsNullOrWhiteSpace(message))
                {
                    return;
                }
                if (WS.webSocketRunStatus)
                {
                    WS.MessageForSend data = new WS.MessageForSend
                    {
                        rid = WS.Rid(),
                        from = e.FromQQ,
                        content = message,
                        qun = e.FromGroup
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
                    if (!WS.cacheGroup.TryGetValue(e.FromGroup, out data.qunname))
                    {
                        var groupInfo = Robot.GetGroupName(e.FromGroup);
                        if (groupInfo != null)
                        {
                            data.qunname = groupInfo;
                            WS.cacheGroup.TryAdd(e.FromGroup, groupInfo);
                        }
                        else
                        {
                            WS.Log("群消息接口没有获取到群信息, 群号码:" + e.FromGroup);
                            return;
                        }
                    }
                    WS.postMessage(data);
                }
                else
                {
                    WS.Log("WebSocket状态异常");
                }
            }
            catch (Exception ex)
            {
                WS.Log("处理群消息时发生未知错误, 错误信息:" + ex.Message);
            }
        }
        internal static void AddFriend(RequestAddFriendArgs e)
        {
            try
            {
                if (WS.webSocketRunStatus)
                {
                    e.Accept();
                    try
                    {
                        if (WS.webSocketRunStatus)
                        {
                            //推送好友信息
                            WS.FriendAndQunInfo info = new WS.FriendAndQunInfo();
                            info.number = e.FromQQ;
                            info.type = 1;
                            info.from = 0;
                            //优先从缓存中查找数据
                            if (!WS.cacheQQ.TryGetValue(e.FromQQ, out info.name))
                            {
                                var qqInfo = Robot.GetNick(e.FromQQ);
                                if (qqInfo != null)
                                {
                                    info.name = qqInfo;
                                    WS.cacheQQ.TryAdd(e.FromQQ, qqInfo);
                                }
                                else
                                {
                                    WS.Log("好友消息接口没有获取到QQ详细信息, QQ号码:" + e.FromQQ);
                                    return;
                                }
                            }
                            WS.FriendAndQun friendInfo = new WS.FriendAndQun();
                            friendInfo.infos = new List<WS.FriendAndQunInfo>();
                            friendInfo.rid = WS.Rid();
                            friendInfo.infos.Add(info);
                            string message = JsonConvert.SerializeObject(friendInfo);
                            WS.sendMessage(message);
                            //推送好友添加通知
                            Task.Run(() =>
                            {
                                Thread.Sleep(2000);
                                WS.MessageForSend data = new WS.MessageForSend();
                                data.rid = WS.Rid();
                                data.from = e.FromQQ;
                                data.content = "[好友添加通知]";
                                //优先从缓存中查找数据
                                if (!WS.cacheQQ.ContainsKey(e.FromQQ))
                                {
                                    var qqInfo = Robot.GetNick(e.FromQQ);
                                    if (qqInfo != null)
                                    {
                                        data.fromname = qqInfo;
                                        WS.cacheQQ[e.FromQQ] = qqInfo;
                                    }
                                    else
                                    {
                                        WS.Log("好友消息接口没有获取到QQ详细信息, QQ号码:" + e.FromQQ);
                                        return;
                                    }
                                }
                                else
                                {
                                    data.fromname = WS.cacheQQ[e.FromQQ].ToString();
                                }
                                WS.postMessage(data);
                            });
                        }
                        else
                        {
                            WS.Log("WebSocket状态异常");
                        }

                    }
                    catch (Exception ex)
                    {
                        WS.Log("好友添加成功事件发生未知错误,错误原因:" + ex.Message);
                    }
                }
                else
                {
                    WS.Log("WebSocket状态异常");
                }

            }
            catch (Exception ex)
            {
                WS.Log("处理收到好友添加请求出错,错误原因:" + ex.Message);
            }
        }
        internal static void GroupAddMember(GroupMemberChangedArgs e)
        {
            try
            {
                Customize.Add(e);
                if (WS.webSocketRunStatus)
                {
                    if (e.BeingOperateQQ == AppInfo.self)
                    {
                        //自己新进了一个群
                        WS.SendGroupAndFriend();
                    }
                    else
                    {
                        //新成员加入
                        WS.MessageForSend data = new WS.MessageForSend();
                        data.rid = WS.Rid();
                        data.from = e.BeingOperateQQ;
                        data.content = "[入群通知]";
                        data.qun = e.Group;
                        //优先从缓存中查找数据
                        if (!WS.cacheQQ.TryGetValue(e.BeingOperateQQ, out data.fromname))
                        {
                            var qqInfo = Robot.GetNick(e.BeingOperateQQ);
                            if (qqInfo != null)
                            {
                                data.fromname = qqInfo;
                                WS.cacheQQ.TryAdd(e.BeingOperateQQ, qqInfo);
                            }
                            else
                            {
                                WS.Log("群消息接口没有获取到QQ详细信息, QQ号码:" + e.BeingOperateQQ);
                                return;
                            }
                        }

                        if (!WS.cacheGroup.TryGetValue(e.Group, out data.qunname))
                        {
                            var groupInfo = Robot.GetGroupName(e.Group);
                            if (groupInfo != null)
                            {
                                data.qunname = groupInfo;
                                WS.cacheGroup.TryAdd(e.Group, groupInfo);
                            }
                            else
                            {
                                WS.Log("群消息接口没有获取到群信息, 群号码:" + e.Group);
                                return;
                            }
                        }
                        WS.postMessage(data);
                    }

                }
                else
                {
                    WS.Log("WebSocket状态异常");
                }
            }
            catch (Exception ex)
            {
                WS.Log("处理新增群成员事件时出错, 错误信息:" + ex.Message);
            }
        }
        internal static void GroupLeftMember(GroupMemberChangedArgs e)
        {
            try
            {
                if (WS.webSocketRunStatus)
                {
                    if (e.FromQQ == AppInfo.self)
                    {
                        //自己被T
                    }
                    else
                    {
                        WS.MessageForSend data = new WS.MessageForSend();
                        data.rid = WS.Rid();
                        data.from = e.BeingOperateQQ;
                        data.content = "[退群通知]";
                        data.qun = e.Group;
                        //优先从缓存中查找数据
                        if (!WS.cacheQQ.TryGetValue(e.BeingOperateQQ, out data.fromname))
                        {
                            var qqInfo = Robot.GetNick(e.BeingOperateQQ);
                            if (qqInfo != null)
                            {
                                data.fromname = qqInfo;
                                WS.cacheQQ.TryAdd(e.BeingOperateQQ, qqInfo);
                            }
                            else
                            {
                                WS.Log("没有获取到QQ详细信息, QQ号码:" + e.BeingOperateQQ);
                                return;
                            }
                        }

                        if (!WS.cacheGroup.TryGetValue(e.Group, out data.qunname))
                        {
                            var groupInfo = Robot.GetGroupName(e.Group);
                            if (groupInfo != null)
                            {
                                data.qunname = groupInfo;
                                WS.cacheGroup.TryAdd(e.Group, groupInfo);
                            }
                            else
                            {
                                WS.Log("群消息接口没有获取到群信息, 群号码:" + e.Group);
                                return;
                            }
                        }
                        WS.postMessage(data);
                    }

                }
                else
                {
                    WS.Log("WebSocket状态异常");
                }
            }
            catch (Exception ex)
            {
                WS.Log("处理群成员退出事件时出错, 错误信息:" + ex.Message);
            }
        }
        internal static void JoinGroupRequest(RequestAddGroupArgs e)
        {
            try
            {
                if (WS.webSocketRunStatus)
                {
                    WS.MessageForSend data = new WS.MessageForSend();
                    data.rid = WS.Rid();
                    data.from = e.QQ;
                    data.content = "[入群通知]";
                    data.qun = e.Group;
                    //优先从缓存中查找数据
                    if (!WS.cacheQQ.TryGetValue(e.QQ, out data.fromname))
                    {
                        var qqInfo = Robot.GetNick(e.QQ);
                        if (qqInfo != null)
                        {
                            data.fromname = qqInfo;
                            WS.cacheQQ.TryAdd(e.QQ, qqInfo);
                        }
                        else
                        {
                            WS.Log("群消息接口没有获取到QQ详细信息, QQ号码:" + e.QQ);
                            return;
                        }
                    }
                    e.Accept();

                    WS.Log("通过群添加请求");
                    //优先从缓存中查找数据
                    if (!WS.cacheGroup.TryGetValue(e.Group, out data.qunname))
                    {
                        var groupInfo = Robot.GetGroupName(e.Group);
                        if (groupInfo != null)
                        {
                            data.qunname = groupInfo;
                            WS.cacheGroup.TryAdd(e.Group, groupInfo);
                        }
                        else
                        {
                            WS.Log("群消息接口没有获取到群信息, 群号码:" + e.Group);
                            return;
                        }
                    }

                    WS.postMessage(data);
                }
                else
                {
                    WS.Log("WebSocket状态异常");
                }
            }
            catch (Exception ex)
            {
                WS.Log("处理入群申请时发生未知错误, 错误信息:" + ex.Message);
            }
        }
        internal static void InviteGroupRequest(RequestAddGroupArgs e)
        {
            try
            {
                if (WS.webSocketRunStatus)
                {
                    e.Accept();
                    WS.Log("接受群邀请");
                }
                else
                {
                    WS.Log("WebSocket状态异常");
                }
            }
            catch (Exception ex)
            {
                WS.Log("处理新增群成员事件时出错, 错误信息:" + ex.Message);
            }
        }
    }
}
