namespace ai_truck_load_measurement.Models
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
        /// タイムスタンプ
        /// </summary>
        public DateTime TimeStamp { get; set; }
    }
}
