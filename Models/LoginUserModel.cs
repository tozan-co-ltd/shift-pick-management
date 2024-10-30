namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// ログイン中ユーザーモデル
    /// </summary>
    public class LoginUserModel
    {
        /// <summary>
        /// データベース名
        /// </summary>
        public String DatabaseName { get; set; }

        /// <summary>
        /// ユーザーID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// ユーザー名
        /// </summary>
        public String UserName { get; set; }

        /// <summary>
        /// ロール
        /// </summary>
        public int Role { get; set; }

        /// <summary>
        /// タイムスタンプ
        /// </summary>
        public DateTime TimeStamp { get; set; }
    }
}
