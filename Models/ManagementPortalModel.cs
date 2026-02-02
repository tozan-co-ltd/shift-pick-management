namespace ai_truck_load_measurement.Models
{
    public class ManagementPortalModel : CommonModel
    {
        /// <summary>
        /// 便リスト
        /// </summary>
        public List<M_TripModel> Trips { get; set; }
    }

    public class CountNonTripNameRemarkModel : CommonModel
    {
        /// <summary>
        /// 紐づけ切れ原因
        /// </summary>
        public string UnlinkedReasonName { get; set; }

        /// <summary>
        /// 紐づけ切れ原因ID
        /// </summary>
        public int UnlinkedReasonID { get; set; }

        /// <summary>
        /// 原因毎の発生数
        /// </summary>
        public int UnlinkedReasonCount { get; set; }

        /// <summary>
        /// 全体における原因の比率
        /// </summary>
        public double RemarkPercentage { get; set; }

        /// <summary>
        /// デポID
        /// </summary>
        public int DepoID { get; set; }

        /// <summary>
        /// デポ名
        /// </summary>
        public string DepoName { get; set; }
    }

   
}
