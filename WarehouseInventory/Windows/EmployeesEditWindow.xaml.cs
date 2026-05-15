using System.Windows;
using Library.DB;
using Library.DTO;
using WarehouseInventory.ViewModels;

namespace WarehouseInventory.Windows;

public partial class EmployeesEditWindow : Window
{
    public EmployeesEditWindow(EmployeeDTO employee)
    {
        /*InitializeComponent();
    
        var vm = DataContext as EmployeeEditViewModel(); 
        vm?.SetPasswordBinding(PasswordBox);             
        vm?.SetClose(Close);
        DataContext = vm;*/
        
        InitializeComponent();
        
        var vm = new EmployeeEditViewModel(employee);
        vm.SetPasswordBinding(PasswordBox);
        vm.SetClose(Close);
        DataContext = vm;
    }
}