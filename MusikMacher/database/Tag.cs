using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LorusMusikMacher.database
{
  public class Tag : INotifyPropertyChanged
  {
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    private string _name;
    public string Name
    {
      get { return _name; }
      set
      {
        if (value != _name)
        {
          _name = value;
          OnPropertyChanged(nameof(Name));
        }
      }
    }

    private bool _isChecked;
    public bool IsChecked
    {
      get { return _isChecked; }
      set
      {
        if (value != _isChecked)
        {
          _isChecked = value;
          OnPropertyChanged(nameof(IsChecked));
        }
      }
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}