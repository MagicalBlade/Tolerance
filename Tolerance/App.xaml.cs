using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TSM = Tekla.Structures.Model;

namespace Tolerance
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            InitializeComponent();
        }
        static Mutex mutex = new Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true) && InitializeConnection())
            {
                App app = new App();
                MainWindow window = new MainWindow();
                app.Run(window);
            }
            //Проверка открыта модель или нет.
            bool InitializeConnection()
            {
                TSM.Model _model = new TSM.Model();
                if (_model.GetConnectionStatus())
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("Нет конекта к модели :(");
                    return false;
                }
            }
        }


    }
}
