using System.Windows;
using WarehouseInventory.ViewModels;

namespace WarehouseInventory.Windows;

public partial class SuppliersWindow : Window
{
    public SuppliersWindow()
    {
        InitializeComponent();
        var vm = DataContext as SupplierViewModel;
        vm.SetCurrentWindow(this);
    }
}