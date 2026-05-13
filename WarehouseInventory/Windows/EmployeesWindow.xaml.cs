using System.Windows;
using WarehouseInventory.ViewModels;

namespace WarehouseInventory.Windows;

public partial class EmployeesWindow : Window
{
    public EmployeesWindow()
    {
        InitializeComponent();
        DataContext = new EmployeeViewModel();
    }
}