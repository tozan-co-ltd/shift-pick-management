using X.PagedList;

namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// トップ画面のModel
    /// </summary>
    public class TopModel : CommonModel
    { 
        // トップ画面モデルリスト
        public List<ViewCardModel>? ViewCardModelList { get; set; }
        // 現在のデポID
        public int MainDepoID {  get; set; }
        // 現在のデポ名
        public string? MainDepoName { get; set; }
    }

    /// <summary>
    /// カードレイアウトに必要な情報のモデル
    /// </summary>
    public class ViewCardModel : CommonModel
    {
        // ステーションID
        public int StationID { get; set; }
        // 荷量クラス
        public int LoadClass { get; set; }
        // base64画像
        public string ImageBase64 { get; set; }
        // 車両の存在有無
        public bool TruckExist { get; set; }
        // トラックの状況
        public string? TruckStatus { get; set; }
        // 画像のIPアドレス
        public string? IPAdress { get; set; }
    }
}
