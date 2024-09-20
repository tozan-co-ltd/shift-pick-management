using System.ComponentModel.DataAnnotations;

namespace  ai_truck_load_measurement.Models
{
    public class D_ScanResultModel : CommonModel
    {
        /// <summary>
        /// 読取実績ID
        /// </summary>
        [Display(Name = "ID")]
        public int ScanResultID { get; set; }

        /// <summary>
        /// 倉庫ID
        /// </summary>
        [Display(Name = "倉庫ID")]
        public int DepoID { get; set; }

        /// <summary>
        /// ハンディメニューID
        /// </summary>
        [Display(Name = "ハンディメニューID")]
        public int HandyMenuID { get; set; }

        /// <summary>
        /// 仕入先かんばんID
        /// </summary>
        [Display(Name = "仕入先かんばんID")]
        public int SupplierKanbanID { get; set; }

        /// <summary>
        /// 入力箱数
        /// </summary>
        [Display(Name = "入力箱数")]
        public int NumberOfInputBoxes { get; set; }

        /// <summary>
        /// スキャン文字列1
        /// </summary>
        [Display(Name = "スキャン文字列1")]
        public string FirstScanedString { get; set; }

        /// <summary>
        /// スキャン文字列2
        /// </summary>
        [Display(Name = "スキャン文字列2")]
        public string SecondScanedString { get; set; }

        /// <summary>
        /// 読取日時
        /// </summary>
        [Display(Name = "読取日時")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime ScanedAt { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        [Display(Name = "作成日時")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime ScanCreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        [Display(Name = "作成者")]
        public string ScanCreatedBy { get; set; }
    }
}
