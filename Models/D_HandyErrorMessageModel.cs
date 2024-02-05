namespace mar_sumaken_web.Models
{
    /// <summary>
    /// ハンディエラーメッセージ実績テーブルのModel
    /// </summary>
    public class D_HandyErrorMessageModel
    {
        public class D_HandyErrorMessage : CommonModel
        {
            /// <summary>
            /// ハンディエラーメッセージ実績リスト
            /// </summary>
            public List<D_HandyErrorMessage> D_HandyErrorMessageList { get; set; }

            /// <summary>
            /// ハンディエラーメッセージID
            /// </summary>
            public int HandyErrorMessageID { set; get; }

            /// <summary>
            /// ハンディメニュー名
            /// </summary>
            public string HandyMenuName { set; get; }

            /// <summary>
            /// エラーメッセージ
            /// </summary>
            public string? ErrorMessage { set; get; }

            // <summary>
            /// 読取日時
            /// </summary>
            public DateTime CreatedAt { get; set; }

            /// <summary>
            /// 登録者
            /// </summary>
            public string? CreatedBy { get; set; }
        }
    }
}