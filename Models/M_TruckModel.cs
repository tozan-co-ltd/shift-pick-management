using ai_truck_load_measurement.Properties;
using System.ComponentModel.DataAnnotations;
using X.PagedList;

namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// 車両マスターのモデル
    /// </summary>
    public class M_TruckModel: CommonModel
    {
        /// <summary>
        /// 倉庫リスト
        /// </summary>
        public IPagedList<M_TruckModel>? M_TruckList { get; set; }

        /// <summary>
        /// 車両ID
        /// </summary>
        [Display(Name="車両ID")]
        public int TruckID {  get; set; }

        /// <summary>
        /// 車両番号
        /// </summary>
        [Display(Name="車両番号")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [RegularExpression(@"^[0-9]{4,4}$", ErrorMessage ="4桁の半角数字で入力してください。")]
        public int TruckNumber {  get; set; }

        /// <summary>
        /// 識別番号
        /// </summary>
        [Display(Name = "識別番号")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [RegularExpression(@"^[0-9]{4,4}$", ErrorMessage = "4桁の半角数字で入力してください。")]
        public int IdentifyNumber {  get; set; }

        /// <summary>
        /// 削除フラグ
        /// </summary>
        [Display(Name="削除フラグ")]
        public bool IsDeleted {  get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        [Display(Name = "作成日時")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        [Display(Name = "作成者")]
        public string? CreatedBy {  get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        [Display(Name = "更新日時")]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        [Display(Name = "更新者")]
        public string? UpdatedBy { get; set; }
    }
}
