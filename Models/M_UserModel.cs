using X.PagedList;

namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// ユーザーマスターのモデル
    /// </summary>
    public class M_UserModel : CommonModel
    {
        /// <summary>
        /// ユーザーリスト
        /// </summary>
        public IPagedList<M_UserModel>? M_UserList { get; set; }

        /// <summary>
        /// ユーザーID
        /// </summary>
        public int? UserID { get; set; }

        /// <summary>
        /// ユーザー名
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// ユーザーのActiveDirectory名
        /// </summary>
        public string? ADName {  get; set; }

        /// <summary>
        /// メインデポID
        /// </summary>
        public string? DepoID {  get; set; }

        /// <summary>
        /// メインデポ名
        /// </summary>
        public string? DepoName { get; set; }

        /// <summary>
        /// 管理権限区分
        /// </summary>
        public int? AuthorizedKubun {  get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        public string? CreatedBy {  get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime? UpdatedAt {  get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string? UpdatedBy {  get; set; }
    }
}
