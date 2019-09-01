using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace SetDigitSubstitution
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            ICollectionView view = CollectionViewSource.GetDefaultView(cultures);
            view.SortDescriptions.Add(new SortDescription(nameof(CultureInfo.DisplayName), ListSortDirection.Ascending));
            _cultureBox.ItemsSource = view;

            string[] data = Properties.Resources.Data.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<DataEntry> entries = new List<DataEntry>(data.Length);
            foreach (string line in data)
                entries.Add(DataEntry.Parse(line));

            view = CollectionViewSource.GetDefaultView(entries);
            view.SortDescriptions.Add(new SortDescription(nameof(DataEntry.Section), ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription(nameof(DataEntry.Subsection), ListSortDirection.Ascending));
            _digitsBox.ItemsSource = view;

            ResetTo(CultureInfo.CurrentCulture);
        }

        private void ResetTo(CultureInfo info)
        {
            _cultureBox.SelectedValue = info.LCID;
            _digitsBox.SelectedValue = string.Concat(info.NumberFormat.NativeDigits);

            switch (info.NumberFormat.DigitSubstitution)
            {
                case DigitShapes.Context:
                    _substContext.IsChecked = true;
                    break;

                case DigitShapes.None:
                    _substNever.IsChecked = true;
                    break;

                case DigitShapes.NativeNational:
                    _substAlways.IsChecked = true;
                    break;
            }
        }

        private void OnApplyClicked(object sender, RoutedEventArgs e)
        {
            DigitShapes shapes = DigitShapes.None;
            if (_substContext.IsChecked == true) shapes = DigitShapes.Context;
            else if(_substAlways.IsChecked == true) shapes = DigitShapes.NativeNational;

            try
            {
                Cursor = Cursors.Wait;
                Program.Apply((int)_cultureBox.SelectedValue, shapes, (string)_digitsBox.SelectedValue);
                MessageBox.Show(this, "Succesfully applied.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show(this, "Selected configuration was rejected. Windows might not support these settings yet, sorry!" + Environment.NewLine + Environment.NewLine + "Returned error: " + ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ClearValue(CursorProperty);
            }
        }
    }
}
