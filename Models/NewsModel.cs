using shift_pick_management.Properties;
using System.ComponentModel.DataAnnotations;

namespace shift_pick_management.Models
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
        [StringLength(300, ErrorMessageResourceName ="E1015", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public string NewsContent { get; set; }
        [Display(Name = "登録日付")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public DateTime NewsDate { get; set; }
        [Display(Name ="カテゴリ")]
        [Required(ErrorMessageResourceName = "E1001", ErrorMessageResourceType = typeof(ErrorMessagesResources))]
        public int CategoryClass { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        public int AuthorizedKubun { get; set; }
    }
}
