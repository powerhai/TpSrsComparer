using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Xps.Packaging;
using Aspose.Words;
using Aspose.Words.Layout;
using Aspose.Words.Tables;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using TpSrsComparer.Domain;
using TpSrsComparer.Properties;
using Table = Aspose.Words.Tables.Table;
namespace TpSrsComparer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public bool IsBusy
        {
            get => mIsBusy;
            set => mIsBusy = SetProperty(ref mIsBusy, value);
        }

        public string SubTitle => "Document Comparison Tool v2.0 SRS vs TP";
        private string _title = "Document Comparison Tool";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {

        }

        private string mCurrentDir = "";
         

        private CollectionViewSource mCompareView;

        private FixedDocumentSequence mCurrentSrsDocument;

        private bool mIsBusy1;

        private readonly Regex mLeftRegex = new Regex("[\\{｛]\\s*([a-zA-Z]+[\\-_]*[a-zA-Z0-9]+)\\s*[\\}｝]");

        private readonly string mLeftTempFile = "left.xps";

        private int mProgressTotal = 100;

        private int mProgressValue;

        private string mPtFile;

        private readonly Regex mRightRegex = new Regex("\\s*([a-zA-Z]+[\\-_]*[a-zA-Z0-9]+)\\s*");

        private string mRightTempFile = "right.xps";

        private string mLeftMarkedFile = "LeftMarked.docx";

        private string mRightMarkedFile = "RightMarked.docx";

        private string mSrsFile;

        private FixedDocumentSequence mCurrentTpDocument;

        private string mFilterWord = "";
        private bool mIsBusy;
        private static IRegionManager RegionManager;

        private string CurrentUri => "file:///" + mCurrentDir + "\\";

        public FixedDocumentSequence CurrentSrsDocument
        {
            get
            {
                return mCurrentSrsDocument;
            }
            set
            {
                mCurrentSrsDocument = value;
               OnPropertyChanged();
            }
        }

        public int ProgressTotal
        {
            get
            {
                return mProgressTotal;
            }
            set
            {
                mProgressTotal = value;
                OnPropertyChanged();

            }
        }

        public string FilterWord
        {
            get
            {
                return mFilterWord;
            }
            set
            {
                mFilterWord = value; OnPropertyChanged();

                ResetFilter();
            }
        }

        public int ProgressValue
        {
            get
            {
                return mProgressValue;
            }
            set
            {
                mProgressValue = value; OnPropertyChanged();

            }
        }

        public ObservableCollection<CompareItem> CompareItems
        {
            get;
        } = new ObservableCollection<CompareItem>();


        public string PtFile
        {
            get
            {
                return mPtFile;
            }
            set
            {
                mPtFile = value; OnPropertyChanged();

            }
        }

        public string SrsFile
        {
            get
            {
                return mSrsFile;
            }
            set
            {
                mSrsFile = value; OnPropertyChanged();

            }
        }

        public CollectionViewSource CompareView
        {
            get
            {
                if (mCompareView == null)
                {
                    mCompareView = new CollectionViewSource
                    {
                        Source = CompareItems
                    };
                    mCompareView.View.CurrentChanged += View_CurrentChanged;
                    mCompareView.GroupDescriptions.Add(new PropertyGroupDescription("ComparedType"));
                    mCompareView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                }
                return mCompareView;
            }
        }

        private object mLockObject
        {
            get;
        } = new object();


        public ICommand PickPtFileCommand => (ICommand)new DelegateCommand((Action)delegate
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                PtFile = openFileDialog.FileName;
            }
        });

        public ICommand PickSrsFileCommand => (ICommand)new DelegateCommand((Action)delegate
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                SrsFile = openFileDialog.FileName;
            }
        });

        public ICommand CompareCommand => (ICommand)new DelegateCommand((Action)async delegate
        {
            SaveSettings();
            ProgressTotal = 100;
            ProgressValue = 0;
            if (string.IsNullOrEmpty(SrsFile))
            {
                MessageBox.Show("Please input SRS file");
            }
            else if (string.IsNullOrEmpty(PtFile))
            {
                MessageBox.Show("Please input TP file");
            }
            else
            {
                IsBusy = true;
               // ShowBusyBox();
                CompareItems.Clear();
                FilterWord = "";
                await Task.Run(delegate
                {
                    Compare();
                });
                CompareView.View.Refresh();
               // CloseBusyBox();
                IsBusy = false;
            }
        });

        public FixedDocumentSequence CurrentTpDocument
        {
            get
            {
                return mCurrentTpDocument;
            }
            set
            {
                mCurrentTpDocument = value;
                OnPropertyChanged();
            }
        }

        private void ResetFilter()
        {
            CompareView.View.Filter = ((object o) => ((o as CompareItem).Name.IndexOf(mFilterWord, StringComparison.CurrentCultureIgnoreCase) >= 0) ? true : false);
        }

        public MainWindowViewModel(IRegionManager regionManager )
        { 
           
            BindingOperations.EnableCollectionSynchronization(CompareItems, mLockObject);
            mCurrentDir = Environment.CurrentDirectory;
            LoadViewData();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public   void LoadViewData()
        {
            LoadPreviousSettings();
        }

 

  

        private void Compare()
        {
            try
            {
                File.Delete(mLeftMarkedFile);
                File.Delete(mRightMarkedFile);
                Document document = new Document(SrsFile);
                Document document2 = new Document(PtFile);
                int num2 = ProgressTotal = document.GetChildNodes(NodeType.Paragraph, isDeep: true).Count + document2.GetChildNodes(NodeType.Paragraph, isDeep: true).Count;
                ProgressValue = 0;
                PickSrsWords(document);
                PickTpWorks(document2);
                SaveMarkedLeftFile(document);
                SaveMarkedRightFile(document2);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PickTpWorks(Document doc)
        {
            LayoutCollector layoutCollector = new LayoutCollector(doc);
            NodeCollection childNodes = doc.GetChildNodes(NodeType.Paragraph, isDeep: true);
            Regex regex = mRightRegex;
            foreach (Aspose.Words.Paragraph item in childNodes)
            {
                string text = item.GetText();
                MatchCollection matchCollection = regex.Matches(text);
                int pageNumber = -1;
                if (matchCollection.Count > 0)
                {
                    pageNumber = layoutCollector.GetStartPageIndex(item);
                }
                if (item.IsListItem)
                {
                    item.ListFormat.ApplyBulletDefault();
                }
                CompositeNode compositeNode = item;
                compositeNode = (CompositeNode)((!item.IsInCell) ? ((object)item) : ((object)(item.ParentNode.ParentNode as Row)));
                foreach (Match item2 in matchCollection)
                {
                    string name = item2.Groups[1].Value;
                    CompareItem compareItem = CompareItems.FirstOrDefault((CompareItem a) => a.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                    compareItem?.RightLocations.Add(new CompareLocation
                    {
                        PageNumber = pageNumber,
                        Paragraph = compositeNode
                    });
                    compareItem?.Update();
                }
            }
        }

        private void PickSrsWords(Document doc)
        {
            LayoutCollector layoutCollector = new LayoutCollector(doc);
            NodeCollection childNodes = doc.GetChildNodes(NodeType.Paragraph, isDeep: true);
            Regex regex = mLeftRegex;
            foreach (Aspose.Words.Paragraph item in childNodes)
            {
                string text = item.GetText();
                MatchCollection matchCollection = regex.Matches(text);
                int pageNumber = -1;
                if (matchCollection.Count > 0)
                {
                    pageNumber = layoutCollector.GetStartPageIndex(item);
                }
                foreach (Match item2 in matchCollection)
                {
                    string name = item2.Groups[1].Value;
                    CompareItem compareItem = CompareItems.FirstOrDefault((CompareItem a) => a.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                    if (compareItem == null)
                    {
                        compareItem = new CompareItem
                        {
                            Name = name
                        };
                        CompareItems.Add(compareItem);
                    }
                    compareItem.LeftLocations.Add(new CompareLocation
                    {
                        PageNumber = pageNumber,
                        Paragraph = item
                    });
                }
                ProgressValue++;
            }
        }

        private void SaveMarkedLeftFile(Document doc)
        {
            DocumentBuilder documentBuilder = new DocumentBuilder(doc);
            foreach (CompareItem compareItem in CompareItems)
            {
                int num = 0;
                foreach (CompareLocation leftLocation in compareItem.LeftLocations)
                {
                    documentBuilder.MoveTo(leftLocation.Paragraph);
                    string bookmarkName = $"{compareItem.Name}_{num}";
                    documentBuilder.StartBookmark(bookmarkName);
                    documentBuilder.EndBookmark(bookmarkName);
                    num++;
                }
            }
            doc.Save(mLeftMarkedFile, SaveFormat.Docx);
        }

        private void SaveMarkedRightFile(Document doc)
        {
            DocumentBuilder documentBuilder = new DocumentBuilder(doc);
            foreach (CompareItem compareItem in CompareItems)
            {
                int num = 0;
                foreach (CompareLocation rightLocation in compareItem.RightLocations)
                {
                    if (rightLocation.Paragraph is Row)
                    {
                        Node child = rightLocation.Paragraph.GetChild(NodeType.Paragraph, 0, isDeep: true);
                        documentBuilder.MoveTo(child);
                    }
                    else
                    {
                        documentBuilder.MoveTo(rightLocation.Paragraph);
                    }
                    string bookmarkName = $"{compareItem.Name}_{num}";
                    documentBuilder.StartBookmark(bookmarkName);
                    documentBuilder.EndBookmark(bookmarkName);
                    num++;
                }
            }
            doc.Save(mRightMarkedFile, SaveFormat.Docx);
        }

        private void SaveMatchedToFile(CompareItem citem, bool isLeft)
        {
            Document document = new Document();
            DocumentBuilder documentBuilder = new DocumentBuilder(document);
            documentBuilder.PageSetup.LeftMargin = 5.0;
            documentBuilder.PageSetup.TopMargin = 5.0;
            documentBuilder.PageSetup.RightMargin = 5.0;
            documentBuilder.PageSetup.BottomMargin = 5.0;
            documentBuilder.MoveToDocumentStart();
            int num = 0;
            ObservableCollection<CompareLocation> obj = isLeft ? citem.LeftLocations : citem.RightLocations;
            string text = isLeft ? mLeftMarkedFile : mRightMarkedFile;
            string text2 = isLeft ? mLeftTempFile : mRightTempFile;
            string html = "<p style='padding: 10;  color:red'><b>The view is only for evaluation, please see the original document.</b></p>";
            documentBuilder.InsertHtml(html);
            foreach (CompareLocation item in obj)
            {
                NodeImporter nodeImporter = new NodeImporter(item.Paragraph.Document, document, ImportFormatMode.KeepSourceFormatting);
                string arg = $"{CurrentUri}{text}#{citem.Name}_{num}";
                string html2 = $"<p style='padding: 10; background-color:#C0C0C0;color:blue' ><b><a href='{arg}'>Page NO: {item.PageNumber}</a></b></p>";
                documentBuilder.InsertHtml(html2);
                if (item.Paragraph is Row)
                {
                    Row obj2 = item.Paragraph as Row;
                    documentBuilder.StartTable();
                    foreach (Cell cell2 in obj2.Cells)
                    {
                        Cell cell = documentBuilder.InsertCell();
                        cell.CellFormat.BottomPadding = 5.0;
                        cell.CellFormat.TopPadding = 5.0;
                        cell.CellFormat.LeftPadding = 5.0;
                        cell.CellFormat.RightPadding = 5.0;

                        foreach (var node in cell2.ChildNodes)
                        {
                            if(node is Aspose.Words.Paragraph)
                            {
                                Aspose.Words.Paragraph childNode = node as Aspose.Words.Paragraph;
                                childNode.ParagraphFormat.SpaceAfter = 0.0;
                                Node newChild = nodeImporter.ImportNode(childNode, isImportChildren: true);
                                cell.AppendChild(newChild);
                            }
                            else if (node is Table)
                            {
                                var table = node as Table;
                                Node newChild = nodeImporter.ImportNode(table, isImportChildren: true);
                                cell.AppendChild(newChild);
                            }

                        }
                    }
                    documentBuilder.EndTable();
                }
                else if (item.Paragraph is Aspose.Words.Paragraph)
                {
                    Aspose.Words.Paragraph paragraph2 = item.Paragraph as Aspose.Words.Paragraph;
                    if (paragraph2.IsListItem)
                    {
                        paragraph2.ListFormat.ApplyBulletDefault();
                    }
                    Node newChild2 = nodeImporter.ImportNode(paragraph2, isImportChildren: true);
                    document.FirstSection.Body.AppendChild(newChild2);
                }
                else
                {
                    Node newChild3 = nodeImporter.ImportNode(item.Paragraph, isImportChildren: true);
                    document.FirstSection.Body.AppendChild(newChild3);
                }
                documentBuilder.MoveToDocumentEnd();
                num++;
            }
            File.Delete(text2);
            document.Save(text2, SaveFormat.Xps);
            try
            {
                XpsDocument xpsDocument = new XpsDocument(text2, FileAccess.Read, CompressionOption.Maximum);
                FixedDocumentSequence fixedDocumentSequence = xpsDocument.GetFixedDocumentSequence();
                xpsDocument.Close();
                if (isLeft)
                {
                    CurrentSrsDocument = fixedDocumentSequence;
                }
                else
                {
                    CurrentTpDocument = fixedDocumentSequence;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void View_CurrentChanged(object sender, EventArgs e)
        {
            if (mCompareView.View.CurrentItem != null)
            {
                SaveMatchedToFile(mCompareView.View.CurrentItem as CompareItem, isLeft: true);
                SaveMatchedToFile(mCompareView.View.CurrentItem as CompareItem, isLeft: false);
            }
        }

        private void LoadPreviousSettings()
        {
            SrsFile = Settings.Default.SrsFile;
            PtFile = Settings.Default.TpFile;
        }

        private void SaveSettings()
        {
            Settings.Default.SrsFile = SrsFile;
            Settings.Default.TpFile = PtFile;
            Settings.Default.Save();
        }

    }
}
