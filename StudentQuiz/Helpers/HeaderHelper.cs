using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentQuiz.Helpers;

public static class HeaderHelper
{
    public static void OverrideHeader(string header)
    {
        var window = (App.Current as App)?.Window as MainWindow;
        window.OverrideHeader(header);
    }
}
