using System.ComponentModel.DataAnnotations;

namespace mar_sumaken_web.Models
{
    public class D_ShipmentModel : CommonModel
    {

        /// <summary>
        /// タイトル
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// 出荷実績リスト
        /// </summary>
        public List<D_ShipmentModel>? DShipmentList { get; set; }

        /// <summary>
        /// 出荷実績ID
        /// </summary>
        [Display(Name = "ID")]
        public int ShipmentID { get; set; }

        /// <summary>
        /// 出荷指示ID
        /// </summary>
        [Display(Name = "出荷指示ID")]
        public int ShipmentScheduleID { get; set; }

        /// <summary>
        /// 読取実績ID
        /// </summary>
        [Display(Name = "読取実績ID")]
        public int ScanResultID { get; set; }

        /// <summary>
        /// 倉庫ID
        /// </summary>
        [Display(Name = "倉庫ID")]
        public int DepoID { get; set; }

        /// <summary>
        /// 倉庫名
        /// </summary>
        [Display(Name = "倉庫名")]
        public string? DepoName { get; set; }

        /// <summary>
        /// 納入先ID
        /// </summary>
        [Display(Name = "納入先ID")]
        public int DeliveryID { get; set; }

        /// <summary>
        /// 納入先名
        /// </summary>
        [Display(Name = "納入先名")]
        public string? DeliveryName { get; set; }

        /// <summary>
        /// 納入指示日
        /// </summary>
        [Display(Name = "納入指示日")]
        public DateTime DeliveryDate { get; set; }

        /// <summary>
        /// 便
        /// </summary>
        [Display(Name = "便")]
        public int DeliveryTimeClass { get; set; }

        /// <summary>
        /// 出荷日
        /// </summary>
        [Display(Name = "出荷日")]
        public DateTime ShipmentDatetime { get; set; }

        /// <summary>
        /// 納入先品番
        /// </summary>
        [Display(Name = "納入先品番")]
        public string? DeliveryProductNumber { get; set; }

        /// <summary>
        /// 背番号
        /// </summary>
        [Display(Name = "背番号")]
        public string? DeliveryProductAbbreviation { get; set; }

        /// <summary>
        /// シリアル番号(かんばん識別番号)
        /// </summary>
        [Display(Name = "シリアル番号(かんばん識別番号)")]
        public int KanbanSerialNumber { get; set; }

        /// <summary>
        /// 納入場所
        /// </summary>
        [Display(Name = "納入場所")]
        public string? DeliveryLocation { get; set; }

        /// <summary>
        /// 納品書番号
        /// </summary>
        [Display(Name = "納品書番号")]
        public string? DeliverySlipNumber { get; set; }

        /// <summary>
        /// 仕入先品番
        /// </summary>
        [Display(Name = "仕入先品番")]
        public string? SupplierProductNumber { get; set; }

        /// <summary>
        /// ロット番号
        /// </summary>
        [Display(Name = "ロット番号")]
        public string? LotNumber { get; set; }

        /// <summary>
        /// メインキー
        /// </summary>
        [Display(Name = "メインキー")]
        public string? MainProductKey { get; set; }

        /// <summary>
        /// サブキー1
        /// </summary>
        [Display(Name = "サブキー1")]
        public string? FirstSubProductKey { get; set; }

        /// <summary>
        /// サブキー2
        /// </summary>
        [Display(Name = "サブキー2")]
        public string? SecondSubProductKey { get; set; }

        /// <summary>
        /// 箱数
        /// </summary>
        [Display(Name = "箱数")]
        public int NumberOfBoxes { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        [Display(Name = "数量")]
        public int Quantity { get; set; }

        /// <summary>
        /// 読取日時
        /// </summary>
        [Display(Name = "読取日時")]
        public string? ScanedAt { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        [Display(Name = "作成日時")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        [Display(Name = "作成者")]
        public string? CreatedBy { get; set; }

        //--------------------------------D_ScanResult--------------------------------
        /// <summary>
        /// ハンディメニューID
        /// </summary>
        [Display(Name = "ハンディメニューID")]
        public int HandyMenuID { get; set; }


        /// <summary>
        /// ハンディメニュー名
        /// </summary>
        [Display(Name = "ハンディメニュー名")]
        public string? HandyMenuName { get; set; }

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
        public string? FirstScanedString { get; set; }

        /// <summary>
        /// スキャン文字列2
        /// </summary>
        [Display(Name = "スキャン文字列2")]
        public string? SecondScanedString { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        [Display(Name = "作成日時")]
        public DateTime ScanCreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        [Display(Name = "作成者")]
        public string? ScanCreatedBy { get; set; }
    }
}
