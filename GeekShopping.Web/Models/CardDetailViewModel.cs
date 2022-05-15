namespace GeekShopping.Web.Models
{
    public class CardDetailViewModel
    {
        public long Id { get; set; }

        public long CardHeaderId { get; set; }

        public CardHeaderViewModel CardHeader { get; set; }

        public long ProductId { get; set; }

        public ProductViewModel Product { get; set; }

        public int Count { get; set; }
    }
}
