using System.Windows;
using WarehouseInventory.ViewModels;

namespace WarehouseInventory.Windows;

public partial class InvoicesWindow : Window
{
    public InvoicesWindow()
    {
        InitializeComponent();
        DataContext = new InvoiceViewModel();
    }
}