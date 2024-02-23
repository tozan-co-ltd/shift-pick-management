using X.PagedList;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// ハンディエラーメッセージ実績テーブルのModel
    /// </summary>
    public class D_HandyErrorMessageModel
    {
        /// <summary>
        /// ハンディエラーメッセージ実績リスト
        /// </summary>
        public IPagedList<D_HandyErrorMessageModel> D_HandyErrorMessageList { set; get; }

        /// <summary>
        /// ハンディエラーメッセージID
        /// </summary>
        public int HandyErrorMessageID { get; set; }

        /// <summary>
        /// ハンディメニュー名
        /// </summary>
        public string HandyMenuName { get; set; }

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        public string? ErrorMessage { get; set; }

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