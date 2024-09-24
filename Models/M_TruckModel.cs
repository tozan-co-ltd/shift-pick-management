namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// 車両マスターのモデル
    /// </summary>
    public class M_TruckModel: CommonModel
    {
        /// <summary>
        /// 車両ID
        /// </summary>
        public int TruckID {  get; set; }

        /// <summary>
        /// 車両番号
        /// </summary>
        public int TruckNumber {  get; set; }

        /// <summary>
        /// 識別番号
        /// </summary>
        public int IdentifyNumber {  get; set; }

        /// <summary>
        /// 削除フラグ
        /// </summary>
        public bool IsDeleted {  get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        public string CreatedBy {  get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string UpdatedBy { get; set; }
    }
}
