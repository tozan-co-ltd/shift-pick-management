namespace mar_sumaken_web.Models
{
    //Itemモデル
    public class SelectItem
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public bool IsSelected { get; set; } = false;
    }
}
