using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace mar_sumaken_web.Models
{
    public class D_ShipmentScheduleSearchModel : CommonModel
    {
        public List<SelectListItem>? BinList { get; set; }

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
        /// 実績数不一致のみ
        /// </summary>
        public bool DiffenceCountCheck { get; set; }

        /// <summary>
        /// 検索倉庫リスト
        /// </summary>
        public IEnumerable<SelectListItem>? SearchDepoList { get; set; }

        /// <summary>
        /// 選択された倉庫ID
        /// </summary>
        public int SelectedDepoID { get; set; }

        /// <summary>
        /// 検索会社リスト
        /// </summary>
        public IEnumerable<SelectListItem>? SearchCompanyList { get; set; }

        /// <summary>
        /// 選択された会社ID
        /// </summary>
        public int SelectedCompanyID { get; set; }
    }
}
