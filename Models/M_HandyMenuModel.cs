using System.ComponentModel.DataAnnotations;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// ハンディメニューマスターのModel
    /// </summary>
    public class M_HandyMenuModel : CommonModel
    {
        /// <summary>
        /// ハンディメニューID
        /// </summary>
        public int HandyMenuID { get; set; }

        /// <summary>
        /// ソート番号
        /// </summary>
        public int SortNumber { get; set; }

        /// <summary>
        /// ハンディメニュー名
        /// </summary>
        public string HandyMenuName { get; set; }

        /// <summary>
        /// 未使用フラグ
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string UpdatedBy { get; set; }
    }
}
