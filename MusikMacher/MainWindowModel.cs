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
      SkipPositionMovement = settings.SkipPositionMovement;
      WindowTitle = settings.WindowTitle;
      PlayEffectsFromBeginning = settings.PlayEffectsFromBeginning;
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
    
    public BrowseViewModel[] viewModels = [];

    private double _skipPosition;
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

    private double _skipPositionMovement; // amount to move on arrow key press
    public double SkipPositionMovement
    {
      get => _skipPositionMovement;
      set
      {
        if (value != _skipPositionMovement)
        {
          _skipPositionMovement = value;
          RaisePropertyChanged(nameof(SkipPositionMovement));
          
          // saving location in settings
          Settings.getSettings().SkipPositionMovement = value;
          Settings.saveSettings();
        }
      }
    }

    private bool _playEffectsFromBeginning;

    public bool PlayEffectsFromBeginning
    {
      get => _playEffectsFromBeginning;
      set
      {
        if (value != _playEffectsFromBeginning)
        {
          _playEffectsFromBeginning = value;
          RaisePropertyChanged(nameof(PlayEffectsFromBeginning));
          
          // saving location in settings
          Settings.getSettings().PlayEffectsFromBeginning = value;
          Settings.saveSettings();
        }
      }
    }



    private string _windowTitle;
    public string WindowTitle
    {
      get => _windowTitle;
      set
      {
        if (value != _windowTitle)
        {
          _windowTitle = value;
          RaisePropertyChanged(nameof(WindowTitle));
        }
      }
    }
  }
}
