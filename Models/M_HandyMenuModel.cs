namespace mar_sumaken_web.Models
{
    public class M_HandyMenuModel
    {
        /// <summary>
        /// ハンディメニューID
        /// </summary>
        public string HandyMenuID { set; get; }

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
        public string IsDeleted { set; get; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public string CreatedAt { set; get; }

        /// <summary>
        /// 作成者
        /// </summary>
        public string CreatedBy { set; get; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public string UpdatedAt { set; get; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string UpdatedBy { set; get; }
    }
}
