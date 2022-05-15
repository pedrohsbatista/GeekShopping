namespace GeekShopping.Web.Models
{
    public class CardViewModel
    {
        public CardHeaderViewModel CartHeader { get; set; }

        public IEnumerable<CardDetailViewModel> CartDetails { get; set; }
    }
}
