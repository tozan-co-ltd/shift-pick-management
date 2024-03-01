using mar_sumaken_web.Properties;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 倉庫マスターのModel
    /// </summary>
    public class TopModel : CommonModel
    {
        /// <summary>
        /// 倉庫リスト
        /// </summary>
        public IPagedList<D_ShipmentScheduleModel> MyModel1 { get; set; }
        public IPagedList<D_HandyErrorMessageModel> MyModel2 { get; set; }
    }
}
