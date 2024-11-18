using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Models
{
    public class Vacation
    {
        [Key]
        public Guid ID { get; set; } 

        [Required]
        public Guid EmployeeID { get; set; } 

        [Required]
        public DateTime DateFrom { get; set; } 

        [Required]
        public DateTime DateTo { get; set; } 

        public Vacation(Guid employeeID, DateTime dateFrom, DateTime dateTo)
        {
            ID = Guid.NewGuid(); 
            EmployeeID = employeeID;
            DateFrom = dateFrom;
            DateTo = dateTo;
        }
    }
}
