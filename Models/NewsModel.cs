namespace ai_truck_load_measurement.Models
{
    public class NewsModel : CommonModel
    {
        public int NewsID {  get; set; }

        public string NewsSubject {  get; set; }
        public string NewsContent {  get; set; }
        public int NewsGenreClass {  get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy {  get; set; }

    }

    public class NewsListViewModel : CommonModel
    {
        public List<NewsModel> NewsList { get; set; }
        public int NewsID { get; set; }
        public string NewsSubject { get; set; }
        public string NewsContent { get; set; }
        public int NewsGenreClass { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

    }
}
