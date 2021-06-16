using Assets.Scripts.comm;
using Google.Protobuf;
using pb;
using System;
using UnityEngine;

namespace Assets.Scripts.Scenes
{
    public partial class BaseScene : MonoBehaviour
    {
        protected int UserLogin(long userID, string phoneNum, string email, ByteString token, ByteString deviceCode)
        {
            if (Hydra.Sphinx == null)
                return -100;

            if (userID <= 0)
                return -1;

            if (phoneNum == null && email == null)
                return -2;

            if (token == null || token.Length != 16)
                return -3;

            if (deviceCode == null || deviceCode.Length != 16)
                return -4;

            UserLoginReq req = new UserLoginReq
            {
                ClientVer = (long)Config.ClientVersion,
                OSType = Config.OS_TYPE,
                DeviceCode = deviceCode,
                Token = token,
                UserID = userID,
                PhoneNum = phoneNum,
                Email = email
            };

            try
            {
                SendPackage(PackageID.PidUserDelivery, MessageID.MidUserLoginReq, req.ToByteString(), Hydra.Sphinx);
                return 0;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            return -1000;
        }

        protected int UserLogin(string account, string vcode)
        {
            if (Hydra.Sphinx == null)
                return -100;

            if (account == null || account.Length == 0 || account.Length > 50)
                return -1;

            if (vcode == null || vcode.Length != 5)
                return -2;

            UserLoginReq req = new UserLoginReq
            {
                VCode = vcode,
                ClientVer = (long)Config.ClientVersion,
                OSType = Config.OS_TYPE,
                DeviceCode = ByteString.CopyFrom(Config.DeviceCode)
            };

            if (ulong.TryParse(account, out _))
                req.PhoneNum = account;
            else
                req.Email = account;

            try
            {
                SendPackage(PackageID.PidUserDelivery, MessageID.MidUserLoginReq, req.ToByteString(), Hydra.Sphinx);
                return 0;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            return -1000;
        }

        protected virtual int UserLoginHandle(UserLoginRsp rsp)
        {
            throw new NotImplementedException();
        }

        protected int UserUnregist(string account, string vcode)
        {
            if (Hydra.Sphinx == null)
                return -100;

            if (account == null || account.Length == 0 || account.Length > 50)
                return -1;

            if (vcode == null || vcode.Length != 5)
                return -5;

            UserUnregistReq req = new UserUnregistReq()
            {
                VCode = vcode
            };

            if (ulong.TryParse(account, out _))
                req.PhoneNum = account;
            else
                req.Email = account;

            try
            {
                SendPackage(PackageID.PidUserDelivery, MessageID.MidUserUnregistReq, req.ToByteString(), Hydra.Sphinx);
                return 0;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            return -1000;
        }

        protected int SearchUserInfo(long userID, string account, long offset = 0, long limit = 1, string orderBy = "", bool desc = false)
        {
            if (Hydra.Sphinx == null)
                return -100;

            SearchUserInfoReq req = new SearchUserInfoReq
            {
                UserID = userID,
                Offset = offset,
                Limit = limit,
                OrderBy = orderBy,
                Desc = desc
            };

            if (account != null)
            {
                if (account.Length == 0 || account.Length > 50)
                    return -2;

                if (ulong.TryParse(account, out _))
                    req.PhoneNum = account;
                else
                    req.Email = account;
            }

            try
            {
                SendPackage(PackageID.PidUserDelivery, MessageID.MidSearchUserInfoReq, req.ToByteString(), Hydra.Sphinx);
                return 0;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }

            return -1000;
        }

        protected int ModifyUserInfo(UserInfo userInfo, byte[] token)
        {
            if (Hydra.Sphinx == null)
                return -100;

            if (userInfo == null)
                return -1;

            if (userInfo.UserID <= 0)
                return -2;

            if (token == null || token.Length != 36)
                return -3;

            ModifyUserInfoReq req = new ModifyUserInfoReq
            {
                UserInfo = userInfo,
                Token = ByteString.CopyFrom(token)
            };

            try
            {
                SendPackage(PackageID.PidUserDelivery, MessageID.MidModifyUserInfoReq, req.ToByteString(), Hydra.Sphinx);
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


