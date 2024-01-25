using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using static mar_sumaken_web.Models.M_UserModel;

namespace mar_sumaken_web.Commons
{
    /// <summary> 
    /// 利用可能な値を確認する関数 
    /// </summary>
    public static class CheckAvailableValue
    {
        /// <summary>
        /// 数値型の真偽
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="value">確認する値</param>
        /// <param name="error">エラーメッセージ</param>
        /// <param name="isAvailableValue">利用可能な値であるかの真偽</param>
        /// <param name="logicalName">論理名</param>
        /// <param name="lineNum">行数</param>
        /// <param name="isNullable">null許容型の真偽</param>
        /// <param name="maxDigits">最大桁数</param>
        /// <returns>利用可能な値であるかの真偽、エラーメッセージ</returns>
        public static (bool, string) IsOfIntType(string sheetName, string value, string error, bool isAvailableValue, string logicalName, int lineNum, bool isNullable, int maxDigits)
        {
            var errorLocation = lineNum + "行目" + "　" + logicalName + "　";
            if (sheetName != null)
                sheetName += "：　";

            // nullチェック
            if (string.IsNullOrWhiteSpace(value))
            {
                // null許容型でない場合はエラー
                if (!isNullable)
                {
                    isAvailableValue = false;
                    // 「値が未入力です。値を入力してください。」
                    error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1001")
                        + "<br>";
                }
                else
                {
                    isAvailableValue = true;
                }
            }
            // int型でない場合はエラー
            else if (!int.TryParse(value, out _))
            {
                isAvailableValue = false;
                // 「数値指定に不正な値があります。半角数字で正しい値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1005")
                        + "<br>";
            }
            else
            {
                // 最大桁数を超える場合はエラー
                if (maxDigits != 0 && maxDigits < value.Length)
                {
                    isAvailableValue = false;
                    //「規定範囲外の値があります。正しい値を入力してください。」
                    error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1007")
                        + "（" + maxDigits + "文字以内）"
                        + "<br>";
                }
                else
                {
                    isAvailableValue = true;
                }
            }
            return (isAvailableValue, error);
        }


        /// <summary>
        /// 文字型の真偽
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="value">確認する値</param>
        /// <param name="error">エラーメッセージ</param>
        /// <param name="isAvailableValue">利用可能な値であるかの真偽</param>
        /// <param name="logicalName">論理名</param>
        /// <param name="lineNum">行数</param>
        /// <param name="isNullable">null許容型の真偽</param>
        /// <param name="maxDigits">最大桁数</param>
        /// <returns>利用可能な値であるかの真偽、エラーメッセージ</returns>
        public static (bool, string) IsOfStringType(string sheetName, string value, string error, string logicalName, int lineNum, bool isNullable, int maxDigits)
        {
            bool isAvailableValue = false;
            var errorLocation = lineNum + "行目" + "　" + logicalName + "　";
            if (sheetName != null)
                sheetName += "：　";

            // nullチェック
            if (string.IsNullOrWhiteSpace(value))
            {
                // null許容型でない場合はエラー
                if (!isNullable)
                {
                    isAvailableValue = false;
                    //「値が未入力です。値を入力してください。」
                    error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1001")
                        + "<br>";
                }
                else
                {
                    isAvailableValue = true;
                }
            }
            else
            {
                // 最大桁数を超える場合はエラー
                if (maxDigits != 0 && maxDigits < value.Length)
                {
                    isAvailableValue = false;
                    //「規定範囲外の値があります。正しい値を入力してください。」
                    error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1007")
                        + "（" + maxDigits + "文字以内）"
                        + "<br>";
                }
                else
                {
                    isAvailableValue = true;
                }
            }
            return (isAvailableValue, error);
        }


        /// <summary>
        /// 時間型の真偽
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="value">確認する値</param>
        /// <param name="error">エラーメッセージ</param>
        /// <param name="isAvailableValue">利用可能な値であるかの真偽</param>
        /// <param name="logicalName">論理名</param>
        /// <param name="lineNum">行数</param>
        /// <returns>利用可能な値であるかの真偽、エラーメッセージ</returns>
        public static (bool, string) IsOfTimeType(string sheetName, string value, string error, bool isAvailableValue, string logicalName, int lineNum)
        {
            var errorLocation = lineNum + "行目" + "　" + logicalName + "　";
            if (sheetName != null)
                sheetName += "：　";

            // nullの場合はエラー
            if (string.IsNullOrWhiteSpace(value))
            {
                isAvailableValue = false;
                //「値が未入力です。値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1001")
                        + "<br>";
            }
            // 時間型(HH:MM)に変換できない場合はエラー
            else if (!DateTime.TryParse(value, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.NoCurrentDateDefault, out _))
            {
                isAvailableValue = false;
                //「時間指定に不正な値があります。半角HH:MM形式で正しい値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1004")
                        + "<br>";
            }
            else
            {
                isAvailableValue = true;
            }
            return (isAvailableValue, error);
        }

        /// <summary>
        /// 日付型の真偽
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="value">確認する値</param>
        /// <param name="error">エラーメッセージ</param>
        /// <param name="isAvailableValue">利用可能な値であるかの真偽</param>
        /// <param name="logicalName">論理名</param>
        /// <param name="lineNum">行数</param>
        /// <param name="isNullable">null許容型の真偽</param>
        /// <returns>利用可能な値であるかの真偽、エラーメッセージ</returns>
        public static (bool, string) IsOfDateType(string sheetName, string value, string error, bool isAvailableValue, string logicalName, int lineNum, bool isNullable)
        {
            var year = DateTime.Now.Year.ToString();
            string date = year + value;
            var errorLocation = lineNum + "行目" + "　" + logicalName + "　";
            if (sheetName != null)
                sheetName += "：　";

            // nullの場合はエラー
            if (string.IsNullOrWhiteSpace(value))
            {
                // null許容型でない場合はエラー
                if (!isNullable)
                {
                    isAvailableValue = false;
                    //「値が未入力です。値を入力してください。」
                    error += sheetName + errorLocation
                            + ErrorHandling.CreateErrorMessage("E1001")
                            + "<br>";
                }
                else
                {
                    isAvailableValue = true;
                }
            }
            // 日付型(yyyyMMdd)に変換できない場合はエラー
            else if (!DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                isAvailableValue = false;
                //「日付指定に不正な値があります。半角MMDD形式で正しい値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1019")
                        + "<br>";
            }
            else
            {
                isAvailableValue = true;
            }
            return (isAvailableValue, error);
        }

        /// <summary>
        /// 日付型の真偽
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="value">確認する値</param>
        /// <param name="error">エラーメッセージ</param>
        /// <param name="isAvailableValue">利用可能な値であるかの真偽</param>
        /// <param name="logicalName">論理名</param>
        /// <param name="lineNum">行数</param>
        /// <returns>利用可能な値であるかの真偽、エラーメッセージ</returns>
        public static (bool, string) IsOfDateType(string sheetName, string value, string error, bool isAvailableValue, string logicalName, int lineNum)
        {
            var errorLocation = lineNum + "行目" + "　" + logicalName + "　";
            if (sheetName != null)
                sheetName += "：　";

            // nullの場合はエラー
            if (string.IsNullOrWhiteSpace(value))
            {
                isAvailableValue = false;
                //「値が未入力です。値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1001")
                        + "<br>";
            }
            // 日付型(yyyyMMdd)に変換できない場合はエラー
            else if (!DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                isAvailableValue = false;
                //「日付指定に不正な値があります。半角YYYYMMDD形式で正しい値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1003")
                        + "<br>";
            }
            else
            {
                isAvailableValue = true;
            }
            return (isAvailableValue, error);
        }


        /// <summary>
        /// 日付/時刻型の真偽
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="value">確認する値</param>
        /// <param name="error">エラーメッセージ</param>
        /// <param name="isAvailableValue">利用可能な値であるかの真偽</param>
        /// <param name="logicalName">論理名</param>
        /// <param name="lineNum">行数</param>
        /// <returns>利用可能な値であるかの真偽、エラーメッセージ</returns>
        public static (bool, string) IsOfDateTimeType(string sheetName, string value, string error, bool isAvailableValue, string logicalName, int lineNum)
        {
            var errorLocation = lineNum + "行目" + "　" + logicalName + "　";
            if (sheetName != null)
                sheetName += "：　";

            // nullの場合はエラー
            if (string.IsNullOrWhiteSpace(value))
            {
                isAvailableValue = false;
                //「値が未入力です。値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1001")
                        + "<br>";
            }
            // DateTime型でない場合はエラー
            else if (!DateTime.TryParse(value, out _))
            {
                isAvailableValue = false;
                //「日付指定に不正な値があります。半角YYYY/MM/DD形式で正しい値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1012")
                        + "<br>";
            }
            else
            {
                isAvailableValue = true;
            }
            return (isAvailableValue, error);
        }


        /// <summary>
        /// ログインIDの真偽
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="value">確認する値</param>
        /// <param name="error">エラーメッセージ</param>
        /// <param name="isAvailableValue">利用可能な値であるかの真偽</param>
        /// <param name="logicalName">論理名</param>
        /// <param name="lineNum">行数</param>
        /// <returns>利用可能な値であるかの真偽、エラーメッセージ</returns>
        public static (bool, string) IsLoginId(string sheetName, string value, string error, bool isAvailableValue, string logicalName, int lineNum)
        {
            var errorLocation = lineNum + "行目" + "　" + logicalName + "　";
            if (sheetName != null)
                sheetName += "：　";

            // 固定長(4桁)
            int fixedLength = 4;

            // nullの場合はエラー
            if (string.IsNullOrWhiteSpace(value))
            {
                isAvailableValue = false;
                //「値が未入力です。値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1001")
                        + "<br>";
            }
            // フォーマットが半角・英字小文字・数字でない場合はエラー
            else if (!Regex.IsMatch(value, "^[a-z0-9]*$"))
            {
                isAvailableValue = false;
                //「文字指定に不正な値があります。半角英数字で正しい値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1015")
                        + "<br>";
            }
            else
            {
                // 長さが固定長と一致しない場合はエラー
                if (value.Length != fixedLength)
                {
                    isAvailableValue = false;
                    //「規定の桁数に満たない、または超えている値が指定されています。正しく指定してください。」
                    error += sheetName + errorLocation
                           + ErrorHandling.CreateErrorMessage("E1006")
                           + "（半角英数字" + fixedLength + "桁のみ）"
                           + "<br>";
                }
                else
                {
                    isAvailableValue = true;
                }
            }
            return (isAvailableValue, error);
        }


        /// <summary>
        /// 管理権限区分の真偽
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="value">確認する値</param>
        /// <param name="error">エラーメッセージ</param>
        /// <param name="isAvailableValue">利用可能な値であるかの真偽</param>
        /// <param name="logicalName">論理名</param>
        /// <param name="lineNum">行数</param>
        /// <returns>利用可能な値であるかの真偽、エラーメッセージ</returns>
        public static (bool, string) IsAuthorizedKubun(string sheetName, string value, string error, bool isAvailableValue, string logicalName, int lineNum)
        {
            var errorLocation = lineNum + "行目" + "　" + logicalName + "　";
            if (sheetName != null)
                sheetName += "：　";

            // 管理権限区分リスト
            string[] listAuthen = { "1", "2", "3", "4" };

            // nullの場合はエラー
            if (string.IsNullOrWhiteSpace(value))
            {
                isAvailableValue = false;
                //「値が未入力です。値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1001")
                        + "<br>";
            }
            // 管理権限区分リスト内に存在しない場合はエラー
            else if (!listAuthen.Contains(value))
            {
                isAvailableValue = false;
                //「数値指定に不正な値があります。半角数字で正しい値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1005")
                        + "<br>";
            }
            else
            {
                isAvailableValue = true;
            }
            return (isAvailableValue, error);
        }


        /// <summary>
        /// 削除フラグの真偽
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="value">確認する値</param>
        /// <param name="error">エラーメッセージ</param>
        /// <param name="isAvailableValue">利用可能な値であるかの真偽</param>
        /// <param name="logicalName">論理名</param>
        /// <param name="lineNum">行数</param>
        /// <returns>利用可能な値であるかの真偽、エラーメッセージ</returns>
        public static (bool, string) IsDeleted(string sheetName, string value, string error, bool isAvailableValue, string logicalName, int lineNum)
        {
            var errorLocation = lineNum + "行目" + "　" + logicalName + "　";
            if (sheetName != null)
                sheetName += "：　";

            // 削除フラグリスト
            string[] listDeleted = { "0", "1" };

            // nullの場合はエラー
            if (string.IsNullOrWhiteSpace(value))
            {
                isAvailableValue = false;
                //「値が未入力です。値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1001")
                        + "<br>";
            }
            // 削除フラグリスト内に存在しない場合はエラー
            else if (!listDeleted.Contains(value))
            {
                isAvailableValue = false;
                //「数値指定に不正な値があります。半角数字で正しい値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1005")
                        + "<br>";
            }
            else
            {
                isAvailableValue = true;
            }
            return (isAvailableValue, error);
        }


        /// <summary>
        /// 出荷レーン名の真偽
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="value">確認する値</param>
        /// <param name="error">エラーメッセージ</param>
        /// <param name="isAvailableValue">利用可能な値であるかの真偽</param>
        /// <param name="logicalName">論理名</param>
        /// <param name="lineNum">行数</param>
        /// <param name="isNullable">null許容型の真偽</param>
        /// <returns>利用可能な値であるかの真偽、エラーメッセージ</returns>
        public static (bool, string) IsShippingLane(string sheetName, string value, string error, bool isAvailableValue, string logicalName, int lineNum, bool isNullable)
        {
            var errorLocation = lineNum + "行目" + "　" + logicalName + "　";
            if (sheetName != null)
                sheetName += "：　";

            // nullチェック
            if (string.IsNullOrWhiteSpace(value))
            {
                // null許容型でない場合はエラー
                if (!isNullable)
                {
                    isAvailableValue = false;
                    //「値が未入力です。値を入力してください。」
                    error += sheetName + errorLocation
                            + ErrorHandling.CreateErrorMessage("E1001")
                            + "<br>";
                }
                else
                {
                    isAvailableValue = true;
                }
            }
            else
            {
                // FK存在チェック
                var getShippingLaneName = GetShippingLaneName();
                // 出荷レーン情報内に存在しない場合はエラー
                if (!getShippingLaneName.Contains(value))
                {
                    isAvailableValue = false;
                    //「該当データがありません。」
                    error += sheetName + errorLocation
                            + ErrorHandling.CreateErrorMessage("E2007")
                            + "<br>";
                }
                else
                {
                    isAvailableValue = true;
                }
            }
            return (isAvailableValue, error);
        }


        /// <summary>
        /// 工場区分の真偽
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="value">確認する値</param>
        /// <param name="error">エラーメッセージ</param>
        /// <param name="isAvailableValue">利用可能な値であるかの真偽</param>
        /// <param name="logicalName">論理名</param>
        /// <param name="lineNum">行数</param>
        /// <returns>利用可能な値であるかの真偽、エラーメッセージ</returns>
        public static (bool, string) IsFactoryKubun(string sheetName, string value, string error, bool isAvailableValue, string logicalName, int lineNum)
        {
            var errorLocation = lineNum + "行目" + "　" + logicalName + "　";
            if (sheetName != null)
                sheetName += "：　";

            // 工場区分リスト
            string[] listAuthen = { "000", "001", "003" };

            // nullの場合はエラー
            if (string.IsNullOrWhiteSpace(value))
            {
                isAvailableValue = false;
                //「値が未入力です。値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1001")
                        + "<br>";
            }
            // 工場区分リスト内に存在しない場合はエラー
            else if (!listAuthen.Contains(value))
            {
                isAvailableValue = false;
                //「文字指定に不正な値があります。正しい値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1016")
                        + "<br>";
            }
            else
            {
                isAvailableValue = true;
            }
            return (isAvailableValue, error);
        }

        /// <summary>
        /// 仕入先発日時の真偽
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="value">確認する値</param>
        /// <param name="error">エラーメッセージ</param>
        /// <param name="isAvailableValue">利用可能な値であるかの真偽</param>
        /// <param name="logicalName">論理名</param>
        /// <param name="lineNum">行数</param>
        /// <param name="isNullable">null許容型の真偽</param>
        /// <param name="rawLoadingDate">仕入先発ルート日付</param>
        /// <returns>利用可能な値であるかの真偽、エラーメッセージ</returns>
        public static (bool, string) IsOfSupplierDepartureDateTime(string sheetName, string value, string error, bool isAvailableValue, string logicalName, int lineNum, bool isNullable, string rawLoadingDate)
        {
            var errorLocation = lineNum + "行目" + "　" + logicalName + "　";
            if (sheetName != null)
                sheetName += "：　";

            // nullの場合はエラー
            if (string.IsNullOrWhiteSpace(value))
            {
                // null許容型でない場合はエラー
                if (!isNullable)
                {
                    isAvailableValue = false;
                    //「値が未入力です。値を入力してください。」
                    error += sheetName + errorLocation
                            + ErrorHandling.CreateErrorMessage("E1001")
                            + "<br>";
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(rawLoadingDate))
                    {
                        isAvailableValue = false;
                        //「値が未入力です。仕入先発ルート日付と仕入先発日時の値はセットで入力してください。」
                        error += sheetName + errorLocation
                            + ErrorHandling.CreateErrorMessage("E1018")
                            + "<br>";
                    }
                    else
                    {
                        isAvailableValue = true;
                    }
                }
            }
            // 日付型(yyyyMMdd)に変換できない場合はエラー
            else if (!DateTime.TryParseExact(value, "yyyyMMddHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                isAvailableValue = false;
                //「日付指定に不正な値があります。半角YYYYMMDDHHMM形式で正しい値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1020")
                        + "<br>";
            }
            else
            {
                isAvailableValue = true;
            }
            return (isAvailableValue, error);
        }


        /// <summary>
        /// ヘッダーのエラーメッセージ取得
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="value">確認する値</param>
        /// <param name="error">エラーメッセージ</param>
        /// <param name="logicalName">論理名</param>
        /// <param name="lineNum">行数</param>
        /// <returns>エラーメッセージ</returns>
        public static string GetErrorMessageForHeader(string sheetName, string value, string error, string logicalName, int lineNum)
        {
            var errorLocation = lineNum + "行目" + "　" + logicalName + "　";

            if (sheetName != null)
                sheetName += "：　";

            // nullの場合はエラー
            if (string.IsNullOrWhiteSpace(value))
            {
                // 「値が未入力です。値を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1001")
                        + "<br>";
            }
            else
            {
                // ヘッダー名が一致しない場合はエラー
                if (value != logicalName)
                {
                    // 「ヘッダー名が正しくありません。正しい値を入力してください。」
                    error += sheetName + errorLocation
                            + ErrorHandling.CreateErrorMessage("E1010")
                            + "<br>";
                }
            }
            return error;
        }


        /// <summary>
        /// パスワードの真偽
        /// </summary>
        public static (bool, string) CheckErrorStringTypePassWord(string sheetName, string id, string loginId, string passwordCheck, string isDeleted, string error, bool physicalName, string logicalName, int lineNum, int maxText, bool deletedAvaliableCheck)
        {
            var errorLocation = lineNum + "行目" + "　" + logicalName + "　";
            if (sheetName != null)
                sheetName += "：　";

            // パスワードチェック
            if (!deletedAvaliableCheck)
            {
                //「規定範囲外の値が指定されています。新規登録の場合、0を入力してください。」
                error += sheetName + errorLocation
                        + ErrorHandling.CreateErrorMessage("E1014")
                        + "<br>";
                physicalName = false;
            }
            // 削除フラグが数値でないまたはユーザーIDが数値でない場合はエラー
            else if (!string.IsNullOrWhiteSpace(isDeleted) && (!int.TryParse(isDeleted, out _) || !int.TryParse(id, out _)))
                physicalName = false;
            // 削除フラグが空欄かつユーザーIDが数値でない場合はエラー
            else if (string.IsNullOrWhiteSpace(isDeleted) && !int.TryParse(id, out _))
                physicalName = false;
            else
            {
                // ユーザー取得実施
                var userRegisterd = GetListMUsersForCheckFromExcel(id, loginId, isDeleted);
                // ユーザー重複取得
                var userDuplicate = GetListMUsersForCheckDuplicate(id, loginId, isDeleted);
                // 登録されたユーザー取得
                var avaliableUser = GetAvaliableMUsers(id, loginId);
                // 削除したユーザー取得
                var deleteUser = GetDeleteUsers(id, loginId);

                if (userRegisterd.Count > 0)　// 削除フラグと登録されたユーザーが存在している
                {
                    if (!string.IsNullOrWhiteSpace(passwordCheck) && (passwordCheck.Length > maxText || passwordCheck.Length < 4))
                    {
                        //「規定の桁数に満たない、または超えている値が指定されています。正しく指定してください。」
                        error += sheetName + errorLocation
                                + ErrorHandling.CreateErrorMessage("E1006")
                                + "（4～" + maxText + "文字以内）"
                                + "<br>";
                        physicalName = false;
                    }
                    else
                        physicalName = true;
                }
                else if (string.IsNullOrWhiteSpace(passwordCheck))
                {
                    if (avaliableUser.Count > 0)　// 更新するため、削除フラグ0と登録されたユーザーが存在をチェック
                        physicalName = true;
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(isDeleted) && Convert.ToInt16(isDeleted) != 0)　// 新規登録の場合、削除フラグ0をチェック
                        {
                            //「規定範囲外の値が指定されています。新規登録の場合、0を入力してください。」
                            error += sheetName + errorLocation
                                    + ErrorHandling.CreateErrorMessage("E1014")
                                    + "<br>";
                            physicalName = false;
                        }
                        else if (deleteUser.Count > 0)
                            physicalName = true; // 登録されたユーザーは 削除フラグ1から0に変更
                        else
                        {
                            //「値が未入力です。値を入力してください。」
                            error += sheetName + errorLocation
                                    + ErrorHandling.CreateErrorMessage("E1001")
                                    + "<br>";
                            physicalName = false;
                        }
                    }
                }
                else if (userDuplicate.Count > 0)　//　ユーザー重複をチェック
                {
                    //「E1002 指定した値が重複しています。」
                    error += sheetName + lineNum + "行目　ログインID　"
                            + ErrorHandling.CreateErrorMessage("E1002")
                            + "<br>";
                    physicalName = false;
                }
                else　// 削除フラグと登録されたユーザーが存在していない
                {
                    if (!string.IsNullOrWhiteSpace(passwordCheck) && Regex.IsMatch(passwordCheck, "^[a-zA-Z0-9]*$"))  // 英数字以外 チェック
                    {
                        if (passwordCheck.Length > maxText || passwordCheck.Length < 4)
                        {
                            //「規定の桁数に満たない、または超えている値が指定されています。正しく指定してください。」
                            error += sheetName + errorLocation
                                + ErrorHandling.CreateErrorMessage("E1006")
                                + "（4～" + maxText + "文字以内）"
                                + "<br>";
                            physicalName = false;
                        }
                        else
                            physicalName = true;
                    }
                    else
                    {
                        //「文字指定に不正な値があります。半角英数字で正しい値を入力してください。」
                        error += sheetName + errorLocation
                                + ErrorHandling.CreateErrorMessage("E1015")
                                + "<br>";
                        physicalName = false;
                    }
                }
            }
            return (physicalName, error);
        }

        /// <summary>
        /// ユーザーマスター取得実施
        /// </summary>
        /// <returns>strList</returns>
        public static List<M_User> GetListMUsersForCheckFromExcel(string id, string loginId, string isDelete)
        {
            //SQL作成
            var sql = M_UsersConnectController.CreateSQLToGetMUsersByCondition(id, loginId, isDelete);

            //DB接続
            List<M_User> strList = M_UsersConnectController.ConnectMUsers(sql);

            return strList;
        }

        /// <summary>
        /// ユーザーマスター取得実施
        /// </summary>
        /// <returns>strList</returns>
        public static List<M_User> GetListMUsersForCheckDuplicate(string userId, string loginId, string isDelete)
        {
            //SQL作成
            var sql = M_UsersConnectController.CreateSQLToGetMUsersDuplicate(userId, loginId, isDelete);

            //DB接続
            List<M_User> strList = M_UsersConnectController.ConnectMUsers(sql);

            return strList;
        }

        /// <summary>
        /// 削除フラグ(0)のユーザーマスター取得実施
        /// </summary>
        /// <returns>strList</returns>
        public static List<M_User> GetAvaliableMUsers(string userId, string loginId)
        {
            //SQL作成
            var sql = M_UsersConnectController.CreateSQLToGetAvaliableMUsers(userId, loginId);

            //DB接続
            List<M_User> strList = M_UsersConnectController.ConnectMUsers(sql);

            return strList;
        }

        /// <summary>
        /// 削除フラグ(0)のユーザーマスター取得実施
        /// </summary>
        /// <returns>strList</returns>
        public static List<M_User> GetDeleteUsers(string userId, string loginId)
        {
            //SQL作成
            var sql = M_UsersConnectController.CreateSQLToGetDeleteUsers(userId, loginId);

            //DB接続
            List<M_User> strList = M_UsersConnectController.ConnectMUsers(sql);

            return strList;
        }


        /// <summary>
        /// 全ての出荷レーン名を取得
        /// </summary>
        private static List<string> GetShippingLaneName()
        {
            // SQLServer接続文字列取得
            var connectionString = ConnectToSQLServer.GetSQLServerConnectionString();
            var idList = new List<string>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var commandText = D_ShipmentScheduleConnectController.CreateSQLGetShippingLanesName();
                using var command = new SqlCommand(commandText, connection);
                using var reader = command.ExecuteReader();
                while (reader.Read() == true)
                {
                    idList.Add(reader["shipping_lane_name"].ToString());
                }
            }
            return idList;
        }

    }
}
