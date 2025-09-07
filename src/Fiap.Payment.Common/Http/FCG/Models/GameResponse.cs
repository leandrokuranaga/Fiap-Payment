namespace Fiap.Payment.Common.Http.FCG.Models
{
    public class GameResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Genre { get; set; }
        public decimal Price { get; set; }
        public int? PromotionId { get; set; }
        public decimal PriceWithDiscount { get; set; }
    }
}
