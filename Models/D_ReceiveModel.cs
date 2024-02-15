using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 入荷予定照会テーブルのModel
    /// </summary>
    public class D_ReceiveModel : CommonModel
    {
        /// <summary>
        /// 入荷予定照会リスト
        /// </summary>
        public IPagedList<D_ReceiveModel> D_ReceiveModelList { get; set; }

        /// <summary>
        /// 検索入荷日(開始)
        /// </summary>
        [Display(Name = "入荷日")]
        public string SearchStartDate { get; set; }

        /// <summary>
        /// 検索入荷日(終了)
        /// </summary>
        public string SearchEndDate { get; set; }

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
        /// 選択された倉庫ID
        /// </summary>
        public int SelectedDepoID { get; set; }

        /// <summary>
        /// 検索会社リスト
        /// </summary>
        public IEnumerable<SelectListItem> SearchCompanyList
        {
            get
            {
                return MCompanyList;
            }
        }

        /// <summary>
        /// 選択された会社ID
        /// </summary>
        public int SelectedCompanyID { get; set; }

        /// <summary>
        /// 初期値設定
        /// </summary>
        public D_ReceiveModel()
        {
            // 現在日
            var now = DateTime.Today.ToString("yyyy/MM/dd");
            SearchStartDate = now;
            SearchEndDate = now;
        }
    }
}
