using AutoMapper;
using BusinessLogic.Crud;
using BusinessLogic.Models;
using GalaSoft.MvvmLight.Command;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfClient.Models;
using WpfClient.Views;

namespace WpfClient.ViewModels
{
    public class EmployeeViewModel : BaseViewModel
    {
        private readonly ICrud<BusinessLogic.Models.Employee, Guid> _employeeCrud;
        private readonly ICrud<BusinessLogic.Models.Vacation, Guid> _vacationCrud;
        private readonly IMapper _mapper;
        
        private ObservableCollection<EmployeeModel> _employees;

        public ObservableCollection<EmployeeModel> Employees
        {
            get { return _employees; }
            set { _employees = value; OnPropertyChanged(); }
        }

        private EmployeeModel _selectedEmployee;
        public EmployeeModel SelectedEmployee
        {
            get { return _selectedEmployee; }
            set
            {
                if (_selectedEmployee != value)
                {
                    _selectedEmployee = value;
                    OnPropertyChanged();

                    if (_selectedEmployee != null)
                    {
                        ShowVacationHistoryCommand.Execute(_selectedEmployee);
                    }
                }
            }
        }

        private ICommand _addEmployeeCommand;
        public ICommand AddEmployeeCommand => _addEmployeeCommand;

        private ICommand _addVacationCommand;
        public ICommand AddVacationCommand => _addVacationCommand;

        private ICommand _showVacationHistoryCommand;
        public ICommand ShowVacationHistoryCommand => _showVacationHistoryCommand;


        public EmployeeViewModel(ICrud<BusinessLogic.Models.Employee, Guid> employeeCrud, ICrud<BusinessLogic.Models.Vacation, Guid> vacationCrud, IMapper mapper)
        {
            _employeeCrud = employeeCrud;
            _vacationCrud = vacationCrud;
            _mapper = mapper;
            _addEmployeeCommand = new RelayCommand(async () => await AddEmployee());
            _showVacationHistoryCommand = new RelayCommand<EmployeeModel>(ShowVacationHistory);
            _addVacationCommand = new RelayCommand(AddVacation);
            LoadEmployees();
        }

        private async Task LoadEmployees()
        {
            try
            {
                var employeesFromDb = await _employeeCrud.GetAllAsync();

                Employees = new ObservableCollection<EmployeeModel>(
                    _mapper.Map<IEnumerable<Employee>, IEnumerable<EmployeeModel>>(employeesFromDb));

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


        private async Task AddEmployee()
        {
            try
            {
                var addEmployeeViewModel = new AddEmployeeViewModel(_employeeCrud, _mapper);
                var secondWindow = new AddEmployeeView
                {
                    DataContext = addEmployeeViewModel
                };

                secondWindow.Closed += async (s, e) =>
                {
                    await LoadEmployees(); 
                };

                secondWindow.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding employee: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ShowVacationHistory(EmployeeModel selectedEmployee)
        {
            try
            {
                if (selectedEmployee != null)
                {
                    var historyVacationViewModel = new VacationHistoryViewModel(_vacationCrud, _employeeCrud, _mapper, selectedEmployee);
                    var secondWindow = new VacationHistoryView
                    {
                        DataContext = historyVacationViewModel
                    };

                    secondWindow.Closed += async (s, e) =>
                    {
                        await LoadEmployees(); 
                    };

                    secondWindow.Show();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing vacation history: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void AddVacation()
        {
            try
            {
                var addVacationViewModel = new AddVacationViewModel(_employeeCrud, _vacationCrud, _mapper);
                var secondWindow = new AddVacationView
                {
                    DataContext = addVacationViewModel
                };

                secondWindow.Closed += async (s, e) =>
                {
                    await LoadEmployees(); 
                };

                secondWindow.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding vacation: {ex.Message}\n{ex.StackTrace}");
            }
        }

    }

}
