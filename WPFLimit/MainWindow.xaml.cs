using System;
using System.Collections.Generic;
using System.Linq;
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


namespace WPFLimit
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TSM.Model model;
        TSD.DrawingHandler drawingHandler = new TSD.DrawingHandler();

        public MainWindow()
        {
            InitializeComponent();
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(!InitializeConnection())
            {
                MessageBox.Show("Нет конекта к модели :(");
                this.Close();
            }

        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TSD.Drawing drawing = drawingHandler.GetActiveDrawing();
            foreach (TSD.DrawingObject drawingObject in drawingHandler.GetDrawingObjectSelector().GetSelected())
            {
                //MessageBox.Show(drawingObject.GetType().ToString());
                if (drawingObject is TSD.StraightDimension)
                {
                    TSD.StraightDimension straightDimension = drawingObject as TSD.StraightDimension;
                    Prefix(straightDimension);
                }

                if (drawingObject is TSD.StraightDimensionSet)
                {
                    TSD.StraightDimensionSet straightDimensionSet = drawingObject as TSD.StraightDimensionSet;
                    TSD.StraightDimensionSet.StraightDimensionSetAttributes straightDimensionSetAttributes = new TSD.StraightDimensionSet.StraightDimensionSetAttributes();
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
            TSD.StraightDimension.StraightDimensionAttributes straightDimensionAttributes = new TSD.StraightDimension.StraightDimensionAttributes();
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
            if (tb_limit_up.Text.IndexOf("+") != -1 && tb_limit_down.Text.IndexOf("-") != -1 && tb_limit_up.Text[0] != tb_limit_down.Text[0])
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

            //MessageBox.Show(straightDimension.Attributes.DimensionValuePostfix.GetUnformattedString());
            //IEnumerator straightDimensionIE = straightDimension.Attributes.DimensionValuePostfix.GetEnumerator();
            //while (straightDimensionIE.MoveNext())
            //    MessageBox.Show(straightDimensionIE.Current.ToString());

            straightDimensionAttributes = straightDimension.Attributes;
            straightDimensionAttributes.DimensionValuePostfix = containerElement;
            straightDimension.Attributes = straightDimensionAttributes;
            straightDimension.Modify();
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
    }
}
