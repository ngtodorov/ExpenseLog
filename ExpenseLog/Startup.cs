using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ExpenseLog.Startup))]
namespace ExpenseLog
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
