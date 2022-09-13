

namespace ConsumerOrders.Models
{
    public class MessageDto
    {
        public MessageDto()
        {
            ProductsList = new List<ProductDto>();
        }

        // Order
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal Total { get; set; }


        // Customer

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FiscalCode { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PIva { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Cap { get; set; }
        
       
        public List<ProductDto> ProductsList { get; set; }


    }
}
