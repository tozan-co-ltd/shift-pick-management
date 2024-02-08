namespace mar_sumaken_web.Models
{
    /// <summary>
    /// 品番マスター
    /// </summary>
    public class M_ProductModel
    {

        public List<M_DepoModel> RDepoProducts { get; set; } = new List<M_DepoModel>();

        /// <summary>
        /// 品番ID
        /// </summary>
        public int ProductID { get; set; }

        /// <summary>
        /// 仕入先ID: 会社マスターの会社IDと紐づく
        /// </summary>
        public int SupplierID { get; set; }

        /// <summary>
        /// 仕入先名
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// 仕入先品番
        /// </summary>
        public string SupplierProductNumber { get; set; }

        /// <summary>
        /// 納入先ID: 会社マスターの会社IDと紐づく
        /// </summary>
        public int DeliveryID { get; set; }

        /// <summary>
        /// 納入先名
        /// </summary>
        public string DeliveryName { get; set; }

        /// <summary>
        /// 納入先品番
        /// </summary>
        public string DeliveryProductNumber { get; set; }

        /// <summary>
        /// 品名
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 収容数
        /// </summary>
        public int LotQuantity { get; set; }

        /// <summary>
        /// 重複許容フラグ
        /// </summary>
        public bool AllowedDuplicatesFlag { get; set; }

        /// <summary>
        /// 未使用フラグ
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 作成者
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 更新者
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// 使用倉庫名
        /// </summary>
        public string UsedWarehouseName
        {
            get
            {
                string warehouseName = string.Empty;
                for (int i = 0; i < RDepoProducts.Count; i++)
                {
                    warehouseName += RDepoProducts[i].DepoName;

                    if (i < RDepoProducts.Count - 1)
                    {
                        warehouseName += ", ";
                    }
                }
                return warehouseName;
            }
        }

    }
}
