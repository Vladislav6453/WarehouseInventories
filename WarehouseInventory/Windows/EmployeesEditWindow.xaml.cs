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

        var vm = DataContext as EmployeeEditViewModel;  // DataContext уже в XAML
        vm?.SetPasswordBinding(PasswordBox);             // Передаём PasswordBox
        vm?.SetClose(Close);
    }
}