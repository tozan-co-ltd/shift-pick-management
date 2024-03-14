using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 出荷指示照会のModel
    /// </summary>
    public class D_ShipmentScheduleSearchModel : CommonModel
    {
        /// <summary>
        /// 検索納入指示日(開始)
        /// </summary>
        [Display(Name = "納入指示日")]
        public string SearchStartDate { get; set; }

        /// <summary>
        /// 検索納入指示日(終了)
        /// </summary>
        public string SearchEndDate { get; set; }

        /// <summary>
        /// 検索便リスト
        /// </summary>
        public List<SelectListItem>? BinList { get; set; }

        /// <summary>
        /// 便リスト
        /// </summary>
        public List<int>? BinListInt { get; set; }

        /// <summary>
        /// 実績数不一致のみ
        /// </summary>
        public bool DifferenceCountCheck { get; set; }

        /// <summary>
        /// 検索倉庫リスト
        /// </summary>
        public IEnumerable<SelectListItem>? SearchDepoList { get; set; }

        /// <summary>
        /// 検索会社リスト
        /// </summary>
        public IEnumerable<SelectListItem>? SearchCompanyList { get; set; }

        /// <summary>
        /// 選択された会社ID
        /// </summary>
        public int SelectedCompanyID { get; set; }

        /// <summary>
        /// 出荷指示ID
        /// </summary>
        [Display(Name = "ID")]
        public int ShipmentScheduleID { get; set; }

        /// <summary>
        /// 納入先名
        /// </summary>
        [Display(Name = "納入先名")]
        public string? DeliveryName { set; get; }

        /// <summary>
        /// 納入指示日
        /// </summary>
        [Display(Name = "納入指示日")]
        public string? DeliveryDate { get; set; }

        /// <summary>
        /// 便
        /// </summary>
        [Display(Name = "便")]
        public string? DeliveryTimeClass { get; set; }

        /// <summary>
        /// 納入先品番
        /// </summary>
        [Display(Name = "納入先品番")]
        public string? DeliveryProductNumber { get; set; }

        /// <summary>
        /// 仕入先品番
        /// </summary>
        [Display(Name = "仕入先品番")]
        public string? SupplierProductNumber { get; set; }

        /// <summary>
        /// 収容数
        /// </summary>
        [Display(Name = "収容数")]
        public string? LotQuantity { get; set; }

        /// <summary>
        /// 箱数
        /// </summary>
        [Display(Name = "箱数")]
        public int NumberOfBoxes { get; set; }

        /// <summary>
        /// 納入指示数
        /// </summary>
        [Display(Name = "納入指示数")]
        public string? Quantity { get; set; }
    }
}
