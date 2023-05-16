using System;
using System.Collections.Generic;
using System.Text;

namespace Samshit.WebUtils
{
    public interface IEmailMessage
    {
        string Body { get; set; }
        string Subject { get; set; }
        string From { get; set; }
        string To { get; set; }
    }
}
