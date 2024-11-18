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

namespace WpfClient.ViewModels
{
    public class AddVacationViewModel : BaseViewModel
    {
        private readonly ICrud<Vacation, Guid> _vacationCrud;
        private readonly ICrud<Employee, Guid> _employeeCrud;
        private readonly IMapper _mapper;

        public ICommand AddVacationCommand { get; }
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
                    _selectedDateTo = value;
                    OnPropertyChanged(nameof(SelectedDateTo));
                }
            }
        }
        public AddVacationViewModel(ICrud<Employee, Guid> employeeCrud, ICrud<Vacation, Guid> vacationCrud, IMapper mapper)
        {
            _employeeCrud = employeeCrud;
            _vacationCrud = vacationCrud;
            _mapper = mapper;
            AddVacationCommand = new RelayCommand(AddVacation);
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
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error loading employees: {ex.Message}\n{ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error loading employees: {ex.Message}\n{ex.StackTrace}");
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
                    // Add the new vacation
                    SelectedEmployee.Vacations.Add(newVacation);
                    await _vacationCrud.AddAsync(vacationEntity);

                    // Update remaining vacation days
                    await UpdateEmployeeRemainingVacationDays(requestedWorkDays);

                    MessageBox.Show("Vacation added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow();
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Error adding vacation: {ex.Message}\n{ex.StackTrace}");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error adding vacation: {ex.Message}\n{ex.StackTrace}");
                }
            }
            else
            {
                MessageBox.Show("The employee does not have enough remaining workdays for the selected period.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error updating employee vacation data: {ex.Message}\n{ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error updating employee vacation data: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private bool ValidateInput()
        {
            if (SelectedEmployee == null || SelectedDateFrom == null || SelectedDateTo == null)
            {
                MessageBox.Show("Please fill in all fields before saving.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
