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
        public const string DateTimeSlashRegex = @"^(?:\d{4})\/(?:[1-9]|0?[1-9]|1[0-2])\/(?:[1-9]|0?[1-9]|[12]\d|3[01])$";

        /// <summary>
        /// 日付チェック（yyyyMMdd）Eg:納入指示日
        /// </summary>
        public const string DateTimeNoneSlashRegex = @"^(19\d{2}|[2-9]\d{3}|99999)(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])$";

        /// <summary>
        /// 数字チェック
        /// </summary>
        public const string NumberOnlyRegex = @"[0-9]+";

        /// <summary>
        /// 日付チェック
        /// </summary>
        public readonly static string[] DateFormats = { "yyyy/MM/dd", "yyyy/M/d", "yyyy-MM-dd", "yyyy-M-d", "yyyyMMdd" };

        /// <summary>
        /// 得意先
        /// </summary>
        public readonly static int Const_CustomerID = 1;

        /// <summary>
        /// 仕入先
        /// </summary>
        public readonly static int Const_SupplierID = 2;

        /// <summary>
        /// 納入先
        /// </summary>
        public readonly static int Const_DeliveryID = 3;

        /// <summary>
        /// 会社区分リスト
        /// </summary>
        public readonly static List<SelectListItem> Const_CompanyKubunList = new()
        {
            new() { Value = Const_CustomerID.ToString(), Text = "得意先", Selected = false },
            new() { Value = Const_SupplierID.ToString(), Text = "仕入先", Selected = false },
            new() { Value = Const_DeliveryID.ToString(), Text = "納入先", Selected = false }
        };

        /// <summary>
        /// 便リスト
        /// </summary>
        public readonly static List<SelectListItem> Const_BinList = new()
        {
            new() { Value = "1", Text = "1", Selected = false },
            new() { Value = "2", Text = "2", Selected = false },
            new() { Value = "3", Text = "3", Selected = false },
            new() { Value = "4", Text = "4", Selected = false }
        };

        /// <summary>
        /// プロパティの表示名取得
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
        /// フォーマットエラーメッセージ取得
        /// </summary>
        /// <param name="validationResult">検証結果</param>
        public static List<string> FormatValidationErrorMessage<T>(ValidationResult validationResult)
        {
            try
            {
                List<string> errorItem = new();

                // 列名取得
                //string memberName = validationResult.MemberNames.FirstOrDefault();
                //string checkitemName = GetDisplayName<T>(memberName);
                //errorItem.Add(checkitemName);

                // エラーメッセージ取得
                string errorMessage = validationResult.ErrorMessage;
                errorItem.Add(errorMessage);

                return errorItem;
            }
            catch (Exception) {
                throw;
            }
        }

        /// <summary>
        /// 翌日(土日を除く)を取得
        /// </summary>
        /// <param name="date">日付</param>
        public static DateTime GetNextWeekday(DateTime date)
        {
            date = date.AddDays(1);

            if (date.DayOfWeek == DayOfWeek.Saturday)
            {
                date = date.AddDays(2);
            }
            else if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }

            return date;
        }

        /// <summary>
        /// 日付に変換する
        /// </summary>
        public static string ConvertToYYYYMMDD(string inputDateString)
        {
            DateTime inputDate = DateTime.Parse(inputDateString);

            int year = inputDate.Year;
            int month = inputDate.Month;
            int day = inputDate.Day;

            string formattedDateString = $"{year}/{month:D2}/{day:D2}";

            return formattedDateString;
        }

    }
}
