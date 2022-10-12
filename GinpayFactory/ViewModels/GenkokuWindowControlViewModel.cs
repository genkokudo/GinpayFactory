using Community.VisualStudio.Toolkit;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GinpayFactory.Services;
using Microsoft.ServiceHub.Resources;
using Microsoft.VisualStudio.Package;
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
        private ISourceService _source;

        //[ObservableProperty]
        //private string name;
        
        [ObservableProperty]
        List<ServiceInput> services;

        public GenkokuWindowControlViewModel(ISourceService source){
            //this.name = "Genkoku";
            _source = source;
            Services = new();
        }

        /// <summary>
        /// サービス一覧をリフレッシュする処理
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [RelayCommand]
        private async Task GreetAsync(string user)
        {
            var serviceNames = await _source.SeekAndGetServiceNameListAsync();
            Services = serviceNames.Select(x => new ServiceInput { ServiceName = x }).ToList();
        }

        /// <summary>
        /// 選択したサービスを決定するボタンの処理
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task SubmitAsync()
        {
            // 現在表示中のクラスに選択したServiceをインジェクションする。
            var serviceNames = services.Where(x => x.IsChecked).Select(x => x.ServiceName).ToList();
            // TODO:ボタン押した時点のカーソルの位置を含むSpanのクラスからソースを取る
            await _source.AddAndReplaceInjectionAsync(serviceNames);

        }
    }

    /// <summary>
    /// サービスを選択するためのリストの1データ
    /// </summary>
    public class ServiceInput
    {
        public bool IsChecked { get; set; } = false;
        public string ServiceName { get; set; }
    }
}
