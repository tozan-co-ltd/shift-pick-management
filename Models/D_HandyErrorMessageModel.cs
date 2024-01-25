using X.PagedList;

namespace mar_sumaken_web.Models
{
    public class D_HandyErrorMessageModel
    {
        /// <summary>
        /// ハンディエラーメッセージID
        /// </summary>
        public int HandyErrorMessageID { set; get; }

        /// <summary>
        /// 読取日時
        /// </summary>
        public DateTime ReadingTime { set; get; }

        /// <summary>
        /// ハンディメニュー名
        /// </summary>
        public string HandyMenuName { set; get; }

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        public string? ErrorMessage { set; get; }

        // <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 登録者
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// ハンディエラーメッセージリスト
        /// </summary>
        public IPagedList<D_HandyErrorMessageModel> D_HandyErrorMessageList {set; get; }
    }
 
}