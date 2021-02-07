using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Native.Csharp.App
{
    /// <summary>
    /// 请在这里填写应用的信息
    /// </summary>
    class AppInfo
    {
        /// <summary>
        /// 固定 不要改
        /// </summary>
        public readonly int apiVer = 6;
        /// <summary>
        /// 应用ID  1.尽量唯一  2.编译出来需要以 .lq.dll 结尾 方便框架识别
        /// </summary>
        public readonly string appId = "com.demo.ss";
        /// <summary>
        /// 应用名称
        /// </summary>
        public readonly string name = "demo";
        /// <summary>
        /// 应用版本
        /// </summary>
        public readonly string ver = "1.0.0";
        /// <summary>
        /// 开发者ID ‘预留  随便填
        /// </summary>
        public readonly string authkey = "123456";
        /// <summary>
        /// 作者名字
        /// </summary>
        public readonly string author = "author";
        /// <summary>
        /// 应用简介
        /// </summary>
        public readonly string description = "demo";
        /// <summary>
        /// 应用详情显示的 URL
        /// </summary>
        public readonly string url = "http://www.baidu.com";
        /// <summary>
        /// 0 表示不需要取 cookies，需要请改为 1
        /// </summary>
        public readonly int ck = 0;

        public static long self = 0;
    }
}
