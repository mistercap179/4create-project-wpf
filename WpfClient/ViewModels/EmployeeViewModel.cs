using AutoMapper;
using BusinessLogic.Crud;
using BusinessLogic.Models;
using GalaSoft.MvvmLight.Command;
using log4net;
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

        private static readonly ILog log = LogManager.GetLogger(typeof(EmployeeViewModel));

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

                log.Info("Successfully loaded employees.");

            }
            catch (InvalidOperationException ex)
            {
                log.Error($"Error loading employees: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                log.Error($"Unexpected error loading employees: {ex.Message}", ex);
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
                    log.Info("AddEmployeeView closed.");
                };

                secondWindow.Show();
            }
            catch (Exception ex)
            {
                log.Error($"Error showing add employee view: {ex.Message}", ex);
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
                        log.Info("VacationHistoryView closed.");
                    };

                    secondWindow.Show();
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error showing vacation history: {ex.Message}", ex);
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
                    log.Info("AddVacationView closed.");
                };

                secondWindow.Show();
            }
            catch (Exception ex)
            {
                log.Error($"Error showing add vacation view: {ex.Message}", ex);
            }
        }

    }

}
