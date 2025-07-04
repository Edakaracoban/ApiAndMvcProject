using System;
namespace hotelapi.Models
{
	public class AccountWithCustomerDto
	{
        public int AccountId { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public CustomerInfoDto Customer { get; set; }

        public class CustomerInfoDto
        {
            public int CustomerId { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Address { get; set; }
        }
    }
}

