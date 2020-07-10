using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OptionsDemo.Services
{
    public class OrderServiceValidateOptions : IValidateOptions<OrderOptions>
    {
        public ValidateOptionsResult Validate(string name, OrderOptions options)
        {
            if (options.MaxOrderCount > 100)
            {
                return ValidateOptionsResult.Fail("不能大于100");
            }
            else
            {
                return ValidateOptionsResult.Success;
            }
        }
    }
}
