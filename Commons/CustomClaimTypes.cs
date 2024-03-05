using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mar_sumaken_web.Commons
{
    /// <summary>
    /// Claim情報
    /// </summary>
    public class CustomClaimTypes
    {
        /// <summary>
        /// データベース名
        /// </summary>
        public const string ClaimType_DatabaseName = "DatabaseName";

        /// <summary>
        /// 会社ID
        /// </summary>
        public const string ClaimType_CampanyID = "CompanyID";

        /// <summary>
        /// メイン倉庫ID
        /// </summary>
        public const string ClaimType_MainDepoID = "MainDepoID";

        /// <summary>
        /// ユーザーID
        /// </summary>
        public const string ClaimType_UserID = "UserID";

        /// <summary>
        /// ロール
        /// </summary>
        public const string ClaimType_Role = "Role";

        /// <summary>
        /// 管理権限区分
        /// </summary>
        public const string ClaimType_AuthorizedKubun = "AuthorizedKubun";

        /// <summary>
        /// タイムスタンプ(ログイン日時)
        /// </summary>
        public const string ClaimType_TimeStamp = "TimeStamp";
    }
}
