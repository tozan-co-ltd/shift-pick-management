namespace shift_pick_management.Models
{
    public class LoadDistributionModel : LoadRecordModel
    {
        // 到着荷量のデータ数
        public int ArrivalLoadClassCount { get; set; }
        // 出発荷量のデータ数
        public int DepartureLoadClassCount { get; set; }
        // 到着荷量のクラス、ステータス、データ数保存用リスト
        public List<LoadDistributionModel>? ArrivalLoadClasses { get; set; }
        // 出発荷量のクラス、ステータス、データ数保存用リスト
        public List<LoadDistributionModel>? DepartureLoadClasses { get; set; }
    }
}