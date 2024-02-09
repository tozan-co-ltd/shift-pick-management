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
        public int HandyMenuID { set; get; }

        /// <summary>
        /// ソート番号
        /// </summary>
        public int SortNumber { set; get; }

        /// <summary>
        /// ハンディメニュー名
        /// </summary>
        public string HandyMenuName { set; get; }

        /// <summary>
        /// 未使用フラグ
        /// </summary>
        public bool IsDeleted { set; get; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { set; get; }

        /// <summary>
        /// 作成者
        /// </summary>
        public string CreatedBy { set; get; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { set; get; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string UpdatedBy { set; get; }
    }
}
