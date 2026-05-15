using System.Windows;
using WarehouseInventory.ViewModels;

namespace WarehouseInventory.Windows;

public partial class InvoicesWindow : Window
{
    public InvoicesWindow()
    {
        InitializeComponent();
        var vm = DataContext as InvoiceViewModel;
        vm.SetCurrentWindow(this);
    }
}