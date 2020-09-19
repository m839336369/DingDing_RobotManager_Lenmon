using Native.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Native.Csharp.App
{
    public static class WS
    {
        #region 变量
        /// <summary>
        /// WebSocket连接
        /// </summary>
        private static WebSocket webSocket;

        /// <summary>
        /// WebSocket运行状态
        /// </summary>
        public static bool webSocketRunStatus = false;

        /// <summary>
        /// 群数据缓存
        /// </summary>
        public static ConcurrentDictionary<long, string> cacheGroup = new ConcurrentDictionary<long, string>();


        /// <summary>
        /// QQ数据缓存
        /// </summary>
        public static ConcurrentDictionary<long, string> cacheQQ = new ConcurrentDictionary<long, string>();


        #endregion 变量



        #region 初始化

        /// <summary>
        /// WebSocket初始化
        /// </summary>
        public static void Init()
        {
            string uri =
                $"ws://api.ronsir.cn/ws/?username=robot&password={md5(md5(DateTime.Now.ToString("yyyyMMdd")))}&type=pcClient&clientid=2&number={AppInfo.self}";
            webSocket = new WebSocket(uri);
            webSocket.OnOpen += WebSocket_OnOpen;
            webSocket.OnMessage += WebSocket_OnMessage;
            webSocket.OnError += WebSocket_OnError;
            webSocket.OnClose += WebSocket_OnClose;
            webSocket.Connect();

        }
        internal static void Start()
        {
            Task.Delay(5000).Wait();
            try
            {
                var strQQ = Robot.GetQQ();
                while (string.IsNullOrEmpty(strQQ))
                {
                    Log("尚未登录账号，5 秒后重新获取");
                    Task.Delay(5000).Wait();
                    strQQ = Robot.GetQQ();
                }
                AppInfo.self = long.Parse(strQQ);
                Init();
                //Task.Run(CheckLoop);
                //更新群和好友信息
                SendGroupAndFriend();
            }
            catch (Exception e)
            {
                Log("初始化错误, 错误信息:" + e.ToString());
            }
        }
        internal static void CheckLoop()
        {
            Task.Delay(5000).Wait();
            while (true)
            {
                var strQQ = Robot.GetQQ();
                if (string.IsNullOrEmpty(strQQ))
                {
                    webSocketRunStatus = false;
                    webSocket.Close();
                    Start();
                    return;
                }
            }
        }

        #endregion 初始化

        #region WebSocket
        /// <summary>
        /// WebSocket关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void WebSocket_OnClose(object sender, CloseEventArgs e)
        {
            Customize.running = false;
            try
            {
                if (webSocketRunStatus)
                {
                    webSocketRunStatus = false;
                    //意外关闭, 等待3秒钟重新连接
                    Log("WebSocket连接意外关闭, 3秒钟后重新连接");
                    new Task(async () =>
                    {
                        await Task.Delay(3000);
                        webSocketRunStatus = true;
                        Init();
                    }).Start();
                }
                else
                {
                    //正常关闭
                    Log("WebSocket连接关闭");
                }
            }
            catch (Exception ex)
            {
                Log("WebSocket连接异常,错误信息:" + ex.Message);
            }

        }

        /// <summary>
        /// WebSocket错误事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void WebSocket_OnError(object sender, ErrorEventArgs e)
        {
            Log("WebSocket发生错误, 错误信息:" + e.Message);
        }

        /// <summary>
        /// WebSocket消息处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void WebSocket_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                Log("收到信息:" + e.Data);
                if (e.Data.Contains(@"""message"":""连接成功"""))
                {
                    Customize.running = true;
                    Log("自定义功能已激活");
                }

                if (e.Data.Contains("\"action\":\"message\""))
                {

                    MessageForReceive msg = JsonConvert.DeserializeObject<MessageForReceive>(e.Data);
                    if (msg.type == 0)
                    {
                        Robot.Send.Friend(msg.to, msg.content);
                        Log("发送消息回执代码:0");
                    }

                    if (msg.type == 1)
                    {
                        Robot.Send.Group(msg.to, msg.content);
                        Log("发送消息回执代码:0");
                    }

                }
            }
            catch
            {
                Log("接收到错误的消息格式, 消息原文:" + e.Data);
            }
        }

        /// <summary>
        /// WebSocket连接成功事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void WebSocket_OnOpen(object sender, EventArgs e)
        {
            Log("WebSocket连接成功");
            webSocketRunStatus = true;
        }


        /// <summary>
        /// WebSocket发送消息
        /// </summary>
        /// <param name="message"></param>
        public static void sendMessage(string message)
        {
            Log("发送消息:" + message);
            webSocket.Send(message);
        }

        /// <summary>
        /// 推送QQ消息
        /// </summary>
        /// <param name="msg"></param>
        public static void postMessage(MessageForSend msg)
        {
            string data = JsonConvert.SerializeObject(msg);
            webSocket.Send(data);
            Log("发送消息:" + data);
        }

        #endregion WebSocket

        #region 结构

        /// <summary>
        /// 结构-发送QQ消息到服务器
        /// </summary>
        public class MessageForSend
        {
            public string rid;
            public string action = "postMessage";
            public long qun;
            public string qunname;
            public long from;
            public string fromname;
            public string content;
        };


        /// <summary>
        /// 结构->发送消息给好友
        /// </summary>
        public class MessageForReceive
        {
            public string action;
            public string rid;
            public string message_id;
            public int type;
            public long to;
            public string content;
        }

        public class MessageStatus
        {
            public string action = "postsendMessageStatus";
            public string message_id;
            public int status;
        }

        /// <summary>
        /// 结构->群信息, 群成员信息
        /// </summary>
        public class FriendAndQunInfo
        {
            /// <summary>  
            /// 群号
            /// Examples: 130780544, 310218045, 362782870, 434208112, 583365135  
            /// </summary>  
            public long number;

            /// <summary>  
            /// 群名称
            /// Examples: "妍诗美天然护肤88群", "丹青、蝶恋花、相遇…", "CMP-CK-PHP交流②群", "蓝幽若读者验证群", "粉丝福利153群"  
            /// </summary>  
            public string name;

            /// <summary>  
            /// 1群 0好友
            /// Examples: 1,0
            /// </summary>  
            public int type;

            /// <summary>  
            /// 来源群
            /// Examples: 130780544
            /// </summary> 
            public long from;
        }

        /// <summary>
        /// 结构->更新群信息,成员信息
        /// </summary>
        public class FriendAndQun
        {
            /// <summary>
            /// 请求名
            /// Examples: "updateFriendAndQun"
            /// </summary>
            public string action = "updateFriendAndQun";

            /// <summary>  
            /// 请求RID
            /// Examples: "973411920048"  
            /// </summary>  
            public string rid;

            /// <summary>
            /// 信息列表
            /// Examples: [{"number":130780544,"name":"妍诗美天然护肤88群","type":1,"from":0},{"number":310218045,"name":"丹青、蝶恋花、相遇…","type":1,"from":0},{"number":362782870,"name":"CMP-CK-PHP交流②群","type":1,"from":0},{"number":434208112,"name":"蓝幽若读者验证群","type":1,"from":0},{"number":583365135,"name":"粉丝福利153群","type":1,"from":0},{"number":614167038,"name":"330701豪曦妈国学育儿群","type":1,"from":0}]
            /// </summary>
            public List<FriendAndQunInfo> infos;
        }



        public class Request<T>
        {
            public string action { get; set; }

            public string rid { get; set; }

            public T data { get; set; }
        }



        #endregion 结构;


        #region 通用
        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="type">类型</param>
        public static void Log(string content, string type = "Enlovo")
        {
            try
            {
                Robot.Print(type, content);
            }
            catch (Exception ex)
            {
                Robot.Print(type, $"写入日志发生错误:{Newtonsoft.Json.JsonConvert.SerializeObject(ex)}");
            }
        }



        public class Member
        {
            public ulong number { get; set; }
            public string nickname { get; set; }
            public int sex { get; set; }
            public string area { get; set; }
            public long jointime { get; set; }
            public long lasttime { get; set; }
            public int auth { get; set; }
        }

        /// <summary>
        /// memberOfGroup
        /// </summary>
        public class GroupMember
        {

            public string groupname { get; set; }

            public ulong group { get; set; }

            public List<Member> list { get; set; }

        }


        /// <summary>
        /// 更新群和好友信息
        /// </summary>
        public static void SendGroupAndFriend()
        {
            Task.Run(async () =>
            {
                await Task.Delay(20000);
                while (true)
                {
                    if (webSocketRunStatus)
                    {
                        var groupList = Robot.GetGroupList();
                        try
                        {
                            var infos = new FriendAndQun();
                            infos.rid = Rid();
                            infos.infos = new List<FriendAndQunInfo>();
                            foreach (var group in groupList)
                            {
                                var info = new FriendAndQunInfo();
                                info.number = group.Value<long>("uin");
                                info.name = group.Value<string>("name");
                                info.type = 1;
                                infos.infos.Add(info);
                                cacheGroup[group.Value<long>("uin")] = group.Value<string>("name");
                            }
                            string message = JsonConvert.SerializeObject(infos);
                            sendMessage(message);
                        }
                        catch { }

                        try
                        {
                            var friendList = Robot.GetFriendsList();
                            var infos = new FriendAndQun();
                            infos.rid = Rid();
                            infos.infos = new List<FriendAndQunInfo>();
                            foreach (var friend in friendList)
                            {
                                var info = new FriendAndQunInfo();

                                info.number = friend.Value<long>("uin");
                                info.name = friend.Value<string>("name");
                                info.type = 0;
                                infos.infos.Add(info);
                                cacheQQ[friend.Value<long>("uin")] = friend.Value<string>("name");
                            }
                            string message = JsonConvert.SerializeObject(infos);
                            sendMessage(message);

                        }
                        catch { }
                        Log("启动更新群成员线程");
                        foreach (var group in groupList)
                        {
                            try
                            {
                                var info1 = Robot.GetGroupMembers(group.Value<long>("uin"));
                                var info = info1.SelectToken("member");
                                var data = new GroupMember()
                                {
                                    group = group.Value<ulong>("uin"),
                                    groupname = group.Value<string>("name"),
                                    list = new List<Member>()
                                };
                                foreach (var item in info)
                                {
                                    var m = new Member()
                                    {
                                        jointime = item.Value<long>("jointime"),
                                        lasttime = item.Value<long>("speaktime"),
                                        sex = item.Value<int>("sex"),
                                        number = item.Value<ulong>("uin"),
                                        nickname = item.Value<string>("name")
                                    };
                                    cacheQQ[item.Value<long>("uin")] = item.Value<string>("name");
                                    if (info1.Value<long>("owner") == item.Value<long>("uin"))
                                        m.auth = 3;
                                    //todo 管理员判断
                                    //else if (info1["adm"].Contains(long.Parse(item.Key)))
                                    //    m.auth = 2;
                                    else
                                        m.auth = 1;
                                    data.list.Add(m);
                                }
                                var request = new Request<List<GroupMember>>
                                {
                                    action = "memberOfGroup",
                                    rid = Rid()
                                };
                                var datas = new List<GroupMember>();
                                datas.Add(data);
                                request.data = datas;
                                string json = JsonConvert.SerializeObject(request);
                                sendMessage(json);
                                await Task.Delay(20000);
                            }
                            catch { }
                        }

                        await Task.Delay(3600 * 24 * 1000);
                    }
                    else
                    {
                        await Task.Delay(20000);
                    }
                }


            });

        }


        /// <summary>
        /// MD5
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string md5(string str)
        {
            string cl = str;
            string pwd = "";
            MD5 md5 = MD5.Create();//实例化一个md5对像
                                   // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 

                pwd = pwd + s[i].ToString("x");

            }
            return pwd;
        }

        /// <summary>
		/// 处理消息,解析图片，去除其余CQ码
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		public static string HandleMsg(string msg)
        {
            //先处理图片
            if (msg.Contains("[LQ:pic="))
            {
                Regex regex = new Regex(@"\[LQ:pic={(.+?)}\.jpg\]");
                foreach (Match item in regex.Matches(msg))
                {
                    msg = msg.Replace(item.Groups[0].Value, "[Image " + IniReadValue(AppDomain.CurrentDomain.BaseDirectory + "data\\Images\\" + item.Groups[1].Value, "data", "url") + "]");
                }
            }

            //处理其他CQ码
            if (msg.Contains("[LQ:"))
            {
                Regex regex = new Regex(@"\[LQ:.+?\]");
                foreach (Match item in regex.Matches(msg))
                {
                    msg = msg.Replace(item.Groups[0].Value, "");
                }
            }
            return msg;
        }

        /// <summary>
        /// 读配置项
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="TextLength"></param>
        /// <returns></returns>
        public static string IniReadValue(string filepath, string Section, string Key, string def = null, int TextLength = 65535)
        {
            StringBuilder temp = new StringBuilder(TextLength);
            GetPrivateProfileString(Section, Key, def, temp, TextLength, filepath);
            return temp.ToString();
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);


        /// <summary>
        /// 生成通讯ID
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Rid(int length = 12)
        {
            var result = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                var r = new Random(Guid.NewGuid().GetHashCode());
                result.Append(r.Next(0, 10));
            }
            return result.ToString();
        }

        /// <summary>
        /// 获取当前本地时间戳
        /// </summary>
        /// <returns></returns>      
        public static long GetCurrentTimeUnix()
        {
            TimeSpan cha = (DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)));
            long t = (long)cha.TotalSeconds;
            return t;
        }

        public static long ToUnixTimestamp(this DateTime time)
        {
            TimeSpan cha = (time - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)));
            var t = (long)cha.TotalSeconds;
            return t;
        }

        #endregion 通用
    }
}
