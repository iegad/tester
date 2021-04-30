using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class Log
    {
        private Log() { }

        public static void Debug(params object[] args)
        {
            StackTrace st = new StackTrace(1, true);
            int line = st.GetFrame(0).GetFileLineNumber();
            string file = st.GetFrame(0).GetFileName();

            UnityEngine.Debug.Log(string.Format("[<color=#000>{0}</color>:<color=#000>{1}</color>] {2}", file, line, getContext(args)));
        }

        public static void Warn(params object[] args)
        {
            StackTrace st = new StackTrace(1, true);
            int line = st.GetFrame(0).GetFileLineNumber();
            string file = st.GetFrame(0).GetFileName();

            UnityEngine.Debug.LogWarning(string.Format("[<color=#000>{0}</color>:<color=#000>{1}</color>] {2}", file, line, getContext(args)));
        }

        public static void Error(params object[] args)
        {
            StackTrace st = new StackTrace(1, true);
            int line = st.GetFrame(0).GetFileLineNumber();
            string file = st.GetFrame(0).GetFileName();

            UnityEngine.Debug.LogError(string.Format("[<color=#000>{0}</color>:<color=#000>{1}</color>] {2}", file, line, getContext(args)));
        }

        private static string getContext(params object[] args)
        {
            string context = "";

            if (args.Length == 1)
                context = args[0].ToString();
            else if (args.Length > 1)
            {
                if (args[0].GetType() != typeof(string))
                    throw new Exception("args[0] must be string");

                object[] arr = new object[args.Length - 1];

                for (int i = 1, n = args.Length; i < n; i++)
                    arr[i - 1] = args[i];

                context = string.Format(args[0].ToString(), arr);
            }

            return context;
        }
    }
}
