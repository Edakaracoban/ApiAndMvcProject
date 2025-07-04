using System;
using System.ComponentModel.DataAnnotations;

namespace hotelmvc.Models
{
    public class Branch
    {
        [Key]
        public int BranchId { get; set; }

        public string BranchName { get; set; }

        public string Location { get; set; }

        public ICollection<Employee> Employees { get; set; }
    }
}