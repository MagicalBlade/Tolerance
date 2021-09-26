﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TSM = Tekla.Structures.Model;
using TSD = Tekla.Structures.Drawing;
using System.Collections;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using System.Xml.Serialization;
using System.Xml;

namespace WPFLimit
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TSM.Model model;
        TSD.DrawingHandler drawingHandler;
        List<String> save = new List<string>();
        public MainWindow()
        {
            List<string> save = new List<string>();
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            XDocument xdoc = XDocument.Load("save.xml");
            w_main.Topmost = (bool)xdoc.Element("setting").Element("Поверх_окон");
            w_main.Left = (double)xdoc.Element("setting").Element("Лево");
            w_main.Top = (double)xdoc.Element("setting").Element("Верх");

            XmlSerializer formatter = new XmlSerializer(typeof(List<string>));
            List<string> f_temp = (List<string>)formatter.Deserialize(xdoc.Element("setting").Element("ArrayOfString").CreateReader());
            save = f_temp;

            //MessageBox.Show(save.Remove("2; ").ToString());


            foreach (var item in save)
            {
                lb_save.Items.Add(StackPanel(item, b_delete_Click));
            }

            if (w_main.Topmost)
            {
                b_Topmost.Content = Resources["Image.Second"];
            }
            else
            {
                b_Topmost.Content = Resources["Image.First"];
            }
        }
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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!InitializeDrawing())
            {
                MessageBox.Show("Чертеж не запущен :(");
                return;
            }
            StraightDimension();

            if (lb_history.Items.Count == 0)
            {
                lb_history.Items.Add(StackPanel(tb_limit_up.Text + "; " + tb_limit_down.Text, b_save_Click));
            }
            else
            {
                if (lb_history.Items[lb_history.Items.Count - 1].ToString() != (tb_limit_up.Text + "; " + tb_limit_down.Text))
                {
                    lb_history.Items.Add(StackPanel(tb_limit_up.Text + "; " + tb_limit_down.Text, b_save_Click));
                }
            }
        }

        private void StraightDimension()
        {
            TSD.Drawing drawing = drawingHandler.GetActiveDrawing();
            foreach (TSD.DrawingObject drawingObject in drawingHandler.GetDrawingObjectSelector().GetSelected())
            {
                if (drawingObject is TSD.StraightDimension)
                {
                    TSD.StraightDimension straightDimension = drawingObject as TSD.StraightDimension;
                    Prefix(straightDimension);
                }

                if (drawingObject is TSD.StraightDimensionSet)
                {
                    TSD.StraightDimensionSet straightDimensionSet = drawingObject as TSD.StraightDimensionSet;
                    TSD.DrawingObjectEnumerator drawingObjectEnumerator = straightDimensionSet.GetObjects();
                    while (drawingObjectEnumerator.MoveNext())
                        if (drawingObjectEnumerator.Current is TSD.StraightDimension)
                        {
                            TSD.StraightDimension straightDimension = drawingObjectEnumerator.Current as TSD.StraightDimension;
                            Prefix(straightDimension);
                        }
                }
            }
            drawing.CommitChanges();
        }
       private void Prefix(TSD.StraightDimension straightDimension)
        {
            TSD.ContainerElement containerElement = new TSD.ContainerElement();
            TSD.ContainerElement cE_return = new TSD.ContainerElement();
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

        private StackPanel StackPanel(string text, RoutedEventHandler routedEventHandler)
        {
            Button b_save = new Button();
            Label l_save = new Label();
            l_save.Content = text;
            b_save.Height = 15;
            b_save.Width = 15;
            b_save.Click += routedEventHandler;
            b_save.Tag = text;
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Children.Add(b_save);
            stackPanel.Children.Add(l_save);
            return stackPanel;
        }

        private void tb_limit_up_GotFocus(object sender, RoutedEventArgs e)
        {
            tb_limit_up.SelectAll();
        }

        private void tb_limit_down_GotFocus(object sender, RoutedEventArgs e)
        {
            tb_limit_down.SelectAll();
        }

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

        private void Window_Activated(object sender, EventArgs e)
        {
            tb_limit_up.Focus();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void b_Topmost_Click(object sender, RoutedEventArgs e)
        {
            if (!w_main.Topmost)
            {
                w_main.Topmost = true;
                b_Topmost.Content = Resources["Image.Second"];
            }
            else
            {
                w_main.Topmost = false;
                b_Topmost.Content = Resources["Image.First"];
            }
        }

        private void w_main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            XDocument xdoc = new XDocument();
            xdoc.Add(new XElement("setting",
                new XElement("Поверх_окон", w_main.Topmost),
                new XElement("Лево", w_main.Left),
                new XElement("Верх", w_main.Top)));
            XmlSerializer formatter = new XmlSerializer(typeof(List<string>));
            XDocument xdoc1 = new XDocument();
            using (XmlWriter fs = xdoc1.CreateWriter())
            {
                formatter.Serialize(fs, save);
            }
            xdoc.Root.Add(xdoc1.Root);
            xdoc.Save("save.xml");
        }

        private void lb_history_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            StackPanel stackPanel = lb_history.SelectedItem as StackPanel;
            Label label = stackPanel.Children[1] as Label;
            string[] temp = label.Content.ToString().Split(';');
            tb_limit_up.Text = temp[0].Trim();
            tb_limit_down.Text = temp[1].Trim();
            StraightDimension();
        }

        private void b_save_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            lb_save.Items.Add(StackPanel(button.Tag.ToString(), b_delete_Click));
            save.Add(button.Tag.ToString());
        }
        private void b_delete_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("delete");
        }

        private void lb_save_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
