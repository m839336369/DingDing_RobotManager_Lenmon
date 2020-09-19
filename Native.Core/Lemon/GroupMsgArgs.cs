﻿using System;

namespace Native.Csharp.App
{
    internal class GroupMsgArgs : EventArgs
    {
        public GroupMsgArgs(long a, long b, string c)
        {
            FromQQ = a;
            FromGroup = b;
            Msg = c;
        }
        /// <summary>
        /// 发送这条消息的人
        /// </summary>
        public long FromQQ { get; }
        /// <summary>
        /// 来源群组
        /// </summary>
        public long FromGroup { get; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Msg { get; }
        /// <summary>
        /// 快捷回复
        /// </summary>
        /// <param name="msg">消息内容</param>
        /// <param name="includeSrcMsg">是否引用原始消息，设为 true 相当于在 msg 开头加入 SrcMsg</param>
        public void Reply(string msg, bool includeSrcMsg = false) => Robot.Send.Group(FromGroup, msg);
        //TODO 撤回
    }
}