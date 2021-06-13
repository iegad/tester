using Assets.Scripts.comm;
using pb;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using System;

namespace Assets.Scripts.Scenes
{
    public partial class BaseScene : MonoBehaviour
    {
        protected virtual int UserLogin(string account, string vcode)
        {
            if (account == null || account.Length == 0 || account.Length > 50)
                return -1;

            if (vcode == null || vcode.Length != 5)
                return -2;

            UserLoginReq req = new UserLoginReq
            {
                VCode = vcode,
                ClientVer = (long)Config.ClientVersion,
                OSType = Config.OS_TYPE,
                TermInfo = Config.TerminatorInfo
            };

            if (ulong.TryParse(account, out _))
                req.PhoneNum = account;
            else
                req.Email = account;

            try
            {
                if (Hydra.Sphinx == null)
                    return -100;

                SendPackage(Seq, PackageID.PidUserDelivery, MessageID.MidUserLoginReq, req.ToByteString(), Hydra.Sphinx);
                return 0;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            return -1000;
        }
    }
}


