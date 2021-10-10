using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TSM = Tekla.Structures.Model;
using TSD = Tekla.Structures.Drawing;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace Tolerance
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TSM.Model model;
        TSD.DrawingHandler drawingHandler;
        List<String> save = new List<string>();
        List<String> history = new List<string>();
        string xsdatadir = "";
        public MainWindow()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            //Загрузка файла с настройками и сохраненными даными.

            Tekla.Structures.TeklaStructuresSettings.GetAdvancedOption("XSDATADIR", ref xsdatadir);
            xsdatadir += "Environments\\common\\macros\\drawings\\WPFLimit\\save.xml";
            XDocument xdoc = new XDocument();
            if (File.Exists(xsdatadir))
            {
                xdoc = XDocument.Load(xsdatadir);
                w_main.Topmost = (bool)xdoc.Element("setting").Element("Поверх_окон");
                w_main.Left = (double)xdoc.Element("setting").Element("Лево");
                w_main.Top = (double)xdoc.Element("setting").Element("Верх");
                w_main.Height = (double)xdoc.Element("setting").Element("Высота");
                XmlSerializer formatter = new XmlSerializer(typeof(List<string>));
                List<string> f_temp = (List<string>)formatter.Deserialize(xdoc.Element("setting").Element("ArrayOfString").CreateReader());
                save = f_temp;
            }
            else
            {
                MessageBox.Show("Файл не грузится");
            }
            foreach (string item in save)
            {
                lb_save.Items.Add(SP_add(item, b_delete_Click, "Удалить", "Image.Delete"));
            }
            //Устанавливаю картинки для кнопки закрепления окна.
            if (w_main.Topmost)
            {
                b_Topmost.Content = Resources["Image.Second"];
                b_Topmost.ToolTip = "Убрать фиксацию поверх всех окон";
            }
            else
            {
                b_Topmost.Content = Resources["Image.First"];
                b_Topmost.ToolTip = "Зафиксировать поверх всех окон";
            }
        }
        //Проверка открыта модель или нет.
        private bool InitializeConnection()
        {
            TSM.Model _model = new TSM.Model();
            if(_model.GetConnectionStatus())
            {
                model = _model;
                return true;
            }
            else
            {
                return false;
            }
        }
        //Проверка открыт чертеж или нет.
        private bool InitializeDrawing()
        {
            TSD.DrawingHandler _drawingHandler = new TSD.DrawingHandler();
            if (_drawingHandler.GetActiveDrawing()!=null)
            {
                drawingHandler = _drawingHandler;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!InitializeConnection())
            {
                MessageBox.Show("Нет конекта к модели :(");
                this.Close();
            }
        }
        //Логика работы кнопки "Допуск".
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StraightDimension();
            //Запись данных в историю, с отсевом дублирующих.
            if (lb_history.Items.Count == 0)
            {
                history.Add(tb_limit_up.Text + "; " + tb_limit_down.Text);
                lb_history.Items.Add(SP_add(tb_limit_up.Text + "; " + tb_limit_down.Text, b_save_Click, "Сохранить", "Image.Save"));
            }
            else
            {
                if (history.IndexOf(tb_limit_up.Text + "; " + tb_limit_down.Text) == -1)
                {
                    history.Add(tb_limit_up.Text + "; " + tb_limit_down.Text);
                    lb_history.Items.Add(SP_add(tb_limit_up.Text + "; " + tb_limit_down.Text, b_save_Click, "Сохранить", "Image.Save"));
                }
            }
        }
        //Основной метод работы с размером.
        private void StraightDimension()
        {
            if (!InitializeDrawing())
            {
                MessageBox.Show("Чертеж не запущен :(");
                return;
            }
            TSD.Drawing drawing = drawingHandler.GetActiveDrawing();
            //Перебор выделенных объектов с поиском размеров.
            foreach (TSD.DrawingObject drawingObject in drawingHandler.GetDrawingObjectSelector().GetSelected())
            {
                if (drawingObject is TSD.StraightDimension)
                {
                    TSD.StraightDimension straightDimension = drawingObject as TSD.StraightDimension; //Найден одиночный размер, то что нам надо.
                    Prefix(straightDimension);
                }
                //Если найдена цепочка размеров, то ведем переборку по одиночным размерам из которых она состоит.
                if (drawingObject is TSD.StraightDimensionSet)
                {
                    TSD.StraightDimensionSet straightDimensionSet = drawingObject as TSD.StraightDimensionSet;
                    TSD.DrawingObjectEnumerator drawingObjectEnumerator = straightDimensionSet.GetObjects();
                    while (drawingObjectEnumerator.MoveNext())
                        if (drawingObjectEnumerator.Current is TSD.StraightDimension)
                        {
                            TSD.StraightDimension straightDimension = drawingObjectEnumerator.Current as TSD.StraightDimension; //Найден одиночный размер, то что нам надо.
                            Prefix(straightDimension);
                        }
                }
            }
            drawing.CommitChanges(); //Важно. Фиксируем изменения в чертеже.
        }
        //Метод передающий даные в префикс размера.
       private void Prefix(TSD.StraightDimension straightDimension)
        {
            TSD.ContainerElement containerElement = new TSD.ContainerElement();
            TSD.ContainerElement cE_return = new TSD.ContainerElement(); //Пустой контейнер. Нужен для уменьшения интервала от значения размера до допусков.
            TSD.NewLineElement newLineElement = new TSD.NewLineElement();
            TSD.FontAttributes fontAttributes = new TSD.FontAttributes
            {
                Name = "GOST type A",
                Height = 3.5,
                Color = TSD.DrawingColors.NewLine1
            };
            if (tb_limit_up.Text == "0")
            {
                tb_limit_up.Text = " 0";
            }
            if (tb_limit_down.Text == "0")
            {
                tb_limit_down.Text = " 0";
            }
            if (tb_limit_up.Text.IndexOf("+") != -1 && tb_limit_down.Text == "")
            {
                tb_limit_down.Text = " 0";
            }
            if (tb_limit_down.Text.IndexOf("-") != -1 && tb_limit_up.Text == "")
            {
                tb_limit_up.Text = " 0";
            }
            if (tb_limit_up.Text.IndexOf("+") != -1 && tb_limit_down.Text.IndexOf("-") != -1 && tb_limit_up.Text.Replace("+", "") == tb_limit_down.Text.Replace("-", ""))
            {
                TSD.TextElement textElement = new TSD.TextElement("±" + tb_limit_up.Text.Replace("+",""), fontAttributes);
                containerElement.Add(cE_return);
                containerElement.Add(cE_return);
                containerElement.Add(cE_return);
                containerElement.Add(textElement);
            }
            else
            {
                if (tb_limit_up.Text != "")
                {
                    TSD.TextElement textElement = new TSD.TextElement(tb_limit_up.Text + " ", fontAttributes);
                    containerElement.Add(cE_return);
                    containerElement.Add(cE_return);
                    containerElement.Add(cE_return);
                    containerElement.Add(textElement);
                    containerElement.Add(newLineElement);
                }
                if (tb_limit_down.Text != "")
                {
                    TSD.TextElement textElement2 = new TSD.TextElement(tb_limit_down.Text + " ", fontAttributes);
                    containerElement.Add(cE_return);
                    containerElement.Add(cE_return);
                    containerElement.Add(cE_return);
                    containerElement.Add(textElement2);
                }
            }
            straightDimension.Attributes.DimensionValuePostfix.Clear();
            straightDimension.Attributes.DimensionValuePostfix.Add(containerElement);
            straightDimension.Modify();
        }

        //Метод создающий строку для списком истории и сохранения. В зависимости от списка меняется метод кнопки находящейся в строке.
        private StackPanel SP_add(string text, RoutedEventHandler routedEventHandler, string tooltip, string icon)
        {
            
            Button b_save = new Button();
            Label l_save = new Label();
            b_save.Height = 15;
            b_save.Width = 15;
            b_save.Click += routedEventHandler;
            b_save.Tag = text;
            b_save.ToolTip = tooltip;
            b_save.Content = this.FindResource(icon);
            l_save.Content = text;
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Children.Add(b_save);
            stackPanel.Children.Add(l_save);
            return stackPanel;
        }
        //При переключении между ячейками будет выделяться весь текст находящийся в них.
        private void tb_limit_up_GotFocus(object sender, RoutedEventArgs e)
        {
            tb_limit_up.SelectAll();
        }

        private void tb_limit_down_GotFocus(object sender, RoutedEventArgs e)
        {
            tb_limit_down.SelectAll();
        }
        //Передача фокуса по кнопке Enter.
        private void tb_limit_up_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tb_limit_down.Focus();
            }
        }

        private void tb_limit_down_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                b_limit.Focus();
            }
        }

        private void b_limit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tb_limit_up.Focus();
            }
        }
        //При запуске приложения, а также при переключении на него, фокус будет передаваться первом полю ввода (Верх).
        private void Window_Activated(object sender, EventArgs e)
        {
            tb_limit_up.Focus();
        }
        //Окно можно перемещать схватившись за любое место окна.
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
        //Кнопка фиксирет окно по верх всех окон.
        private void b_Topmost_Click(object sender, RoutedEventArgs e)
        {
            if (!w_main.Topmost)
            {
                w_main.Topmost = true;
                b_Topmost.Content = Resources["Image.Second"];
                b_Topmost.ToolTip = "Убрать фиксацию поверх всех окон";
            }
            else
            {
                w_main.Topmost = false;
                b_Topmost.Content = Resources["Image.First"];
                b_Topmost.ToolTip = "Зафиксировать поверх всех окон";
            }
        }

        private void w_main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Запись настроек и данных в файл.
            XDocument xdoc = new XDocument();
            xdoc.Add(new XElement("setting",
                new XElement("Поверх_окон", w_main.Topmost),
                new XElement("Лево", w_main.Left),
                new XElement("Верх", w_main.Top),
                new XElement("Высота", w_main.Height)));
            XmlSerializer formatter = new XmlSerializer(typeof(List<string>));
            XDocument xdoc1 = new XDocument();
            using (XmlWriter fs = xdoc1.CreateWriter())
            {
                formatter.Serialize(fs, save);
            }
            xdoc.Root.Add(xdoc1.Root);
            xdoc.Save(xsdatadir);
        }

        private void lb_history_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lb_history.SelectedIndex != -1)//Отключение активации двойным кликом по кнопке сохранения.
            {
                string[] temp = history[lb_history.SelectedIndex].Split(';');
                tb_limit_up.Text = temp[0].Trim();
                tb_limit_down.Text = temp[1].Trim();
                StraightDimension();
            }
        }
        //Сохраняем строку из истории.
        private void b_save_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (save.IndexOf(button.Tag.ToString()) == -1)
            {
                lb_save.Items.Add(SP_add(button.Tag.ToString(), b_delete_Click, "Удалить", "Image.Delete"));
                save.Add(button.Tag.ToString());
            }

        }
        private void lb_save_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lb_save.SelectedIndex != -1) //Отключение активации двойным кликом по кнопке удаления.
            {
                string[] temp = save[lb_save.SelectedIndex].Split(';');
                tb_limit_up.Text = temp[0].Trim();
                tb_limit_down.Text = temp[1].Trim();
                StraightDimension();
            }
        }
        //Удаление из истории.
        private void b_delete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            lb_save.Items.Remove(button.Parent);
            save.Remove(button.Tag.ToString());
        }

        private void b_clear_Click(object sender, RoutedEventArgs e)
        {
            tb_limit_up.Text = "";
            tb_limit_down.Text = "";
            StraightDimension();
        }
    }
}
