using System.Windows;
using WarehouseInventory.ViewModels;

namespace WarehouseInventory.Windows;

public partial class SuppliersWindow : Window
{
    public SuppliersWindow()
    {
        InitializeComponent();
        DataContext = new SupplierViewModel();
    }
}