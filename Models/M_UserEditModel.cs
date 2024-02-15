using mar_sumaken_web.Properties;
using System.ComponentModel.DataAnnotations;

namespace mar_sumaken_web.Models
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
        [RegularExpression(@"[a-zA-Z0-9]{4,10}", ErrorMessage = "パスワードは4～10文字で入力してください。")]
        public string? Password { get; set; }
    }
}
