using System.Windows;
using Library.DTO;
using WarehouseInventory.ViewModels;

namespace WarehouseInventory.Windows;

public partial class EmployeesWindow : Window
{
    public EmployeesWindow()
    {
        InitializeComponent();
        var vm = DataContext as EmployeeViewModel;
        vm.SetCurrentWindow(this);
    }
}