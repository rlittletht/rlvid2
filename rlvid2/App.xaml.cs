using RestoreWindowPlace;
using System.Configuration;
using System.Data;
using System.Windows;

namespace rlvid2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public WindowPlace WindowPlace { get; }

        public App()
        {
            // Set a name of config file
            this.WindowPlace = new WindowPlace("placement.config");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            this.WindowPlace.Save();
        }
    }

}
