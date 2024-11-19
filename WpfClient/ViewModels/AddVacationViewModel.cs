using AutoMapper;
using BusinessLogic.Crud;
using BusinessLogic.Models;
using GalaSoft.MvvmLight.Command;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfClient.Models;
using log4net;  // Dodaj log4net

namespace WpfClient.ViewModels
{
    public class AddVacationViewModel : BaseViewModel
    {
        private readonly ICrud<Vacation, Guid> _vacationCrud;
        private readonly ICrud<Employee, Guid> _employeeCrud;
        private readonly IMapper _mapper;

        private static readonly ILog log = LogManager.GetLogger(typeof(AddVacationViewModel)); 

        public IEnumerable<EmployeeModel> Employees { get; set; }
        public EmployeeModel SelectedEmployee { get; set; }

        private DateTime _selectedDateFrom = DateTime.Today;
        public DateTime SelectedDateFrom
        {
            get => _selectedDateFrom;
            set
            {
                if (_selectedDateFrom != value)
                {
                    if (value > SelectedDateTo)
                    {
                        MessageBox.Show("The start date cannot be later than the end date.", "Invalid Date Range", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    _selectedDateFrom = value;
                    OnPropertyChanged(nameof(SelectedDateFrom));
                }
            }
        }

        private DateTime _selectedDateTo = DateTime.Today.AddDays(1);
        public DateTime SelectedDateTo
        {
            get => _selectedDateTo;
            set
            {
                if (_selectedDateTo != value)
                {
                    if (value < SelectedDateFrom)
                    {
                        MessageBox.Show("The end date cannot be earlier than the start date.", "Invalid Date Range", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    _selectedDateTo = value;
                    OnPropertyChanged(nameof(SelectedDateTo));
                }
            }
        }

        private ICommand _addVacationCommand;
        public ICommand AddVacationCommand => _addVacationCommand;

        public AddVacationViewModel(ICrud<Employee, Guid> employeeCrud, ICrud<Vacation, Guid> vacationCrud, IMapper mapper)
        {
            _employeeCrud = employeeCrud;
            _vacationCrud = vacationCrud;
            _mapper = mapper;
            _addVacationCommand = new RelayCommand(AddVacation);
            LoadEmployees();
        }

        private async void LoadEmployees()
        {
            try
            {
                var employeesFromDb = await _employeeCrud.GetAllAsync();

                Employees = new ObservableCollection<EmployeeModel>(
                    _mapper.Map<IEnumerable<Employee>, IEnumerable<EmployeeModel>>(employeesFromDb));

                OnPropertyChanged(nameof(Employees));
                log.Info("Employees loaded successfully.");
            }
            catch (InvalidOperationException ex)
            {
                log.Error("Error loading employees due to invalid operation.", ex);
            }
            catch (Exception ex)
            {
                log.Error("Unexpected error loading employees.", ex);
            }
        }

        private async void AddVacation()
        {
            if (!ValidateInput()) return;

            int requestedWorkDays = WorkdayHelper.CountWorkdays(SelectedDateFrom, SelectedDateTo);

            if (SelectedEmployee.RemainingVacationDays >= requestedWorkDays)
            {
                var newVacation = new VacationModel(SelectedEmployee.ID, SelectedDateFrom, SelectedDateTo);
                var vacationEntity = _mapper.Map<Vacation>(newVacation);

                try
                {
                    SelectedEmployee.Vacations.Add(newVacation);
                    await _vacationCrud.AddAsync(vacationEntity);

                    // Update remaining vacation days
                    await UpdateEmployeeRemainingVacationDays(requestedWorkDays);

                    MessageBox.Show("Vacation added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    log.Info($"Vacation for employee {SelectedEmployee.ID} added successfully.");
                    CloseWindow();
                }
                catch (InvalidOperationException ex)
                {
                    log.Error("Error adding vacation due to invalid operation.", ex);
                }
                catch (Exception ex)
                {
                    log.Error("Unexpected error adding vacation.", ex);
                }
            }
            else
            {
                MessageBox.Show("The employee does not have enough remaining vacation days for the selected period.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                log.Warn("Attempt to add vacation failed due to insufficient remaining vacation days.");
            }
        }

        private async Task UpdateEmployeeRemainingVacationDays(int requestedWorkDays)
        {
            SelectedEmployee.RemainingVacationDays -= requestedWorkDays;

            try
            {
                var employeeEntity = await _employeeCrud.GetByIdAsync(SelectedEmployee.ID);
                if (employeeEntity != null)
                {
                    employeeEntity.RemainingVacationDays = SelectedEmployee.RemainingVacationDays;
                    employeeEntity.Vacations = _mapper.Map<List<Vacation>>(SelectedEmployee.Vacations);

                    await _employeeCrud.UpdateAsync(employeeEntity);
                    log.Info($"Employee {SelectedEmployee.ID}'s remaining vacation days updated.");
                }
            }
            catch (InvalidOperationException ex)
            {
                log.Error("Error updating employee vacation data due to invalid operation.", ex);
            }
            catch (Exception ex)
            {
                log.Error("Unexpected error updating employee vacation data.", ex);
            }
        }

        private bool ValidateInput()
        {
            if (SelectedEmployee == null || SelectedDateFrom == null || SelectedDateTo == null)
            {
                MessageBox.Show("Please fill in all fields before saving.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                log.Warn("Invalid input detected while adding vacation: Missing employee or dates.");
                return false;
            }
            return true;
        }

        private void CloseWindow()
        {
            Application.Current.Windows
                .OfType<Window>()
                .SingleOrDefault(w => w.IsActive)?.Close();
        }
    }
}
