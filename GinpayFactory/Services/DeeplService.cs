using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinpayFactory.Services
{
    // IOptionsは不要。作ったらGinpayFactoryPackageに登録。
    public interface IDeeplService
    {
        public string Test();
    }

    public class DeeplService : IDeeplService
    {
        public string Test()
        {
            return "aaaa";
        }
    }
}
