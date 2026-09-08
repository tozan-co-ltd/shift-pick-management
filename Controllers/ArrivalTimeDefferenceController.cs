using shift_pick_management.Commons;
using shift_pick_management.ConnectControllers;
using shift_pick_management.Models;
using shift_pick_management.Properties;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;

namespace shift_pick_management.Controllers
{
    public class ArrivalTimeDefferenceController : BaseController
    {
        public IActionResult Index()
        {
            var model = new LoadRecordViewModel();
            var user = ClaimsLoginUserData();
            model.MainDepoID = user.MainDepoID;
            model.MainDepoName = user.MainDepoName;
            return View(model);
        }

        /// <summary>
        /// アラート履歴情報テーブル非同期更新用
        /// </summary>
        /// <param name="isBeforeApplicablePeriod">適用期間外のデータを含めるか</param>
        /// <returns></returns>
        public JsonResult SearchData(DateTime startOfPeriod, DateTime endOfPeriod, List<SelectedTripModel> trips, bool isRecordCountOneOrMore)
        {
            var searchData = string.Empty;
            List<LoadRecordModel> loadRecordList = new();
            try
            {
                if (trips.Count > 0)
                {
                    // アラート履歴に対応する便実績情報取得SQL作成
                    var loadRecordSql =ArrivalTimeDefferenceConnectController.CreateSQLToSelectArrivalRecordAndSchedule(trips, startOfPeriod, endOfPeriod);
                    // DB接続
                    loadRecordList = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(loadRecordSql);
                }

                // テーブルのヘッダ部分
                searchData += GetTableHeader(trips, "tripTable");

                // テーブルのbody部分
                if (loadRecordList.Count > 0)
                {
                    // 選択した日付1日ごとに
                    for (DateTime date = startOfPeriod; date <= endOfPeriod; date = date.AddDays(1))
                    {
                        var targetDateRecords = loadRecordList.FindAll(x => x.WorkDay == date);

                        // 選択した日付に実績が登録されていない、かつそれらを表示しない場合、処理を飛ばす
                        if (targetDateRecords.Count == 0 && isRecordCountOneOrMore)
                            continue;

                        searchData += $@"
                            <tr>
                                <td>{date.ToString("yyyy/MM/dd")}</td>
                        ";
                        
                        foreach (var trip in trips)
                        {
                            var targetDateRecord = targetDateRecords.Find(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq.ToString());
                            if (targetDateRecord != null)
                            {
                                var comparisonTime = new DateTime(1900, 1, 1, targetDateRecord.ArrivedAt.Hour, targetDateRecord.ArrivedAt.Minute, targetDateRecord.ArrivedAt.Second);
                                var timeDeff = GetTimeDeff(targetDateRecord.ArrivalScheduledTime, comparisonTime);
                                var emphasizeColorStyle = "";

                                var timeDeffString = "";
                                if (timeDeff < 0)
                                {
                                    if (timeDeff <= -60)
                                        emphasizeColorStyle = $" style=\"background-color:#4D79A7; color:#FFF; border-color: #404040;\"";
                                    else if(timeDeff <= -30)
                                        emphasizeColorStyle = $" style=\"background-color:#ABC1D7\"";
                                    timeDeffString = $"{emphasizeColorStyle}>{timeDeff}分";
                                }
                                else
                                {
                                    if (timeDeff >= 20)
                                        emphasizeColorStyle = $@" style=""background-color:#FF5858; color: #FFF; border-color: #404040;""";
                                    else if(timeDeff >= 10)
                                        emphasizeColorStyle = $" style=\"background-color:#FFE5E5\"";
                                    timeDeffString = $@"{emphasizeColorStyle}>+{timeDeff}分";
                                }
                                    searchData += $@"
                                <td{timeDeffString}</td>
                            ";
                            }
                            else
                            {
                                searchData += $@"
                                <td>-</td>
                            ";
                            }
                        }

                        searchData += $@"
                            </tr>
                        ";
                    }

                    searchData += $@"
                            </tbody>
                            <tbody>
                            <tr hidden></tr>
                            {GetCountEarlyTimeOverHTML(loadRecordList, trips)}
                            {GetCountLateTimeOverHTML(loadRecordList, trips)}
                            </tbody>
                    </div>
                    ";
                }
                

                return Json(new SearchedTripRecordListModel
                {
                    searchedTripRecordHTML = searchData,
                    searchedTripRecordLength = loadRecordList.Count()
                });
            }
            catch (SqlException)
            {
                return Json(new { errorMessage = "E3004: " + ErrorMessagesResources.E3004 });
            }
            catch (Exception)
            {
                var errorMessage = "E9999: " + ErrorMessagesResources.E9999;
                return Json(errorMessage);
            }
        }


        /// <summary>
        /// 選択した便毎にデータテーブルのヘッダー箇所を作成する
        /// </summary>
        /// <param name="trips">選択した便</param>
        /// <returns></returns>
        private string GetTableHeader( List<SelectedTripModel> trips, string tableID)
        {
            var tableHeader = $@"
                    <div class=""mt-3"">
                        <table class=""table table-sm stripe hover nowrap datatable-normal table-center statistics-table"" id=""{tableID}"">
                            <thead>
                                <tr align=""center"">
                                    <th class=""font-weight-bold"">稼働日</th>
            ";

            foreach (var trip in trips)
            {
                tableHeader += $@"<th class=""font-weight-bold"">{trip.TripName}_{trip.TripBranchSeq}</th>
                ";
            }

            tableHeader += $@"
                                </tr>
                            </thead>
                            <tbody>
            ";
            return tableHeader;
        }

        /// <summary>
        /// 時間差を分単位で取得する
        /// </summary>
        /// <param name="targetDate">対象時間</param>
        /// <param name="comparisonTime">比較用時間</param>
        /// <returns></returns>
        private int GetTimeDeff(DateTime targetDate, DateTime comparisonTime)
        {
            var timeDeff = (comparisonTime - targetDate).TotalMinutes;

            // 対象時間と比較用時間が日付をまたいでいた場合の処理
            if (timeDeff >= 60 * 20)
                timeDeff -= 60 * 24;
            else if (timeDeff <= -60 * 23)
                timeDeff += 60 * 24;

            return (int)timeDeff;
        }


        /// <summary>
        /// アラート範囲(早着)回数HTML取得
        /// </summary>
        /// <param name="loadRecordList">便実績リスト</param>
        /// <param name="trips">選択した便</param>
        /// <returns></returns>
        private string GetCountEarlyTimeOverHTML(List<LoadRecordModel> loadRecordList, List<SelectedTripModel> trips)
        {
            var earlyTimeOverString = $@"
                    <tr  class=""statistics-table-top mt-2"">
                        <td>アラート範囲(早着)</td>

            ";
            foreach (var trip in trips)
            {
                // 便毎に実績のリストを作成
                var targetTripRecords = loadRecordList.FindAll(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq.ToString());

                // 予実差が－60分以上のデータ数を取得
                var targetTripEarlyOverCount = GetCountEarlyTimeOver(targetTripRecords);

                earlyTimeOverString += $@"
                        <td>{targetTripEarlyOverCount}回</td>
                ";
            }
            earlyTimeOverString += "</tr>";

            return earlyTimeOverString;
        }

        /// <summary>
        /// アラート範囲(早着)回数取得
        /// </summary>
        /// <param name="targetTripRecords">便実績リスト</param>
        /// <returns></returns>
        private int GetCountEarlyTimeOver(List<LoadRecordModel> targetTripRecords)
        {
            var targetTripEarlyOverCount = 0;
            foreach (var tripRecord in targetTripRecords)
            {
                var comparisonTime = new DateTime(1900, 1, 1, tripRecord.ArrivedAt.Hour, tripRecord.ArrivedAt.Minute, tripRecord.ArrivedAt.Second);
                var timeDeff = GetTimeDeff(tripRecord.ArrivalScheduledTime, comparisonTime);
                if (timeDeff <= -60)
                    targetTripEarlyOverCount++;
            }
            return targetTripEarlyOverCount;
        }

        /// <summary>
        /// アラート範囲(遅着)回数HTML取得
        /// </summary>
        /// <param name="loadRecordList">便実績リスト</param>
        /// <param name="trips">選択した便</param>
        /// <returns></returns>
        private string GetCountLateTimeOverHTML(List<LoadRecordModel> loadRecordList, List<SelectedTripModel> trips)
        {
            var lateTimeOverString = $@"
                    <tr class=""statistics-table-bottom"">
                        <td>アラート範囲(遅着)</td>

            ";
            foreach (var trip in trips)
            {
                // 便毎に実績のリストを作成
                var targetTripRecords = loadRecordList.FindAll(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq.ToString());

                // 予実差が+20分以上のデータ数を取得
                var targetTripLateOverCount = GetCountLateTimeOver(targetTripRecords);

                lateTimeOverString += $@"
                        <td>{targetTripLateOverCount}回</td>
                ";
            }
            lateTimeOverString += "</tr>";

            return lateTimeOverString;
        }

        /// <summary>
        /// アラート範囲(遅着)回数取得
        /// </summary>
        /// <param name="targetTripRecords">便実績リスト</param>
        /// <returns></returns>
        private int GetCountLateTimeOver(List<LoadRecordModel> targetTripRecords)
        {
            var targetTripLateOverCount = 0;
            foreach (var tripRecord in targetTripRecords)
            {
                var comparisonTime = new DateTime(1900, 1, 1, tripRecord.ArrivedAt.Hour, tripRecord.ArrivedAt.Minute, tripRecord.ArrivedAt.Second);
                var timeDeff = GetTimeDeff(tripRecord.ArrivalScheduledTime, comparisonTime);
                if (timeDeff >= +20)
                    targetTripLateOverCount++;
            }
            return targetTripLateOverCount;
        }

        /// <summary>
        /// ファイル出力
        /// </summary>
        /// <param name="gamenName">現在の画面名</param>
        /// <param name="startOfPeriod">期間の開始日時</param>
        /// <param name="endOfPeriod">期間の終了日時</param>
        /// <returns></returns>
        public JsonResult ExportFile(string gamenName, DateTime startOfPeriod, DateTime endOfPeriod, List<SelectedTripModel> arrayTrips, List<string> checkedDepos)
        {
            string? errorMessage;
            string startDate = startOfPeriod.ToString("yyyyMMdd");
            string endDate = endOfPeriod.ToString("yyyyMMdd");
            try
            {
                // 検索条件シート用データテーブル作成
                DataTable searchConditionDT = new DataTable();
                searchConditionDT.Columns.Add("項目名");
                searchConditionDT.Columns.Add("検索条件");
                // デポの設定
                var selectedDeposName = LoadRecordController.SelectedDepos(checkedDepos);
                searchConditionDT.Rows.Add("対象デポ", selectedDeposName);
                // 期間の設定
                searchConditionDT.Rows.Add("稼働日", $"{startDate}～{endDate}");
                // 選択された便の設定
                var selectedTripNames = "";
                for (int i = 0; i < arrayTrips.Count; i++)
                {
                    if (i != 0)
                    {
                        selectedTripNames += ", ";
                    }
                    selectedTripNames += arrayTrips[i].SelectedTripName;
                }
                searchConditionDT.Rows.Add("選択された便", selectedTripNames);

                // 便実績情報取得
                var tTripRecordSql = ArrivalTimeDefferenceConnectController.CreateSQLToSelectArrivalRecordAndSchedule(arrayTrips, startOfPeriod, endOfPeriod);
                var tTripRecordList = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(tTripRecordSql);
                var tTripRecordDT = ConversionLoadRecordToDataTable(tTripRecordList, arrayTrips, startOfPeriod, endOfPeriod);

                // 便実績が0の場合
                if (tTripRecordDT.Rows.Count == 0)
                {
                    errorMessage = "E1016:" + ErrorMessagesResources.E1016;
                    return Json(new { res = "NG", error = errorMessage });
                }

                // ファイル名
                var tmpFilename = $"荷量実績_{startDate}-{endDate}.xlsx";
                // 2シートあり
                bool sheetTwo = true;

                // シート名
                string sheetNameOne = "荷量実績";
                string sheetNameTwo = "検索条件";

                // ヘッダ用便名称_便枝番リスト作成
                var selectedTripNameAndBranchSeqs = GetTripNameAndBranchSeqs(arrayTrips);

                try
                {
                    // Excelファイル作成チェック
                    var createRs = CreateFile.CheckCreateExcel(tTripRecordDT, searchConditionDT, tmpFilename, sheetTwo, sheetNameOne, sheetNameTwo, gamenName, selectedTripNameAndBranchSeqs);

                    if (createRs.Item1)
                    {
                        var file = System.IO.File.ReadAllBytes(createRs.Item2);
                        CreateFile.DeleteFile(tmpFilename);

                        return Json(new { data = File(file, System.Net.Mime.MediaTypeNames.Application.Octet, tmpFilename) });
                    }
                    else
                    {
                        // エラーメッセージ取得
                        // 「ファイルが存在しません。」
                        errorMessage = ErrorMessagesResources.E9999;

                        return Json(new { res = "NG", error = errorMessage });
                    }
                }
                catch (Exception ex)
                {
                    // エラーメッセージ取得
                    // 「NASに接続できませんでした。」
                    errorMessage = ErrorMessagesResources.E9999;

                    // log取得
                    var exceptionMessage = ex.Message;
                    return Json(new { res = "NG", error = errorMessage + exceptionMessage });
                }
            }
            catch (Exception ex)
            {
                // エラーメッセージ取得
                // 「予期せぬエラーが発⽣しました。」
                errorMessage = "E9999: " + ErrorMessagesResources.E9999;

                // log取得
                var exceptionMessage = ex.Message;
                return Json(new { res = "NG", error = errorMessage + exceptionMessage });
            }

        }

        /// <summary>
        /// 便実績から予実差平均表示用のデータテーブルに変換する
        /// </summary>
        /// <param name="loadRecordList"></param>
        /// <param name="trips"></param>
        /// <param name="startOfPeriod"></param>
        /// <param name="endOfPeriod"></param>
        /// <returns></returns>
        public DataTable ConversionLoadRecordToDataTable(List<LoadRecordModel> loadRecordList, List<SelectedTripModel> trips, DateTime startOfPeriod, DateTime endOfPeriod)
        {
            var convertedTable = new DataTable();
            // 列情報設定
            convertedTable.Columns.Add("workDay");
            foreach(var trip in trips)
            {
                convertedTable.Columns.Add(trip.TripName + "_" + trip.TripBranchSeq);
            }
            // 日付ごとに行追加
            for (DateTime date = startOfPeriod; date <= endOfPeriod; date = date.AddDays(1))
            {
                var targetDateRecords = loadRecordList.FindAll(x => x.WorkDay == date);
                var dataRow = convertedTable.NewRow();
                dataRow["workDay"] = date.ToString("yyyy/MM/dd");
                foreach (var trip in trips)
                {
                    var targetDateRecord = targetDateRecords.Find(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq.ToString());
                    if (targetDateRecord != null)
                    {
                        var comparisonTime = new DateTime(1900, 1, 1, targetDateRecord.ArrivedAt.Hour, targetDateRecord.ArrivedAt.Minute, targetDateRecord.ArrivedAt.Second);
                        var timeDeff = GetTimeDeff(targetDateRecord.ArrivalScheduledTime, comparisonTime);

                        var timeDeffString = "";
                        if (timeDeff < 0) timeDeffString = $"{timeDeff}分";
                        else timeDeffString = $"+{timeDeff}分";

                        dataRow[trip.TripName + "_" + trip.TripBranchSeq] = timeDeffString;
                    }
                    else
                    {
                        dataRow[trip.TripName + "_" + trip.TripBranchSeq] = "-";
                    }
                }
                convertedTable.Rows.Add(dataRow);
            }
            // アラート範囲(早着)回数行追加
            convertedTable.Rows.Add(GetCountEarlyTimeOverRow(convertedTable, loadRecordList, trips));
            // アラート範囲(遅着)回数行追加
            convertedTable.Rows.Add(GetCountLateTimeOverRow(convertedTable, loadRecordList, trips));
            return convertedTable;
        }

        /// <summary>
        /// アラート範囲(早着)回数行追加
        /// </summary>
        /// <param name="convertedTable">追加先テーブル</param>
        /// <param name="loadRecordList">便実績リスト</param>
        /// <param name="trips">便リスト</param>
        /// <returns></returns>
        private DataRow GetCountEarlyTimeOverRow(DataTable convertedTable, List<LoadRecordModel> loadRecordList, List<SelectedTripModel> trips)
        {
            var dataRow = convertedTable.NewRow();
            dataRow["workDay"] = "アラート範囲(早着)";
            foreach (var trip in trips)
            {
                // 便毎に実績のリストを作成
                var targetTripRecords = loadRecordList.FindAll(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq.ToString());

                // 便毎のアラート範囲(早着)の回数取得
                var countEarlyTimeOver = GetCountEarlyTimeOver(targetTripRecords);

                dataRow[trip.TripName + "_" + trip.TripBranchSeq] = countEarlyTimeOver;
            }
            return dataRow;
        }

        /// <summary>
        /// アラート範囲(遅着)回数行追加
        /// </summary>
        /// <param name="convertedTable">追加先テーブル</param>
        /// <param name="loadRecordList">便実績リスト</param>
        /// <param name="trips">便リスト</param>
        /// <returns></returns>
        private DataRow GetCountLateTimeOverRow(DataTable convertedTable, List<LoadRecordModel> loadRecordList, List<SelectedTripModel> trips)
        {
            var dataRow = convertedTable.NewRow();
            dataRow["workDay"] = "アラート範囲(遅着)";
            foreach (var trip in trips)
            {
                // 便毎に実績のリストを作成
                var targetTripRecords = loadRecordList.FindAll(x => x.TripName == trip.TripName && x.TripBranchSeq == trip.TripBranchSeq.ToString());

                // 便毎のアラート範囲(遅着)の回数取得
                var countLateTimeOver = GetCountLateTimeOver(targetTripRecords);

                dataRow[trip.TripName + "_" + trip.TripBranchSeq] = countLateTimeOver;
            }
            return dataRow;
        }

        /// <summary>
        /// 「便名称_便枝番」のリスト作成
        /// </summary>
        /// <param name="trips"></param>
        /// <returns></returns>
        private List<string> GetTripNameAndBranchSeqs(List<SelectedTripModel> trips)
        {
            var tripNameAndBranchSeqs = new List<string>();
            foreach (var trip in trips)
            {
                var tripNameAndBranchSeq = $"{trip.TripName}_{trip.TripBranchSeq}";
                tripNameAndBranchSeqs.Add(tripNameAndBranchSeq);
            }
            return tripNameAndBranchSeqs;
        }
    }
}
