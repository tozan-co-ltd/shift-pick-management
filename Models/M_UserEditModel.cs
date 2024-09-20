using ai_truck_load_measurement.Properties;
using System.ComponentModel.DataAnnotations;

namespace ai_truck_load_measurement.Models
{
    /// <summary>
    /// ユーザーマスターのModel(修正画面用)
    /// </summary>
    public class M_UserEditModel : M_UserModel
    {
        /// <summary>
        /// パスワード
        /// </summary>
        [Display(Name = "パスワード")]
        [RegularExpression(@"[a-zA-Z0-9]+", ErrorMessageResourceName = "E1003", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [StringLength(12, MinimumLength = 4, ErrorMessageResourceName = "E1023", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? Password { get; set; }
    }
}
