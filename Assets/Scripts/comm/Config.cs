using NKraken;
using NKraken.security;
using pb;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.comm
{
    public static class Config
    {
        /// <summary>
        /// 版本号
        /// |大版 1字节|资料片版 1字节|GAMMA版 2字节|BETA版 2字节|ALPHA版 2字节|
        /// </summary>
        private const ulong _VERSION = 0x0000000000000001;

        private static readonly byte[] _DEVICE_CODE = MD5.Format(Encoding.UTF8.GetBytes(SystemInfo.deviceUniqueIdentifier));

        public const int OS_TYPE = 0;

        public static ulong ClientVersion
        {
            get
            {
                return BigEndian.Uint64(BitConverter.GetBytes(_VERSION));
            }
        }

        public static byte[] DeviceCode
        {
            get
            {
                return _DEVICE_CODE;
            }
        }

        public static UserSession UserSession { get; set; }
    }
}


