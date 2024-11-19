using BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient.Models
{
    public class EmployeeModel
    {
        public Guid ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int RemainingVacationDays { get; set; }
        public ICollection<VacationModel> Vacations { get; set; } = new List<VacationModel>();

        public EmployeeModel(string firstName, string lastName, int remainingVacationDays)
        {
            ID = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            RemainingVacationDays = remainingVacationDays;
        }
    }
}
