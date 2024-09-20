namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// ファイル出力用の検索状態モデル
    /// </summary>
    public class SearchConditionModel
    {
        /// <summary>
        /// 出荷指示ID
        /// </summary>
        public int ShipmentScheduleID { get; set; }

        /// <summary>
        /// 倉庫ID
        /// </summary>
        public int DepoID { get; set; }

        /// <summary>
        /// 倉庫名
        /// </summary>
        public string? DepoName { get; set; }

        /// <summary>
        /// 会社ID
        /// </summary>
        public int CompanyID { get; set; }

        /// <summary>
        /// 会社名
        /// </summary>
        public string? CompanyName { get; set; }

        /// <summary>
        /// 入庫日（開始）
        /// </summary>
        public string? SearchStartDate { get; set; }

        /// <summary>
        /// 入庫日（終了）
        /// </summary>
        public string? SearchEndDate { get; set; }

        /// <summary>
        /// 便リスト
        /// </summary>
        public List<int>? BinList { get; set; }

        /// <summary>
        /// 実績数不一致のみ
        /// </summary>
        public bool DiffenceCountCheck { get; set; }

        /// <summary>
        /// 仕入先品番
        /// </summary>
        public string SupplierProductNumber { get; set; }

    }
}
