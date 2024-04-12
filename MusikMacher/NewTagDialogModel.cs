using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MusikMacher
{
  public class NewTagDialogViewModel : ViewModelBase
  {
    private string _tagName = "";
    public string TagName
    {
      get => _tagName;
      set
      {
        System.Diagnostics.Debug.WriteLine($"text field changed to '{value}'");
        _tagName = value;
        RaisePropertyChanged(nameof(TagName));
      }
    }

    private string _question = "";
    public string Question
    {
      get => _question;
      set
      {
        _question = value;
        RaisePropertyChanged(nameof(Question));
      }
    }

    public ICommand CreateCommand { get; }

    public NewTagDialogViewModel(string question)
    {
      this.Question = question;
      CreateCommand = new RelayCommand(Create);
    }

    private void Create()
    {
       createTag?.Invoke(TagName);
    }

    public Action<string>? createTag { get; set; } //callback that gets called by the dialog
  }
}
