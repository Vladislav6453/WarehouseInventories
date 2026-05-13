using System.Windows;
using WarehouseInventory.ViewModels;

namespace WarehouseInventory.Windows;

public partial class InvoicesEditWindow : Window
{
    public InvoicesEditWindow()
    {
        InitializeComponent();
        var vm = new InvoiceEditViewModel();
        vm.SetClose(Close);
        DataContext = vm;
    }
}