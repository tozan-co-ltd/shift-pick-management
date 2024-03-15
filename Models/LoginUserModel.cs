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
        public int CompanyID { get; set; }

        /// <summary>
        /// 会社コード
        /// </summary>
        public String CompanyCode { get; set; }

        /// <summary>
        /// 会社名
        /// </summary>
        public String CompanyName { get; set; }

        /// <summary>
        /// データベース名
        /// </summary>
        public String DatabaseName { get; set; }

        /// <summary>
        /// ユーザーID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// ユーザー名
        /// </summary>
        public String UserName { get; set; }

        /// <summary>
        /// ロール
        /// </summary>
        public int Role { get; set; }

        /// <summary>
        /// メイン倉庫ID
        /// </summary>
        public int MainDepoID { get; set; }

        /// <summary>
        /// メイン倉庫名
        /// </summary>
        public String MainDepoName { get; set; }

        /// <summary>
        /// 管理権限区分
        /// </summary>
        public int AuthorizedKubun { get; set; }

        /// <summary>
        /// タイムスタンプ
        /// </summary>
        public DateTime TimeStamp { get; set; }
    }
}
