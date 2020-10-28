using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextAnalyzerNetCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string text;
        public List<KeyValuePair<string, int>> myList;
        public OpenFileDialog openFileDialog = new OpenFileDialog();

        private IOrderedEnumerable<PropertyInfo> props;

        public MainWindow()
        {
            InitializeComponent();
            tbStatusOfTextFile.Text = "No text file selected";
        }

        /// <summary>
        /// Opens a text file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.Multiselect = false;
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                text = File.ReadAllText(openFileDialog.FileName, Encoding.Default);
                string safeFileName = openFileDialog.SafeFileName;
                tbStatusOfTextFile.Text = (safeFileName + " - loaded successfully");
            }
            else
            {
                MessageBox.Show("Error: Couldn't load file. Wrong file format, or no file selected?");
            }
        }

        /// <summary>
        /// Analyzes text and counts woreds aswell as sorts words after amount of occourence in text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            List<string> wordsLower = new List<string>();

            try
            {
                var words = SplitWords(text);

                // Converts the words to lower in order to get accurate results
                foreach (var word in words)
                {
                    wordsLower.Add(word.ToLower());
                }

                var counts = CountWordOccurrences(wordsLower);
                myList = counts.ToList();
                myList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
                MessageBox.Show("Conversion successfull");
            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't convert file", ex);
            }
        }

        /// <summary>
        /// Saves .csv
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btnSaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.Title = "Save text analysis";
            saveFileDialog.DefaultExt = ".csv";
            string filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";
            saveFileDialog.Filter = filter;
            saveFileDialog.ShowDialog();
            try
            {
                WriteCSV(myList, saveFileDialog.FileName);
                MessageBox.Show("File was saved");
            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't save file", ex);
            }
        }

        /// <summary>
        ///  CSV writer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="path"></param>
        public void WriteCSV<T>(IEnumerable<T> items, string path)
        {
            Type itemType = typeof(T);
            props = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .OrderBy(p => p.Name);

            using (var writer = new StreamWriter(path, true, Encoding.UTF8))
            {
                writer.WriteLine(string.Join(", ", props.Select(p => p.Name)));

                foreach (var item in items)
                {
                    writer.WriteLine(string.Join(", ", props.Select(p => p.GetValue(item, null))));
                }
                writer.Close();
            }
        }

        /// <summary>
        /// Splits the given text into individual words, stripping punctuation
        /// A word is defined by the regex @"\p{L}+"
        /// </summary>
        public IEnumerable<string> SplitWords(string text)
        {
            Regex wordMatcher = new Regex(@"\p{L}+");
            return wordMatcher.Matches(text).Select(c => c.Value);
        }

        /// <summary>
        /// Counts the number of occurrences of each word in the given enumerable
        /// </summary>
        public IDictionary<string, int> CountWordOccurrences(IEnumerable<string> words)
        {
            return CountOccurrences(words, StringComparer.CurrentCulture);
        }

        /// <summary>
        /// Prints word-counts to the given TextWriter
        /// </summary>
        public void WriteWordCounts(List<KeyValuePair<string, int>> myList, TextWriter writer)
        {
            Console.OutputEncoding = Encoding.UTF8;
            writer.WriteLine("The number of counts for each words are:");
            foreach (KeyValuePair<string, int> kvp in myList)
            {
                writer.WriteLine("Counts: " + kvp.Value + " " + kvp.Key.ToLower()); // print word in lower-case for consistency
            }
        }

        /// <summary>
        /// Counts the number of occurrences of each distinct item
        /// </summary>
        public IDictionary<T, int> CountOccurrences<T>(IEnumerable<T> items, IEqualityComparer<T> comparer)
        {
            var counts = new Dictionary<T, int>(comparer);

            foreach (T t in items)
            {
                int count;
                if (!counts.TryGetValue(t, out count))
                {
                    count = 0;
                }
                counts[t] = count + 1;
            }
            return counts;
        }
    }
}