using mar_sumaken_web.Properties;
using System.ComponentModel.DataAnnotations;
using static mar_sumaken_web.Models.M_UserModel;

namespace mar_sumaken_web.Models
{
    public class M_UserEditModel : M_User
    {
        /// <summary>
        /// パスワード
        /// </summary>
        [Display(Name = "パスワード")]
        [RegularExpression(@"[a-zA-Z0-9]{4,10}", ErrorMessage = "パスワードは４～10文字入力してください。")]
        public string? Password { get; set; }
    }
}
