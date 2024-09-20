using X.PagedList;

namespace  ai_truck_load_measurement.Models
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
        /// 倉庫名
        /// </summary>
        public string? DepoName { get; set; }

        /// <summary>
        /// 翌日グラフ情報
        /// </summary>
        public GraphInfo NextDayGraph { get; set; }

        /// <summary>
        /// 翌々日グラフ情報
        /// </summary>
        public GraphInfo NextTwoDayGraph { get; set; }

        /// <summary>
        /// グラフ情報
        /// </summary>
        public class GraphInfo
        {
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
        }
    }
}
