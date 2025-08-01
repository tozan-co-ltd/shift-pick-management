using ai_truck_load_measurement.Commons;
using ai_truck_load_measurement.ConnectControllers;
using ai_truck_load_measurement.Models;
using Microsoft.AspNetCore.Mvc;

namespace ai_truck_load_measurement.Controllers
{
    public class LoadProgressController : BaseController
    {
        public IActionResult Index()
        {
            LoadProgressModel model = new();
            var user = ClaimsLoginUserData();
            model.SelectedDepo = new M_DepoModel { 
                DepoID = user.MainDepoID,
                Name = user.MainDepoName
            };
            return View(model);
        }

        // 進捗表作成
        public LoadProgressModel GetLoadProgress(int depoId, DateTime workDay)
        {
            var model = new LoadProgressModel();

            // 便予定情報取得
            var trips = GetTrips(depoId);

            // 便実績情報取得
            var loadRecords = GetLoadRecords(depoId, workDay);

            // 便実績紐づけチェック
            model.LoadRecords = CheckLoadRecords(loadRecords, trips);
            model.Trips = trips;

            return model;
        }

        // 便予定情報取得
        public static List<M_TripModel> GetTrips(int depoId)
        {
            var sql = LoadProgressConnectController.CreateSQLToSelectM_TripsFromDepo(depoId);
            var trips = ConnectToSQLServer.ExecuteQueryToList<M_TripModel>(sql);
            return trips;
        }

        // 便実績情報取得
        public static List<LoadRecordModel> GetLoadRecords(int depoId, DateTime workDay)
        {
            var sql = LoadProgressConnectController.CreateSQLToSelectLoadRecordsFromDepoAndWorkDay(depoId, workDay);
            var loadRecords = ConnectToSQLServer.ExecuteQueryToList<LoadRecordModel>(sql);
            return loadRecords;
        }

        // 便実績紐づけチェック
        public List<LoadRecordModel> CheckLoadRecords(List<LoadRecordModel> loadRecords, List<M_TripModel> trips)
        {
            foreach (var loadRecord in loadRecords)
            {
                // 便名称が
                if (string.IsNullOrEmpty(loadRecord.TripName) && !string.IsNullOrEmpty(loadRecord.IdentifyNumber))
                {
                    var trip = trips.Find(x => x.IdentifyNumber == loadRecord.IdentifyNumber);
                    if (trip != null)
                        loadRecord.TripName = trip.TripName;
                }
            }
            return loadRecords;
        }
    }
}
