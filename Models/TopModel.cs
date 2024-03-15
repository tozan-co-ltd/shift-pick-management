using X.PagedList;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 倉庫マスターのModel
    /// </summary>
    public class TopModel : CommonModel
    {
        /// <summary>
        /// 出荷指示リスト
        /// </summary>
        public IPagedList<D_ShipmentScheduleModel> ShipmentScheduleList { get; set; }

        /// <summary>
        /// ハンディエラーメッセージリスト
        /// </summary>
        public IPagedList<D_HandyErrorMessageModel> D_HandyErrorMessageList { get; set; }

        /// <summary>
        /// 指示箱数
        /// </summary>
        public int ShipmentScheduleTotal { get; set; } = 0;

        /// <summary>
        /// 出庫箱数
        /// </summary>
        public int StoreOutTotal { get; set; } = 0;

        /// <summary>
        /// グラフのタイトル
        /// </summary>
        public string? GraphTitle { get; set; } = "";

        /// <summary>
        /// 倉庫名
        /// </summary>
        public string? DepoName { get; set; }
    }
}
