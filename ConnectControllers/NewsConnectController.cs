using ai_truck_load_measurement.Models;

namespace ai_truck_load_measurement.ConnectControllers
{
    public class NewsConnectController
    {
        /// <summary>
        /// お知らせ情報取得SQL
        /// </summary>
        /// <returns></returns>
        public static string CreateSQLToSelectNews()
        {
            var sql = $@"
            SELECT
                news_id
                ,news_content
                ,category_class
                ,news_date
                ,created_at
                ,created_by
            FROM t_news
            ORDER BY news_date DESC , news_id DESC
            ";
            return sql;
        }

        /// <summary>
        /// お知らせ情報登録SQL
        /// </summary>
        /// <param name="model"></param>
        /// <param name="createdAt"></param>
        /// <param name="createdBy"></param>
        /// <returns></returns>
        public static string CreateSQLToInsertNews(NewsModel model, DateTime createdAt, string createdBy)
        {
            var sql = $@"
                INSERT INTO t_news(
                    news_content
                    ,category_class
                    ,news_date
                    ,created_at
                    ,created_by
                    ,updated_at
                    ,updated_by
                )
                VALUES (
                    '{model.NewsContent}',
                    '{model.CategoryClass}',
                    '{model.NewsDate}',
                    '{createdAt.ToString("yyyy/MM/dd HH:mm:ss")}',
                    '{createdBy}',
                    '{createdAt.ToString("yyyy/MM/dd HH:mm:ss")}',
                    '{createdBy}'
                );
            ";
            return sql;
        }


        public static string CreateSQLToUpdateNews(NewsModel model, DateTime updatedAt, string updatedBy)
        {
            var sql = $@"
                UPDATE t_news
                SET
                    category_class = '{model.CategoryClass}'
                    ,news_content = '{model.NewsContent}'
                    ,news_date = '{model.NewsDate}'
                    ,updated_at = '{updatedAt.ToString("yyyy/MM/dd HH:mm:ss")}'
                    ,updated_by = '{updatedBy}'
                WHERE
                    news_id = '{model.NewsID}'
            ";
            return sql;
        }
    }
}