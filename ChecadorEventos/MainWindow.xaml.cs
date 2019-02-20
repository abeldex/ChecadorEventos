using CefSharp;
using DPUruNet;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ChecadorEventos
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        MaterialDesignThemes.Wpf.PaletteHelper materialPaletteHelper = new MaterialDesignThemes.Wpf.PaletteHelper();
        PersonalUAU.DigitalPersona objMethods;
        Reader objReader;
        string xml;
        List<PersonalUAU.Clases.Persona> listPerson;

        public MainWindow()
        {
            InitializeComponent();
            objMethods = new PersonalUAU.DigitalPersona();
            //For one device detected by the computer
            objReader = objMethods.GetDevice();
            var border = (resultStack.Parent as ScrollViewer).Parent as Border;
            border.Visibility = Visibility.Collapsed;
            InitializeObjects();
        }

        private void InitializeObjects()
        {
            listPerson = new List<PersonalUAU.Clases.Persona>();
        }

        private void txt_buscar_KeyUp(object sender, KeyEventArgs e)
        {
            bool found = false;
            var border = (resultStack.Parent as ScrollViewer).Parent as Border;
            var data = GetEventos();

            string query = (sender as TextBox).Text;

            if (query.Length == 0)
            {
                // Clear   
                resultStack.Children.Clear();
                border.Visibility = Visibility.Collapsed;
            }
            else
            {
                border.Visibility = Visibility.Visible;
            }

            // Clear the list   
            resultStack.Children.Clear();

            // Add the result   
            foreach (var obj in data)
            {
                if (obj.Titulo.ToLower().Contains(query.ToLower()))
                {
                    // The word contains with this... Autocomplete must work   
                    addItem(obj.Titulo, obj.Descripcion, obj.Inicio, obj.Final, obj.Id.ToString());
                    found = true;
                }
            }
            if (!found)
            {
                resultStack.Children.Add(new TextBlock() { Text = "No se encontró ningun resultado." });
            }

        }

        private void addItem(string text,string desc, string fi, string ff, string id_evento)
        {
            TextBlock block = new TextBlock();
            block.FontSize = 12; // 24 points
            //block.Text = text;
            //lock.FontWeight = FontWeights.Bold;
            block.Inlines.Add(new Bold(new Run(text)));
            block.Inlines.Add(new Italic(new Run("\n"+desc)));
            block.Inlines.Add(new Run("\nInicia: " + fi + "  Termina: " + ff));
            block.TextWrapping = TextWrapping.Wrap;


            // A little style...   
            block.Margin = new Thickness(2, 3, 2, 3);
            block.Cursor = Cursors.Hand;

            // Mouse events   
            block.MouseLeftButtonUp += (sender, e) =>
            {
                var inline = (Bold)(sender as TextBlock).Inlines.ElementAt(0);
                var textRange = new TextRange(inline.ContentStart, inline.ContentEnd);
                txt_buscar.Text = textRange.Text;
                //txt_buscar.Text = (sender as TextBlock).Inlines.FirstInline;
                //obtener la descripcion
                var inline_desc = (Italic)( sender as TextBlock).Inlines.ElementAt(1);
                var textRange_desc = new TextRange(inline_desc.ContentStart, inline_desc.ContentEnd);
                lbl_desc.Content = textRange_desc.Text;

                var inline_fechas = (sender as TextBlock).Inlines.ElementAt(2);
                var textRange_fechas = new TextRange(inline_fechas.ContentStart, inline_fechas.ContentEnd);
                lbl_fechas.Content = textRange_fechas.Text;
                lbl_id_evento.Content = id_evento;

            };

            block.MouseEnter += (sender, e) =>
            {
                TextBlock b = sender as TextBlock;
                b.Background = Brushes.AliceBlue;
            };

            block.MouseLeave += (sender, e) =>
            {
                TextBlock b = sender as TextBlock;
                b.Background = Brushes.Transparent;
            };

            // Add to the panel   
            resultStack.Children.Add(block);
            Separator separator = new Separator();
            resultStack.Children.Add(separator);
        }

        //obtener el listado de eventos
        public List<entidades.Eventos> GetEventos()
        {
            WebClient wc = new WebClient();
            var json = wc.DownloadString("http://facite.uas.edu.mx/agenda/obtener_eventos_app.php");
            List<entidades.Eventos> data = JsonConvert.DeserializeObject<List<entidades.Eventos>>(json);
            return data;
        }

        //obtener el listado de alumnos
        public List<entidades.Alumnos> GetAlumnosHuellas()
        {
            WebClient wc = new WebClient();
            var json = wc.DownloadString("http://facite.uas.edu.mx/agenda/obtener_alumnos_huellas.php");
            List<entidades.Alumnos> data = JsonConvert.DeserializeObject<List<entidades.Alumnos>>(json);
            return data;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var border = (resultStack.Parent as ScrollViewer).Parent as Border;
            border.Visibility = Visibility.Collapsed;

            //MOSTRAR EL CHECADOR
            try
            {

                foreach (entidades.Alumnos dr in GetAlumnosHuellas())
                {
                    //Creamos un objeto de la clase persona
                    PersonalUAU.Clases.Persona objPersona = new PersonalUAU.Clases.Persona();
                    objPersona.id = dr.Cuenta;
                    objPersona.huella = dr.Huella;
                    objPersona.Nombre = dr.Nombre;
                    listPerson.Add(objPersona);
                }


                //verificamos si existe la persona (cliente)
                objMethods.StartIdentify(objReader, listPerson, txt_buscar.Text, lbl_desc.Content.ToString(), lbl_fechas.Content.ToString(), lbl_id_evento.Content.ToString());
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }


       

        private void ListBoxItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void ListBoxItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void ListBoxItem_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            Alumnos al = new Alumnos();
            al.ShowDialog();
            al.Topmost = true;
        }

        private async void btn_excel_Click(object sender, RoutedEventArgs e)
        {
           // Browser.ExecuteScriptAsync("exportToExcel();");
        }
    }
}
