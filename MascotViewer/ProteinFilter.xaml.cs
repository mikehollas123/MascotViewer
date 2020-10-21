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
 var index = incProtDataGrid.SelectedIndex;
            viewModel.ExProtList.Add(viewModel.IncProtList[index]);
               

                viewModel.IncProtList.RemoveAt(index);
            }
           

        }



        private void buttonLeft_Click(object sender, RoutedEventArgs e)
        {
            MyDataContext viewModel = this.DataContext as MyDataContext;

            if (exProtDataGrid.SelectedIndex != -1)
            {
                var index = exProtDataGrid.SelectedIndex;
                viewModel.IncProtList.Add(viewModel.ExProtList[index]);

                viewModel.ExProtList.RemoveAt(index);
              
            }
        }
    }
}
