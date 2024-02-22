using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace mar_sumaken_web.Commons
{
    public static class Utils
    {

        /// <summary>
        /// 日付チェック（yyyy/MM/dd）Eg:入荷予定日
        /// </summary>
        public const string DateTimeSlashRegex = @"^(?:\d{4})\/(?:[1-9]|0[1-9]|1[0-2])\/(?:[1-9]|[12]\d|3[01])$";

        /// <summary>
        /// 日付チェック（yyyyMMdd）Eg:納入指示日
        /// </summary>
        public const string DateTimeNoneSlashRegex = @"^(19\d{2}|[2-9]\d{3}|99999)(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])$";

        /// <summary>
        /// 数字チェック
        /// </summary>
        public const string NumberOnlyRegex = @"[0-9]+";

        public readonly static string[] DateFormats = { "yyyy/MM/dd", "yyyy/M/d", "yyyy-MM-dd", "yyyy-M-d", "yyyyMMdd" };

        public readonly static int Const_Customer_ID = 1; // 得意先
        public readonly static int Const_Supplier_ID = 2; // 仕入先
        public readonly static int Const_Delivery_ID = 3; // 納入先

        /// <summary>
        /// 会社区分リスト
        /// </summary>
        public readonly static List<SelectListItem> Const_Company_Kubun_List = new List<SelectListItem>()
        {
            new SelectListItem() { Value = "1", Text = "得意先", Selected = false },
            new SelectListItem() { Value = "2", Text = "仕入先", Selected = false },
            new SelectListItem() { Value = "3", Text = "納入先", Selected = false }
        };

        /// <summary>
        /// プロパティの表示名を取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName">プロパティ名</param>
        /// <returns>表示名</returns>
        public static string GetDisplayName<T>(string propertyName)
        {
            var displayName = string.Empty;
            var property = typeof(T).GetProperty(propertyName);
            if (property != null)
            {
                var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                {
                    displayName = displayAttribute.Name;
                }
            }
            return displayName;
        }

        /// <summary>
        /// フォーマットエラーメッセージ
        /// </summary>
        /// <param name="validationResult">検証結果</param>
        public static List<string> FormatValidationErrorMessage<T>(ValidationResult validationResult)
        {
            try
            {
                List<string> errorItem = new();

                // 列名取得
                string memberName = validationResult.MemberNames.FirstOrDefault();
                string checkitemName = GetDisplayName<T>(memberName);
                errorItem.Add(checkitemName);

                // エラーメッセージ取得
                string errorMessage = validationResult.ErrorMessage;
                errorItem.Add(errorMessage);

                return errorItem;
            }
            catch (Exception) {
                throw;
            }
        }

    }
}
