using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Models
{
    public class Employee
    {
        [Key]
        public Guid ID { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public int RemainingVacationDays { get; set; }

        public virtual ICollection<Vacation> Vacations { get; set; } = new List<Vacation>();

        // Constructor to initialize the properties
        public Employee(string firstName, string lastName, int remainingVacationDays)
        {
            ID = Guid.NewGuid(); // Generate a new unique GUID for each employee
            FirstName = firstName;
            LastName = lastName;
            RemainingVacationDays = remainingVacationDays;
        }
    }

}
