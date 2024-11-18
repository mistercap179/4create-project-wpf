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

namespace WpfClient.ViewModels
{
    public class AddEmployeeViewModel : BaseViewModel
    {
        private readonly ICrud<Employee, Guid> _employeeCrud;
        private readonly IMapper _mapper;

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

        public ICommand AddEmployeeCommand { get; }

        public AddEmployeeViewModel(ICrud<Employee, Guid> employeeCrud,IMapper mapper)
        {
            _employeeCrud = employeeCrud;
            _mapper = mapper;
            AddEmployeeCommand = new RelayCommand(AddEmployee);
        }

        private async void AddEmployee()
        {
            try
            {
                if (IsEmployeeValid(FirstName, LastName, DefaultVacationDays))
                {
                    EmployeeModel newEmployee = new EmployeeModel(FirstName, LastName, DefaultVacationDays);

                    await _employeeCrud.AddAsync(_mapper.Map<Employee>(newEmployee));

                    MessageBox.Show($"Employee {FirstName} {LastName} added with {DefaultVacationDays} vacation days.",
                                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow();
                }
                else
                {
                    MessageBox.Show("Please fill in all fields with valid values.",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}",
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
