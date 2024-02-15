using mar_sumaken_web.Properties;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 出荷指示テーブルのModel
    /// </summary>
    public class D_ShipmentScheduleModel : CommonModel
    {
        /// <summary>
        /// 出荷指示リスト
        /// </summary>
        public IPagedList<D_ShipmentScheduleModel> D_ShipmentScheduleList { get; set; }

        /// <summary>
        /// 出庫日
        /// </summary>
        [Display(Name = "入荷日")]
        public string DateSearchStart { get; set; }

        /// <summary>
        /// 出庫日(終了)
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
        /// 出荷指示実績ID
        /// </summary>
        public int ShipmentScheduleID { get; set; }

        /// <summary>
        /// 発注元
        /// </summary>
        [DisplayName("発注元")]
        public string OrdererCode { get; set; } = string.Empty;

        /// <summary>
        /// 発注元工区
        /// </summary>
        [DisplayName("発注元工区")]
        public string OrdererFactoryKubun { get; set; } = string.Empty;

        /// <summary>
        /// 発注元名称
        /// </summary>
        [DisplayName("発注元名称")]
        public string OrdererName { get; set; } = string.Empty;

        /// <summary>
        /// 発注元工場名
        /// </summary>
        [DisplayName("発注元工場名")]
        public string OrdererFactoryName { get; set; } = string.Empty;

        /// <summary>
        /// 出荷元
        /// </summary>
        [DisplayName("出荷元")]
        public string ShipperCode { get; set; } = string.Empty;

        /// <summary>
        /// 出荷元工区
        /// </summary>
        [DisplayName("出荷元工区")]
        public string ShipperFactoryKubun { get; set; } = string.Empty;

        /// <summary>
        /// 出荷元名称
        /// </summary>
        [DisplayName("出荷元名称")]
        public string ShipperName { get; set; } = string.Empty;

        /// <summary>
        /// 納入先
        /// </summary>
        [DisplayName("納入先")]
        public string DeliveryCode { get; set; } = string.Empty;

        /// <summary>
        /// 納入先工区
        /// </summary>
        [DisplayName("納入先工区")]
        public string DeliveryFactoryKubun { get; set; } = string.Empty;

        /// <summary>
        /// 納入場所
        /// </summary>
        [DisplayName("納入場所")]
        public string DeliveryLocation { get; set; } = string.Empty;

        /// <summary>
        /// 納入先名称
        /// </summary>
        [DisplayName("納入先名称")]
        public string DeliveryName { get; set; } = string.Empty;

        /// <summary>
        /// 納入先工場名
        /// </summary>
        [DisplayName("納入先工場名")]
        public string DeliveryFactoryName { get; set; } = string.Empty;

        /// <summary>
        /// 定期／不定期区分名称
        /// </summary>
        [DisplayName("定期／不定期区分名称")]
        public string RegularKubun { get; set; } = string.Empty;

        /// <summary>
        /// 発行日
        /// </summary>
        [DisplayName("発行日")]
        [DataType(DataType.Date)]
        public DateTime IssuedDate { get; set; }

        /// <summary>
        /// 納入指示日
        /// </summary>
        [DisplayName("納入指示日")]
        public DateTime DeliveryDate { get; set; }

        /// <summary>
        /// 納入指示時刻
        /// </summary>
        [DisplayName("納入指示時刻")]
        public string DeliveryTime { get; set; } = string.Empty;

        /// <summary>
        /// 便
        /// </summary>
        [DisplayName("便")]
        public int DeliveryTimeClass { get; set; }

        /// <summary>
        /// 輸送識別
        /// </summary>
        [DisplayName("輸送識別")]
        public string TranspotationIdentify { get; set; } = string.Empty;

        /// <summary>
        /// 納品書番号
        /// </summary>
        [DisplayName("納品書番号")]
        public string DeliverySlipNumber { get; set; } = string.Empty;

        /// <summary>
        /// ページ数
        /// </summary>
        [DisplayName("ページ数")]
        public int DeliverySlipPageNumber { get; set; }

        /// <summary>
        /// 行No
        /// </summary>
        [DisplayName("行No")]
        public int DeliverySlipRowNumber { get; set; }

        /// <summary>
        /// 納入先品番(表示用品番)
        /// </summary>
        [DisplayName("表示用品番")]
        public string DeliveryProductNumber { get; set; } = string.Empty;

        /// <summary>
        /// 背番号
        /// </summary>
        [DisplayName("背番号")]
        public string DeliveryProductAbbreviation { get; set; } = string.Empty;

        /// <summary>
        /// 品名
        /// </summary>
        [DisplayName("品名")]
        public string DeliveryProductName { get; set; } = string.Empty;

        /// <summary>
        /// 収容数
        /// </summary>
        [DisplayName("収容数")]
        public int LotQuantity { get; set; }

        /// <summary>
        /// 枝番
        /// </summary>
        [DisplayName("枝番")]
        public int BranchNumber { get; set; }

        /// <summary>
        /// 納入指示数
        /// </summary>
        [DisplayName("納入指示数")]
        public int Quantity { get; set; }

        /// <summary>
        /// 仕入先品番
        /// </summary>
        [DisplayName("仕入先品番")]
        public string SupplierProductNumber { get; set; } = string.Empty;

        /// <summary>
        /// 箱数
        /// </summary>
        [DisplayName("箱数")]
        public int NumberOfBoxes { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        /// 初期値設定
        /// </summary>
        public D_ShipmentScheduleModel()
        {
            // 現在日
            var now = DateTime.Today.AddDays(+1).ToString("yyyy/MM/dd");
            DateSearchStart = now;
            DateSearchEnd = now;
        }
    }
}
