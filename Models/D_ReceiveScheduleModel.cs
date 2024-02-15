using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 入荷予定テーブルのModel
    /// </summary>
    public class D_ReceiveScheduleModel : CommonModel
    {
        /// <summary>
        /// 入荷予定日
        /// </summary>
        [Display(Name = "入荷予定日")]
        public string DateSearchStart { get; set; }

        /// <summary>
        /// 入荷予定日(終了)
        /// </summary>
        public string DateSearchEnd { get; set; }

        /// <summary>
        /// 検索倉庫リスト
        /// </summary>
        public IEnumerable<SelectListItem> SearchDepoList
        {
            get
            {
                return MDepoList;
            }
        }

        /// <summary>
        /// 取込ファイル名
        /// </summary>
        public string? ImportFileName { get; set; }

        /// <summary>
            /// 選択された倉庫ID
            /// </summary>
            public int SelectedDepoID { get; set; }

            /// <summary>
        /// 選択された会社ID
            /// </summary>
        public int SelectedCompanyID { get; set; }

            /// <summary>
        /// 会社コード
            /// </summary>
        [Display(Name = "会社コード")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [RegularExpression(@"[0-9]+")]
        public string? CompanyCode { get; set; }

            /// <summary>
        /// 入荷予定ID
            /// </summary>
        [Display(Name = "入荷予定ID")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int ReceiveScheduleID { get; set; }

        /// <summary>
        /// 入荷予定日
        /// </summary>
        public List<string> LstErrorMsg { set; get; }

        /// <summary>
        /// 仕入先品番
        /// </summary>
        public string Message { set; get; }

        /// <summary>
        /// ロット番号
        /// </summary>
        public IPagedList<D_ReceiveScheduleModel> LstD_ReceiveSchedule { set; get; }

        /// <summary>
        /// 数量
        /// </summary>
        [Display(Name = "数量")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string Quantity { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string? UpdatedBy { get; set; }
    }
}
