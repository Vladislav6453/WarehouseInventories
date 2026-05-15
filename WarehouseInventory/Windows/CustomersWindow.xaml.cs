using System.Windows;
using WarehouseInventory.ViewModels;

namespace WarehouseInventory.Windows;

public partial class CustomersWindow : Window
{
    public CustomersWindow()
    {
        InitializeComponent();
        var vm = DataContext as CustomerViewModel;
        vm.SetCurrentWindow(this);
    }
}