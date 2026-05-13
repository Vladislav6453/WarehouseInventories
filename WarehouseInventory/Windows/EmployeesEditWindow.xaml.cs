using System.Windows;
using Library.DB;
using Library.DTO;
using WarehouseInventory.ViewModels;

namespace WarehouseInventory.Windows;

public partial class EmployeesEditWindow : Window
{
    public EmployeesEditWindow(EmployeeDTO employee)
    {
        InitializeComponent();

        var vm = new EmployeeEditViewModel(employee);
        vm.SetClose(Close);
        DataContext = vm;
    }
}