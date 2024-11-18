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
        private EmployeeModel _employee;

        public VacationHistoryViewModel(ICrud<Vacation, Guid> vacationCrud, ICrud<Employee, Guid> employeeCrud, IMapper mapper,EmployeeModel employee)
        {
            _vacationCrud = vacationCrud;
            _employeeCrud= employeeCrud;
            _mapper = mapper;
            _employee = employee;
            LoadVacations();
        }

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

        public int TotalVacationDaysUsed=> Vacations?.Sum(v => WorkdayHelper.CountWorkdays(v.DateFrom, v.DateTo)) ?? 0;

        public int RemainingVacationDays => _employee.RemainingVacationDays;
       
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
            // Fetch the employee from the database again to get the updated RemainingVacationDays
            var updatedEmployee = await _employeeCrud.GetByIdAsync(employeeId);
            if (updatedEmployee == null)
            {
                Console.WriteLine("Vacation entity not found.", "Error");
                return;
            }

            // Map the employee data to EmployeeModel
            _employee = _mapper.Map<EmployeeModel>(updatedEmployee);

            // Set Vacations to the updated list
            Vacations = new ObservableCollection<VacationModel>(_employee.Vacations);

            // Trigger the necessary UI updates
            OnPropertyChanged(nameof(RemainingVacationDays));
            OnPropertyChanged(nameof(TotalVacationDaysUsed));
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

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while deleting the vacation: {ex.Message}\n{ex.StackTrace}");
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
                        EmployeeModel employeToEdit = _mapper.Map<EmployeeModel>(employee);
                        var modifyVacationViewModel = new ModifyVacationViewModel(_employeeCrud, _vacationCrud, _mapper, employeToEdit, vacation);
                        var secondWindow = new ModifyVacationView
                        {
                            DataContext = modifyVacationViewModel
                        };

                        secondWindow.Closed += async (s, e) =>
                        {
                            await RefreshEmployeeData(vacation.EmployeeID);
                        };

                        secondWindow.Show();
                    }
                }
                catch (Exception ex) 
                {
                    Console.WriteLine($"An error occurred while editing the vacation: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
    }
}
