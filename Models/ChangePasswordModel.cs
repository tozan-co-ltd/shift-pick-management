using mar_sumaken_web.Properties;
using System.ComponentModel.DataAnnotations;

namespace mar_sumaken_web.Models
{
    /// <summary>
    /// パスワード変更時に使うModel
    /// </summary>
    public class ChangePasswordModel : CommonModel
    {
        /// <summary>
        /// ユーザーID
        /// </summary>
        [Display(Name = "ID")]
        public int UserID { get; set; }

        /// <summary>
        /// ユーザー名
        /// </summary>
        [Display(Name = "ユーザー名")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string UserName { get; set; }

        /// <summary>
        /// パスワード
        /// </summary>
        [Display(Name = "パスワード")]
        [DataType(DataType.Password)]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [RegularExpression(@"[a-zA-Z0-9]+", ErrorMessageResourceName = "E1003", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        [StringLength(12, MinimumLength = 4, ErrorMessageResourceName = "E1023", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string? Password { get; set; }

        /// <summary>
        /// ソルト
        /// </summary>
        [Display(Name = "ソルト")]
        public string? Salt { get; set; }
    }
}
