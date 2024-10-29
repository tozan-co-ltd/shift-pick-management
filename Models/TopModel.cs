using X.PagedList;

namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// トップ画面のModel
    /// </summary>
    public class TopModel : CommonModel
    { 
        // ステーションID
        public int StationID {  get; set; }
        // 荷量クラス
        public int LoadClass {  get; set; }
        // base64画像
        public string ImageBase64 { get; set; }
        // 更新日時
        public DateTime UpdatedAt { get; set; }
        // 車両の存在有無
        public bool TruckExist {  get; set; }
        // トップ画面モデルリスト
        public List<TopModel>? TopModelList { get; set; }
        // トラックの状況
        public string? TruckStatus {  get; set; }
    }
}
