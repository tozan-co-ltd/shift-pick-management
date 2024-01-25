using X.PagedList;

namespace mar_sumaken_web.Models
{
    public class D_HandyErrorMessageModel
    {
        /// <summary>
        /// 出荷指示取込エラー履歴id
        /// </summary>
        public int HandyErrorId { set; get; }

        /// <summary>
        /// 読取日時
        /// </summary>
        public DateTime ReadingTime { set; get; }

        /// <summary>
        /// ハンディメニュー名
        /// </summary>
        public string HandyMenuName { set; get; }

        /// <summary>
        /// エラーメッセージコード
        /// </summary>
        public string? ErrorCode { set; get; }

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        public string? ErrorMessage { set; get; }

        /// <summary>
        /// 登録者
        /// </summary>
        public string? CreatedBy { get; set; }

        // <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        public IPagedList<D_HandyErrorMessageModel> HandyErrorList {set; get; }//一覧画面のlistを取得と設定

        public string Message { set; get; }

    }
 
}