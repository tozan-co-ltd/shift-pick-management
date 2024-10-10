using X.PagedList;

namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// トップ画面のModel
    /// </summary>
    public class TopModel : CommonModel
    { 
        // 道路検出レコードID
        public int LoadDetectRecordID {  get; set; }
        // ステーションID
        public int StationID {  get; set; }
        // 道路クラス
        public int LoadClass {  get; set; }
        // 作成日時
        public DateTime CreatedAt { get; set; }
        // 画像パス
        public string? ImagePath {  get; set; }
        // 車両の存在有無
        public bool TruckExist {  get; set; }
        // トップ画面モデルリスト
        public List<TopModel>? TopModelList { get; set; }
        // トラックの状況
        public string? TruckStatus {  get; set; }
    }
}
