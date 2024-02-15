using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace mar_sumaken_web.Models
{
    /// <summary>
    ///  出荷指示照会テーブルのModel
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
            /// 選択された倉庫ID
            /// </summary>
            public int SelectedDepoID { get; set; }

            /// <summary>
            /// 選択された倉庫名
            /// </summary>
            public string SelectedDepoName { get; set; } = string.Empty;

            /// <summary>
            /// 選択された会社ID
            /// </summary>
            public int SelectedCompanyID { get; set; }

            /// <summary>
            /// 選択された会社名
            /// </summary>
            [Display(Name = "納入先名")]
            public string SelectedCompanyName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public List<string> LstErrorMsg { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IPagedList<D_ReceiveScheduleModel> LstD_ReceiveSchedule { get; set; }

        /// <summary>
        /// 初期値設定
        /// </summary>
        public D_ReceiveScheduleModel()
        {
            // 現在日
            var now = DateTime.Today.ToString("yyyy/MM/dd");
            // 1ヶ月前
            var oneWeeklater = DateTime.Today.AddDays(+7).ToString("yyyy/MM/dd");

            DateSearchStart = now;
            DateSearchEnd = oneWeeklater;
        }

    }
}
