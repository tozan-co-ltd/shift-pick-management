namespace mar_sumaken_web.Models
{
    /// <summary>
    /// ログイン中ユーザーモデル
    /// </summary>
    public class LoginUserModel
    {
        /// <summary>
        /// 会社ID
        /// </summary>
        public int CompanyID { set; get; }

        /// <summary>
        /// 会社コード
        /// </summary>
        public String CompanyCode { set; get; }

        /// <summary>
        /// 会社名
        /// </summary>
        public String CompanyName { set; get; }

        /// <summary>
        /// データベース名
        /// </summary>
        public String DatabaseName { set; get; }

        /// <summary>
        /// ユーザーID
        /// </summary>
        public int UserID { set; get; }

        /// <summary>
        /// ユーザー名
        /// </summary>
        public String UserName { set; get; }

        /// <summary>
        /// ロール
        /// </summary>
        public int Role { set; get; }

        /// <summary>
        /// メイン倉庫ID
        /// </summary>
        public int MainDepoID { set; get; }

        /// <summary>
        /// メイン倉庫名
        /// </summary>
        public String MainDepoName { set; get; }

        /// <summary>
        /// 管理権限区分
        /// </summary>
        public int AuthorizedKubun { set; get; }

        /// <summary>
        /// タイムスタンプ
        /// </summary>
        public DateTime TimeStamp { set; get; }
    }
}
