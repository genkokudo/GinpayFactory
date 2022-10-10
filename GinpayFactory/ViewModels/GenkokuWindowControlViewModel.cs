using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinpayFactory.ViewModels
{
    [INotifyPropertyChanged]
    public partial class GenkokuWindowControlViewModel
    {
        [ObservableProperty]
        private string name;

        public GenkokuWindowControlViewModel(){
            this.name = "Genkoku";
        }

        [RelayCommand]
        private void Greet(string user)
        {
            Name = $"Hello {user}!";
            Debug.WriteLine($"Hello {user}!");
        }
    }
}
