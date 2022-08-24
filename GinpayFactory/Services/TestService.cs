using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinpayFactory.Services
{

    public interface ITestService
    {
        public string Test();
    }

    public class TestService : ITestService
    {
        //private readonly ILogger<TestService> _logger;

        //public TestService(ILogger<TestService> logger)
        //{
        //    _logger = logger;
        //}

        public string Test()
        {
            return "aaaa";
        }
    }

}
