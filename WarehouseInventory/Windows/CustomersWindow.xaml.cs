using System.Windows;
using WarehouseInventory.ViewModels;

namespace WarehouseInventory.Windows;

public partial class CustomersWindow : Window
{
    public CustomersWindow()
    {
        InitializeComponent();
        DataContext = new CustomerViewModel();
    }
}