namespace hotelapi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }  // Gerçek projede hash’lenmiş olmalı
        public string Email { get; set; }
    }
}
