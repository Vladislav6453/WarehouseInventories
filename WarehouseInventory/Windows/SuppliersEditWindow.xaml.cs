using System.Windows;
using Library.DTO;
using WarehouseInventory.ViewModels;

namespace WarehouseInventory.Windows;

public partial class SuppliersEditWindow : Window
{
    public SuppliersEditWindow(SupplierDTO? supplier = null)
    {
        InitializeComponent();

        if (supplier != null)
        {
            var vm = new SupplierEditViewModel(supplier);
            vm.SetClose(Close);
            DataContext = vm;
        }
        else
        {
            var vm = new SupplierEditViewModel();
            vm.SetClose(Close);
            DataContext = vm;
        }
    }
}