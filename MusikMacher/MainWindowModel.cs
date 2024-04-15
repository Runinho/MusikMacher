using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LorusMusikMacher.database;
using Microsoft.EntityFrameworkCore;
using MusikMacher.components;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace MusikMacher
{
  class MainWindowModel : ViewModelBase
  {

    public static MainWindowModel Instance = new MainWindowModel();

    public MainWindowModel()
    {

      // load Settings
      Settings settings = Settings.getSettings();
      AndTags = settings.ANDTagCombination;
      SkipPosition = settings.SkipPosition;
    }

    private bool _andTags = false;
    public bool AndTags
    {
      get { return _andTags; }
      set
      {
        if (value != _andTags)
        {
          _andTags = value;
          RaisePropertyChanged(nameof(AndTags));
          // TODO: refresh traclist
          foreach (var viewModel in viewModels)
          {
            viewModel.RefreshTracksView();
          }
          //RefreshTracksView();

          // saving location in settings
          Settings.getSettings().ANDTagCombination = value;
          Settings.saveSettings();
        }
      }
    }

    private double _skipPosition;
    public BrowseViewModel[] viewModels = [];

    public double SkipPosition
    {
      get { return _skipPosition; }
      set
      {
        if (value != _skipPosition)
        {
          _skipPosition = value;
          RaisePropertyChanged(nameof(SkipPosition));


          // saving location in settings
          Settings.getSettings().SkipPosition = value;
          Settings.saveSettings();
        }
      }
    }
  }
}
