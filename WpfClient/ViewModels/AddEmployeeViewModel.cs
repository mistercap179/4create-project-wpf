using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using AutoMapper;
using BusinessLogic.Crud;
using BusinessLogic.Models;
using WpfClient.Models;
using log4net;

namespace WpfClient.ViewModels
{
    public class AddEmployeeViewModel : BaseViewModel
    {
        private readonly ICrud<Employee, Guid> _employeeCrud;
        private readonly IMapper _mapper;

        private static readonly ILog log = LogManager.GetLogger(typeof(AddEmployeeViewModel));

        private const int minVacationDays = 1;  
        private const int maxVacationDays = 30; 

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
            }
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
            }
        }

        private int _defaultVacationDays;
        public int DefaultVacationDays
        {
            get => _defaultVacationDays;
            set
            {
                _defaultVacationDays = value;
                OnPropertyChanged(nameof(DefaultVacationDays));
            }
        }

        private ICommand _addEmployeeCommand;
        public ICommand AddEmployeeCommand => _addEmployeeCommand;

        public AddEmployeeViewModel(ICrud<Employee, Guid> employeeCrud,IMapper mapper)
        {
            _employeeCrud = employeeCrud;
            _mapper = mapper;
            _addEmployeeCommand = new RelayCommand(AddEmployee);
        }

        private async void AddEmployee()
        {
            try
            {
                if (IsEmployeeValid(FirstName, LastName, DefaultVacationDays))
                {
                    EmployeeModel newEmployee = new EmployeeModel(FirstName, LastName, DefaultVacationDays);

                    log.Info($"Attempting to add employee: {FirstName} {LastName} with {DefaultVacationDays} vacation days.");

                    await _employeeCrud.AddAsync(_mapper.Map<Employee>(newEmployee));

                    MessageBox.Show($"Employee {FirstName} {LastName} added with {DefaultVacationDays} vacation days.",
                                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow();

                    log.Info($"Employee {FirstName} {LastName} successfully added.");
                }
                else
                {
                    MessageBox.Show("Please fill in all fields with valid values.",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);

                    log.Warn("Validation failed for new employee: Missing or invalid fields.");
                }
            }
            catch (Exception ex)
            {
                log.Error($"An unexpected error occurred while adding employee {FirstName} {LastName}.", ex);

                MessageBox.Show($"An unexpected error occurred: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private bool IsEmployeeValid(string firstName, string lastName, int vacationDays)
        {
            return !string.IsNullOrWhiteSpace(firstName) &&
                   !string.IsNullOrWhiteSpace(lastName) &&
                   vacationDays > minVacationDays && vacationDays <= maxVacationDays;
        }

        private void CloseWindow()
        {
            System.Windows.Application.Current.Windows
                .OfType<System.Windows.Window>()
                .SingleOrDefault(w => w.IsActive)?.Close();
        }
    }
}
