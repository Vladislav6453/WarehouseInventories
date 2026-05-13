using System.Windows;
using Library.DB;
using Library.DTO;
using WarehouseInventory.ViewModels;

namespace WarehouseInventory.Windows;

public partial class CustomersEditWindow : Window
{
    public CustomersEditWindow(CustomerDTO? customer = null)
    {
        InitializeComponent();

        if (customer != null)
        {
            var vm = new CustomerEditViewModel(customer);
            vm.SetClose(Close);
            DataContext = vm;
        }
        else
        {
            var vm = new CustomerEditViewModel();
            vm.SetClose(Close);
            DataContext = vm;
        }
    }
}