﻿using LorusMusikMacher.database;
using MusikMacher.components;
using System.Collections.Frozen;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace MusikMacher
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : FluentWindow
  {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    MainWindowModel model = MainWindowModel.Instance;

    public MainWindow()
    {
      Settings settings = Settings.getSettings();

      InitializeComponent();
      DataContext = model;

      var browseTracksViewModel = new BrowseViewModel("tracks", settings.TrackBrowseSettings, false);
      var effectsTracksViewModel = new BrowseViewModel("effects", settings.EffectBrowseSettings, true);
      var importDataModel = new ImportViewModel(model, browseTracksViewModel, effectsTracksViewModel);

      browseTracksViewModel.ScrollTrackIntoView = tracksBrowse.ScrollTrackIntoView;
      effectsTracksViewModel.ScrollTrackIntoView = effectsBrowse.ScrollTrackIntoView;

      import.DataContext = importDataModel;
      tracksBrowse.model = browseTracksViewModel;
      tracksBrowse.DataContext = browseTracksViewModel;
      
      effectsBrowse.model = effectsTracksViewModel;
      effectsBrowse.DataContext = effectsTracksViewModel;

      model.viewModels = [browseTracksViewModel, effectsTracksViewModel]; // needed to trigger update if AND/OR config changed

      this.Closed += Window_Closed;


      Left = settings.MainWindowLeft;
      Top = settings.MainWindowTop;
      Width = settings.MainWindowWidth;
      Height = settings.MainWindowHeight;
    }
    private void Window_OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == Key.Space)
      {
        if (tabControl.SelectedItem != null)
        {
          // Access the content of the selected tab
          object selectedTabContent = ((TabItem)tabControl.SelectedItem).Content;

          // Now you can work with the content of the selected tab
          // For example, you can cast it to the specific type if you know it
          // For demonstration purposes, let's say the content is a TextBox
          if (selectedTabContent is Browse browse)
          {
            browse.model.Player.PlayPause();
          }
        }
      }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      // Save the loaction of the window
      Settings settings = Settings.getSettings();

      settings.MainWindowLeft = Left;
      settings.MainWindowTop = Top;
      settings.MainWindowWidth = Width;
      settings.MainWindowHeight = Height;
      settings.WindowTitle = model.WindowTitle;
      //settings.TracksSortingDescriptions = dataGrid.GetSortDescriptions();

      Settings.saveSettings(); // maybee this is enough and we can remove all the other saves?

    }

    private void mnuCultureChanged(object sender, EventArgs e)
    {
      if(sender is System.Windows.Controls.ComboBox cb)
      {
        if(cb.SelectedValue is string s)
        {
          if(System.Threading.Thread.CurrentThread.CurrentUICulture.Name != s)
          {
            Settings.getSettings().Language = s;
            Settings.saveSettings();

            // TODO: dialog to ask for restart
            //ContentDialog dialog = new ContentDialog();
            //
            //// XamlRoot must be set in the case of a ContentDialog running in a Desktop app
            //dialog.XamlRoot = this.XamlRoot;
            //dialog.Style = System.Windows.Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            //dialog.Title = "Changeing the Language requires a restart";
            //dialog.PrimaryButtonText = "Restart";
            //dialog.CloseButtonText = "Cancel";
            //dialog.DefaultButton = ContentDialogButton.Primary;
            //dialog.Content = new ContentDialogContent();
            //
            //var result = await dialog.ShowAsync();


            System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
          }
        }
      }
    }
  }
}