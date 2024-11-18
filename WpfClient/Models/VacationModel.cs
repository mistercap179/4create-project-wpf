using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient.Models
{
    public class VacationModel
    {
        public Guid ID { get; set; }
        public Guid EmployeeID { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public VacationModel(Guid employeeID, DateTime dateFrom, DateTime dateTo)
        {
            ID = Guid.NewGuid();
            EmployeeID = employeeID;
            DateFrom = dateFrom;
            DateTo = dateTo;
        }
    }
}
