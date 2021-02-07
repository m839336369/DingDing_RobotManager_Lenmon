using System;

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
        //TODO 撤回
    }
}