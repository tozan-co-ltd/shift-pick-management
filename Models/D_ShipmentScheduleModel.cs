using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 出荷指示テーブルModel
    /// </summary>
    public class D_ShipmentScheduleModel
    {
        public class D_ShipmentSchedule : CommonModel
        {
            /// <summary>
            /// 出荷指示リスト
            /// </summary>
            public List<D_ShipmentSchedule> D_ShipmentScheduleList { get; set; }

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
            /// 選択された会社ID
            /// </summary>
            public int SelectedCompanyID { get; set; }

            /// <summary>
            /// 出荷指示実績ID
            /// </summary>
            public int ShipmentScheduleID { get; set; }

            /// <summary>
            /// 発注元
            /// </summary>
            public string OrdererCode { get; set; }

            /// <summary>
            /// 発注元工区
            /// </summary>
            public string OrdererFactoryKubun { get; set; }

            /// <summary>
            /// 発注元名称
            /// </summary>
            public string OrdererName { get; set; }

            /// <summary>
            /// 更新日時
            /// </summary>
            public DateTime UpdatedAt { get; set; }

            /// <summary>
            /// 更新者
            /// </summary>
            public string UpdatedBy { get; set; } = string.Empty;
        }
    }
}
