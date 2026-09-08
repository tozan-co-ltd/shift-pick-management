namespace shift_pick_management.Models
{
    /// <summary>
    /// ログイン中ユーザーモデル
    /// </summary>
    public class LoginUserModel
    {
        /// <summary>
        /// ユーザー名
        /// </summary>
        public String UserName { get; set; }
        /// <summary>
        /// ユーザーのAD名
        /// </summary>
        public String ADName {  get; set; }
        /// <summary>
        /// ユーザーの権限区分
        /// </summary>
        public int AuthorizedKubun {  get; set; }
        /// <summary>
        /// ユーザーのメインデポ
        /// </summary>
        public int MainDepoID {  get; set; }
        public string MainDepoName { get; set; }
    }
}
