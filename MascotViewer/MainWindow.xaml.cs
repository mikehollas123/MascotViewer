
using Microsoft.Win32;

using SciChart.Charting.Model.DataSeries;

using SciChart.Charting.Visuals.Annotations;
using SciChart.Charting.Visuals.PointMarkers;
using SciChart.Charting.Visuals.RenderableSeries;
using SciChart.Core;

using System;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using System.Linq;

using System.Threading.Tasks;

using System.Windows;

using System.Windows.Media;


namespace MascotViewer
{


    public class MyDataContext
    {
        

        private Dictionary<string, string> _mascotfiles = new Dictionary<string, string>();

        public Dictionary<string, string>  Mascotfiles
        {
            get { return _mascotfiles ; }
            set { _mascotfiles =  value; }
        }


        private ObservableCollection<string> _sampleList = new ObservableCollection<string>();

        public ObservableCollection<string> SampleList
        {
            get { return _sampleList; }
            set { _sampleList = value; }
        }
        private ObservableCollection<string> _controlList = new ObservableCollection<string>();
        public ObservableCollection<string> ControlList
        {
            get { return _controlList; }
            set { _controlList = value; }
        }


        private Dictionary<string, Dictionary<string, IProtein>> _incProtDict = new Dictionary<string, Dictionary<string, IProtein>>();

        public Dictionary<string, Dictionary<string, IProtein>> IncProtDict
        {
            get { return _incProtDict; }
            set { _incProtDict = value; }
        }

        private Dictionary<string, Dictionary<string, IProtein>> _exProtDict = new Dictionary<string, Dictionary<string, IProtein>>();

        public Dictionary<string, Dictionary<string, IProtein>> ExProtDict
        {
            get { return _exProtDict; }
            set { _exProtDict = value; }
        }



        private ObservableCollection<IProtein> _ExProtList = new ObservableCollection<IProtein>();

        public ObservableCollection<IProtein> ExProtList
        {
            get { return _ExProtList; }
            set { _ExProtList = value; }
        }

        private ObservableCollection<IProtein> _incProtList = new ObservableCollection<IProtein>();

        public ObservableCollection<IProtein> IncProtList
        {
            get { return _incProtList; }
            set { _incProtList = value; }
        }


    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        MyDataContext theDataContext = new MyDataContext();

        public Dictionary<string, double> _fileTotalPetideCount = new Dictionary<string, double>();

  
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = theDataContext;
            
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var datFile = new OpenFileDialog();
            datFile.Multiselect = true;
            datFile.Filter = "Mascot files (*.dat)|*.dat|All files (*.*)|*.*"; ;
            if (datFile.ShowDialog() == true)
            {
                var files = datFile.FileNames;

                //List<string> toAdd = new List<>
                ProgressBar.Visibility = Visibility.Visible;
                int multiplier = 1;
                var progress = new Progress<double>(percent => ProgressBar.ProgressValue = (percent * (100/files.Length)) *multiplier);

               

                foreach (var file in files)
                {
                    using (MascotReader mFile = new MascotReader(file))
                    {
                        if (!theDataContext.Mascotfiles.ContainsKey(mFile.FileName))
                        {
                        theDataContext.Mascotfiles.Add(mFile.FileName, file);
                        theDataContext.SampleList.Add(mFile.FileName);
                            await AddNewProteins(mFile.FileName,progress);
                        }
                    }
                    multiplier++;
                } 
            }
            ProgressBar.Visibility = Visibility.Hidden;
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> tempList = new List<string>();
            foreach (string sample in controlListBox.SelectedItems)
            {

                tempList.Add(sample);
                theDataContext.IncProtDict[sample] = new Dictionary<string, IProtein>();
                theDataContext.IncProtDict[sample] = theDataContext.ExProtDict[sample];
                theDataContext.ExProtDict.Remove(sample);

            }

            foreach (var item in tempList)
            {
                theDataContext.ControlList.Remove(item);
                theDataContext.SampleList.Add(item);
            }
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {

            List<string> tempList = new List<string>();
            foreach (string sample in sampleListBox.SelectedItems)
            {

                tempList.Add(sample);
                theDataContext.ExProtDict[sample] = new Dictionary<string, IProtein>();
                theDataContext.ExProtDict[sample] = theDataContext.IncProtDict[sample];
                theDataContext.IncProtDict.Remove(sample);
            }

            foreach (var item in tempList)
            {
                theDataContext.SampleList.Remove(item);
                theDataContext.ControlList.Add(item);
            }
         

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (ControlCheckBox.IsChecked == true)
            {
                controlListBox.IsEnabled = true;
                labelControl.IsEnabled = true;
                buttonDown.IsEnabled = true;
                buttonUp.IsEnabled = true;
            }
            else
            {
                controlListBox.IsEnabled = false;
                labelControl.IsEnabled = false;
                buttonDown.IsEnabled = false;
                buttonUp.IsEnabled = false;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        { UpdateObservableLists();
            if (ControlCheckBox.IsChecked == false)
            {
                //Run direct counts, not fold change

                int AN;
                if (!Int32.TryParse(AnnotaionN.Text, out AN))
                    AN = 0;
                sciChartSurface.Annotations.Clear();

                var RankOrderdArray = theDataContext.IncProtList.OrderByDescending(x => x.PeptideCount).ToArray();

                var scatterData = new XyScatterRenderableSeries()
                {
                    Stroke = Colors.OrangeRed,
                    PointMarker = new EllipsePointMarker()
                    {
                        Stroke = Colors.OrangeRed,
                        StrokeThickness = 5,
                        Fill = Colors.OrangeRed
                    },
                    
                };
                sciChartSurface.RenderableSeries.Clear();
                sciChartSurface.RenderableSeries.Add(scatterData);


                var dataSeries = new XyDataSeries<int, double>();
               
                int topN;
                if (topNCheck.IsChecked == false)
                {
                    topN = RankOrderdArray.Length;
                }
                else
                {

                    try
                    {
                        topN = Convert.ToInt32(topNInput.Text);
                        if (topN >= RankOrderdArray.Length)
                        {
                            topN = RankOrderdArray.Length;
                        }
                    }
                    catch
                    {
                        topN = RankOrderdArray.Length;
                    }


                }

                for (int i = 0; i < topN; i++)
                {
                   
dataSeries.Append(i+1, RankOrderdArray[i].PeptideCount, new MikesMetaData(i+1, RankOrderdArray[i].PeptideCount, RankOrderdArray[i].Accession, RankOrderdArray[i].Description.Split(' ')[0]));

                    if (AnnotationCheck.IsChecked == true)
                    {
                        if (i < AN)
                        {

                            sciChartSurface.Annotations.Add(new TextAnnotation() { IsEditable = true, X1 = i + 1, Y1 = RankOrderdArray[i].PeptideCount, Text = RankOrderdArray[i].Description.Split(' ')[0] });
                        }
                    }
                    
                    }
                   


                
                scatterData.DataSeries = dataSeries;
                
                sciChartSurface.YAxes.Default.AxisTitle = "Counts";
        
            }
            else
            {
                int AN;
                if (!Int32.TryParse(AnnotaionN.Text, out AN))
                    AN = 0;

                sciChartSurface.Annotations.Clear();


                //Fold Change
                HashSet<string> ProteinAccs = new HashSet<string>();
                Dictionary<string, double> controlDict = new Dictionary<string, double>();
                Dictionary<string, Tuple<double,string>> FinalDict = new Dictionary<string, Tuple<double, string>>();

             

                foreach (var file in theDataContext.ExProtDict)
                {
                    foreach (var prot in file.Value)
                    {
                        if (controlDict.ContainsKey(prot.Key))
                        {
                            controlDict[prot.Key] += prot.Value.PeptideCount / theDataContext.ExProtDict.Count;
                        }
                        else
                        {
                            controlDict[prot.Key] = prot.Value.PeptideCount / theDataContext.ExProtDict.Count;
                        }
                    }
                }


   foreach (var protein in theDataContext.IncProtList)
                {
                    double controlValue;
                    if (controlDict.ContainsKey(protein.Accession) )

                        controlValue = controlDict[protein.Accession];
                    else
                    {
                        controlValue = 1.0/theDataContext.ControlList.Count();
                    }
                    var forwardValue = protein.PeptideCount ;
                    FinalDict.Add(protein.Accession, new Tuple<double, string>( Math.Log(forwardValue, 2) - Math.Log(controlValue, 2),protein.Description.Split(' ')[0]));
                }
                var RankOrderdArray = FinalDict.OrderByDescending(x => x.Value).ToArray();

                             var scatterData = new XyScatterRenderableSeries()
                {
                    Stroke = Colors.OrangeRed,
                    PointMarker = new EllipsePointMarker()
                    {
                        Stroke = Colors.OrangeRed,
                        StrokeThickness = 5,
                        Fill = Colors.OrangeRed
                    }
                    
                };
                
                
                sciChartSurface.RenderableSeries.Clear();
                sciChartSurface.RenderableSeries.Add(scatterData);
              
                var dataSeries = new XyDataSeries<int, double>();
                int topN;
                if (topNCheck.IsChecked == false)
                {
                    topN = RankOrderdArray.Length;
                }
                else
                {

                    try
                    {
                        topN = Convert.ToInt32(topNInput.Text);
                        if (topN >= RankOrderdArray.Length)
                        {
                            topN = RankOrderdArray.Length;
                        }
                    }
                    catch
                    {
                        topN = RankOrderdArray.Length;
                    }


                }

                for (int i = 0; i < topN; i++)
                {
                        dataSeries.Append(i+1, RankOrderdArray[i].Value.Item1,new MikesMetaData(i+1, RankOrderdArray[i].Value.Item1, RankOrderdArray[i].Key, RankOrderdArray[i].Value.Item2));

                    if (AnnotationCheck.IsChecked == true)
                    {
 if (i < AN)
                    {   

                        sciChartSurface.Annotations.Add(new TextAnnotation() { IsEditable = true, X1 = i + 1, Y1 = RankOrderdArray[i].Value.Item1, Text = RankOrderdArray[i].Value.Item2 });
                    }
                    }
                  
                }
                scatterData.DataSeries = dataSeries;
               


                sciChartSurface.YAxes.Default.AxisTitle = "Fold Change (Log2)";
      



            }
        }

        private void topNCheck_Click(object sender, RoutedEventArgs e)
        {
            if (topNCheck.IsChecked == true)
            {
                topNInput.IsEnabled = true;
            }
            else
            {
                topNInput.IsEnabled = false;
            }

        }




        public async Task AddNewProteins(string File, Progress<double> progress = null)
        { 
            await Task.Run(() => { using (MascotReader mreader = new MascotReader(theDataContext.Mascotfiles[File]))
            {

               

                theDataContext.IncProtDict[File] = new Dictionary<string, IProtein>();
                var proteins =   mreader.GetProteins(progress);
                foreach (var prot in proteins)
                {
                    if (theDataContext.IncProtDict[File].ContainsKey(prot.Accession))
                    {
                        theDataContext.IncProtDict[File][prot.Accession].PeptideCount += prot.PeptideCount;
                    }
                    else
                    {
                        theDataContext.IncProtDict[File][prot.Accession] = prot;
                    }
                }
            } });

           
           
            
        }


        public void UpdateObservableLists()
        {
            Dictionary<string, IProtein> tempIncDict = new Dictionary<string, IProtein>();
            foreach (var file in theDataContext.IncProtDict)
            {
                foreach (var prot in file.Value)
                {
                    if (tempIncDict.ContainsKey(prot.Value.Accession))
                    {
                        tempIncDict[prot.Value.Accession].PeptideCount += prot.Value.PeptideCount / theDataContext.IncProtDict.Count;
                    }
                    else
                    {
                        tempIncDict[prot.Key] = new SmallProtein() { Accession = prot.Value.Accession, Mass = prot.Value.Mass, Description = prot.Value.Description, PeptideCount = prot.Value.PeptideCount };
                        tempIncDict[prot.Key].PeptideCount /= theDataContext.IncProtDict.Count;
                    }
                }
            }

            List<string> ExProtAccs = new List<string>();

            foreach (var prot in theDataContext.ExProtList)
            {
                ExProtAccs.Add(prot.Accession);
            }


            theDataContext.IncProtList.Clear();
            foreach (var prot in tempIncDict)
            {
               if (!ExProtAccs.Contains(prot.Key))
                {
 theDataContext.IncProtList.Add(prot.Value);
                }
               
            }
            


        }




 



        private void ProteinFilter_Click(object sender, RoutedEventArgs e)
        {
            UpdateObservableLists();
            var p = new ProteinFilter();
            p.DataContext = theDataContext;

            p.Show();


        }

        //private void normCheck_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (normCheck.IsChecked == true)
        //    {
        //       foreach (var file in theDataContext.IncProtDict)
        //        {
        //            double totalCounts = 0;
        //            foreach(var prot in file.Value)
        //            {
        //                totalCounts += prot.Value.PeptideCount;
        //            }
        //            _fileTotalPetideCount[file.Key] = totalCounts;

        //            foreach (var prot in file.Value)
        //            {
        //                prot.Value.PeptideCount /= totalCounts;
        //            }


        //        }


        //        foreach (var file in theDataContext.ExProtDict)
        //        {
        //            double totalCounts = 0;
        //            foreach (var prot in file.Value)
        //            {
        //                totalCounts += prot.Value.PeptideCount;
        //            }
        //            _fileTotalPetideCount[file.Key] = totalCounts;
        //            foreach (var prot in file.Value)
        //            {
        //                prot.Value.PeptideCount /= totalCounts;
        //            }


        //        }
        //    }
        //    else if (normCheck.IsChecked == false)
        //    {
        //        foreach (var file in theDataContext.IncProtDict)
        //        {
                    
        //            foreach (var prot in file.Value)
        //            {
        //                prot.Value.PeptideCount *= _fileTotalPetideCount[file.Key];
        //            }
             


        //        }


        //        foreach (var file in theDataContext.ExProtDict)
        //        {
             
        //            foreach (var prot in file.Value)
        //            {
        //                prot.Value.PeptideCount *= _fileTotalPetideCount[file.Key];
        //            }

        //        }
        //    }
        //    UpdateObservableLists();

        //}

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var saveFile = new SaveFileDialog();
            saveFile.DefaultExt = ".png";
            saveFile.Title = "Export";
            saveFile.FileName = "MascotExportedFile.png";
            saveFile.Filter = "PNG (*.png)|*.png|JPEG(*.jpeg)|*.jpeg|XPS(*.xps)|*.xps"; ;
            saveFile.ShowDialog();
            var file = saveFile.FileName;
            var fileType = System.IO.Path.GetExtension(file);
            if (file == "MascotExportedFile.png")
                return;
            switch (fileType)
            {
                case ".png":
                        {
                        sciChartSurface.ExportToFile(file, ExportType.Png, false);
                        break; }
                case ".jpeg":
                    {
                        sciChartSurface.ExportToFile(file, ExportType.Jpeg, false);
                        break;
                    }
                case ".xps":
                    {
                        sciChartSurface.ExportToFile(file, ExportType.Xps, false);
                        break;
                    }

            }
            //
        }
    }
    public class MikesMetaData : IPointMetadata
    {
        public MikesMetaData(int rank,double y)
        {
            Rank = rank;
            Y = y;
        }
        public MikesMetaData(int rank, double y,string accession)
        {
            Rank = rank;
            Y = y;
            Accession = accession;
        }
        public MikesMetaData(int rank, double y, string accession,string name)
        {
            Rank = rank;
            Y = y;
            Accession = accession;
            SimpleName = name;
        }
        public bool IsSelected { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public double Y { get; set; }
        public string SimpleName { get; set; }

        public int Rank { get; set; }
        public string Accession { get; set; }


    }
}
