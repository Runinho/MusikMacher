using System.Windows.Controls;

namespace MusikMacher.components;

public partial class PremiereLoader : UserControl
{
    public PremiereLoader()
    {
        InitializeComponent();
        DataContext = new PremiereDataLoaderViewModel();
    }
}