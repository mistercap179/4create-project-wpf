using AutoMapper;
using BusinessLogic.Crud;
using BusinessLogic.Models;
using GalaSoft.MvvmLight.Command;
using log4net;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfClient.Models;

namespace WpfClient.ViewModels
{
    public class ModifyVacationViewModel : BaseViewModel
    {
        private readonly ICrud<Vacation, Guid> _vacationCrud;
        private readonly ICrud<Employee, Guid> _employeeCrud;
        private readonly IMapper _mapper;

        private static readonly ILog log = LogManager.GetLogger(typeof(ModifyVacationViewModel));

        public VacationModel SelectedVacation { get; set; }
        public EmployeeModel SelectedEmployee { get; set; }
        public ObservableCollection<EmployeeModel> Employees { get; set; }

        private DateTime _selectedDateFrom;
        public DateTime SelectedDateFrom
        {
            get => _selectedDateFrom;
            set
            {
                if (SelectedDateTo != default(DateTime) && value > SelectedDateTo)
                {
                    MessageBox.Show("Date From cannot be later than Date To.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _selectedDateFrom = value;
                OnPropertyChanged(nameof(SelectedDateFrom));
            }
        }


        private DateTime _selectedDateTo;
        public DateTime SelectedDateTo
        {
            get => _selectedDateTo;
            set
            {
                if (SelectedDateFrom != default(DateTime) && value < SelectedDateFrom)
                {
                    MessageBox.Show("Date To cannot be earlier than Date From.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _selectedDateTo = value;
                OnPropertyChanged(nameof(SelectedDateTo));
            }
        }


        private ICommand _modifyVacationCommand;
        public ICommand ModifyVacationCommand => _modifyVacationCommand;

        public ModifyVacationViewModel(ICrud<Employee, Guid> employeeCrud, ICrud<Vacation, Guid> vacationCrud, IMapper mapper, EmployeeModel employee, VacationModel vacation)
        {
            _employeeCrud = employeeCrud;
            _vacationCrud = vacationCrud;
            _mapper = mapper;

            Employees = new ObservableCollection<EmployeeModel> { employee };
            SelectedEmployee = employee;
            SelectedVacation = vacation;
            SelectedDateFrom = vacation.DateFrom;
            SelectedDateTo = vacation.DateTo;

            _modifyVacationCommand = new RelayCommand(SaveVacationChanges);
        }

        private async void SaveVacationChanges()
        {
            if (!ValidateInput()) return;

            int requestedWorkDays = WorkdayHelper.CountWorkdays(SelectedDateFrom, SelectedDateTo);
            var vacationToModify = SelectedVacation;

            int originalWorkDays = WorkdayHelper.CountWorkdays(vacationToModify.DateFrom, vacationToModify.DateTo);
            int difference = requestedWorkDays - originalWorkDays;

            // Adjust vacation days and employee's remaining vacation days
            try
            {
                if (difference < 0)
                {
                    await AdjustRemainingVacationDays(vacationToModify, Math.Abs(difference), false);  // Vacation reduced
                }
                else if (difference >= 0)
                {
                    await AdjustRemainingVacationDays(vacationToModify, difference, true);  // Vacation increased
                }

                log.Info($"Vacation modification successful: {SelectedVacation.ID} for employee {SelectedEmployee.ID}");
            }
            catch (Exception ex)
            {
                log.Error($"Error during vacation modification: {ex.Message}", ex);
            }
        }


        private bool ValidateInput()
        {
            if (SelectedEmployee == null || SelectedDateFrom == null || SelectedDateTo == null)
            {
                MessageBox.Show("Please fill in all fields before saving the vacation changes.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private async Task AdjustRemainingVacationDays(VacationModel vacationToModify, int difference, bool isIncrease)
        {
            // Handle insufficient vacation days when increasing
            if (isIncrease && SelectedEmployee.RemainingVacationDays < difference)
            {
                MessageBox.Show("The employee does not have enough remaining vacation days for the selected period.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Adjust the remaining vacation days
            if (isIncrease)
            {
                SelectedEmployee.RemainingVacationDays -= difference; 
            }
            else
            {
                SelectedEmployee.RemainingVacationDays += difference; 
            }
            // Update the vacation dates
            vacationToModify.DateFrom = SelectedDateFrom;
            vacationToModify.DateTo = SelectedDateTo;

            // Fetch the existing vacation entity from the database
            var vacationEntity = await _vacationCrud.GetByIdAsync(vacationToModify.ID);
            if (vacationEntity == null)
            {
                log.Warn("Vacation entity not found for update.");
                return;
            }

            // Update the vacation and employee data in the database
            await UpdateVacationAndEmployee(vacationEntity);

            // Display success message
            MessageBox.Show("Vacation modified successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            log.Info($"Vacation updated successfully: {vacationEntity.ID} for employee {SelectedEmployee.ID}");
            CloseWindow();
        }

        private async Task UpdateVacationAndEmployee(Vacation vacationEntity)
        {
            try
            {
                // Update the vacation entity with new dates
                vacationEntity.DateFrom = SelectedDateFrom;
                vacationEntity.DateTo = SelectedDateTo;
                await _vacationCrud.UpdateAsync(vacationEntity);

                // Update the employee's remaining vacation days in the database
                var employeeEntity = await _employeeCrud.GetByIdAsync(SelectedEmployee.ID);
                if (employeeEntity != null)
                {
                    employeeEntity.RemainingVacationDays = SelectedEmployee.RemainingVacationDays;
                    await _employeeCrud.UpdateAsync(employeeEntity);
                }

                log.Info($"Vacation and employee data updated successfully: Vacation ID = {vacationEntity.ID}, Employee ID = {SelectedEmployee.ID}");
            }
            catch (Exception ex)
            {
                log.Error($"Error updating vacation and employee: {ex.Message}", ex);
            }
        }

        private void CloseWindow()
        {
            System.Windows.Application.Current.Windows
                .OfType<System.Windows.Window>()
                .SingleOrDefault(w => w.IsActive)?.Close();
        }
    }

}
