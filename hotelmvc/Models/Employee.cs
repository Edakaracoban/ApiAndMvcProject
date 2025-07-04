using System;
using System.ComponentModel.DataAnnotations;

namespace hotelmvc.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }


        public string Name { get; set; }


        public string Position { get; set; }

        public int BranchId { get; set; }
        public Branch Branch { get; set; }


    }
}
