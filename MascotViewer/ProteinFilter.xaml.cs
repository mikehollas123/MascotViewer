using SciChart.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MascotViewer
{ 

    /// <summary>
    /// Interaction logic for ProteinFilter.xaml
    /// </summary>
    public partial class ProteinFilter : Window
    {
        

        

       
        public ProteinFilter()
        {
            InitializeComponent();
          
        }
 


        private void buttonright_Click(object sender, RoutedEventArgs e)
        {

            MyDataContext viewModel = this.DataContext as MyDataContext;

            if (incProtDataGrid.SelectedIndex != -1)
            {
                viewModel.ExProtList.Add((SmallProtein)incProtDataGrid.SelectedItem);

                viewModel.IncProtList.Remove((SmallProtein)incProtDataGrid.SelectedItem);
            }
           

        }



        private void buttonLeft_Click(object sender, RoutedEventArgs e)
        {
            MyDataContext viewModel = this.DataContext as MyDataContext;

            if (exProtDataGrid.SelectedIndex != -1)
            {
                viewModel.IncProtList.Add((SmallProtein)exProtDataGrid.SelectedItem);

                viewModel.ExProtList.Remove((SmallProtein)exProtDataGrid.SelectedItem);

            }
        }
    }
}
