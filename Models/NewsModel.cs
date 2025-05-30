using ai_truck_load_measurement.Properties;
using System.ComponentModel.DataAnnotations;

namespace ai_truck_load_measurement.Models
{
    public class NewsModel : CommonModel
    {
        public int NewsID {  get; set; }

        public string NewsContent {  get; set; }
        public int CategoryClass {  get; set; }
        public string? CategoryStatus { get; set; }
        public DateTime NewsDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy {  get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

    }

    public class NewsListViewModel : CommonModel
    {
        public List<NewsModel> NewsList { get; set; }
        public int NewsID { get; set; }
        [Display(Name="本文")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string NewsContent { get; set; }
        [Display(Name = "登録日付")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public DateTime NewsDate { get; set; }
        [Display(Name ="カテゴリ")]
        public int CategoryClass { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        public int AuthorizedKubun { get; set; }
    }
}
