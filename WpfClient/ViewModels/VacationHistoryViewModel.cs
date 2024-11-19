using AutoMapper;
using BusinessLogic.Crud;
using BusinessLogic.Models;
using GalaSoft.MvvmLight.Command;
using MvvmHelpers;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfClient.Models;
using WpfClient.Views;

namespace WpfClient.ViewModels
{
    public class VacationHistoryViewModel : BaseViewModel
    {
        private readonly ICrud<Vacation, Guid> _vacationCrud;
        private readonly ICrud<Employee, Guid> _employeeCrud;
        private readonly IMapper _mapper;

        private static readonly ILog log = LogManager.GetLogger(typeof(VacationHistoryViewModel));

        private EmployeeModel _employee;

        private ObservableCollection<VacationModel> _vacations;

        public ObservableCollection<VacationModel> Vacations
        {
            get { return _vacations; }
            set { _vacations = value; OnPropertyChanged(); }
        }

        private ICommand _deleteVacationCommand;
        public ICommand DeleteVacationCommand => _deleteVacationCommand;

        private ICommand _editVacationCommand;
        public ICommand EditVacationCommand => _editVacationCommand;

        public int TotalVacationDaysUsed => Vacations?.Sum(v => WorkdayHelper.CountWorkdays(v.DateFrom, v.DateTo)) ?? 0;

        public int RemainingVacationDays => _employee.RemainingVacationDays;

        public VacationHistoryViewModel(ICrud<Vacation, Guid> vacationCrud, ICrud<Employee, Guid> employeeCrud, IMapper mapper, EmployeeModel employee)
        {
            _vacationCrud = vacationCrud;
            _employeeCrud = employeeCrud;
            _mapper = mapper;
            _employee = employee;
            LoadVacations();
        }

        private async Task LoadVacations()
        {
            Vacations = new ObservableCollection<VacationModel>(_employee.Vacations);
            _deleteVacationCommand = new RelayCommand<VacationModel>(DeleteVacation);
            _editVacationCommand = new RelayCommand<VacationModel>(EditVacation);
            OnPropertyChanged(nameof(Vacations));
            OnPropertyChanged(nameof(TotalVacationDaysUsed));
        }

        private async Task RefreshEmployeeData(Guid employeeId)
        {
            var updatedEmployee = await _employeeCrud.GetByIdAsync(employeeId);
            if (updatedEmployee == null)
            {
                log.Error("Employee not found while refreshing data.");
                return;
            }

            _employee = _mapper.Map<EmployeeModel>(updatedEmployee);
            Vacations = new ObservableCollection<VacationModel>(_employee.Vacations);

            OnPropertyChanged(nameof(RemainingVacationDays));
            OnPropertyChanged(nameof(TotalVacationDaysUsed));

            log.Info("Employee data refreshed successfully.");
        }

        private async void DeleteVacation(VacationModel vacation)
        {
            if (vacation != null)
            {
                try
                {
                    var result = MessageBox.Show("Are you sure you want to delete this vacation?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        Vacations.Remove(vacation);

                        int requestedWorkDays = WorkdayHelper.CountWorkdays(vacation.DateFrom, vacation.DateTo);
                        await _vacationCrud.DeleteAsync(vacation.ID);

                        var employee = await _employeeCrud.GetByIdAsync(vacation.EmployeeID);
                        if (employee != null)
                        {
                            employee.RemainingVacationDays += requestedWorkDays;
                            await _employeeCrud.UpdateAsync(_mapper.Map<Employee>(employee));

                            await RefreshEmployeeData(vacation.EmployeeID);

                            log.Info($"Vacation with ID {vacation.ID} deleted successfully.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Error occurred while deleting vacation: {ex.Message}", ex);
                }
            }
        }

        private async void EditVacation(VacationModel vacation)
        {
            if (vacation != null)
            {
                try
                {
                    var employee = await _employeeCrud.GetByIdAsync(vacation.EmployeeID);

                    if (employee != null)
                    {
                        EmployeeModel employeeToEdit = _mapper.Map<EmployeeModel>(employee);

                        var modifyVacationViewModel = new ModifyVacationViewModel(_employeeCrud, _vacationCrud, _mapper, employeeToEdit, vacation);

                        var secondWindow = new ModifyVacationView
                        {
                            DataContext = modifyVacationViewModel
                        };

                        secondWindow.Closed += async (s, e) =>
                        {
                            await RefreshEmployeeData(vacation.EmployeeID);
                        };

                        secondWindow.Show();

                        log.Info("Vacation edit window opened successfully.");
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"Error occurred while showing edit vacation window: {ex.Message}", ex);
                }
            }
        }
    }
}
