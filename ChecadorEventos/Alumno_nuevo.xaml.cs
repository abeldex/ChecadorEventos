using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChecadorEventos
{
    /// <summary>
    /// Lógica de interacción para Alumno_nuevo.xaml
    /// </summary>
    public partial class Alumno_nuevo
    {
        public Alumno_nuevo()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection();
                    values["cuenta"] = txt_cuenta.Text;
                    values["nombre"] = txt_nombre.Text;
                    values["correo"] = txt_correo.Text;
                    var response = client.UploadValues("http://facite.uas.edu.mx/agenda/registrar_alumno_app.php", values);

                    var responseString = Encoding.Default.GetString(response);
                    MessageBox.Show(responseString);
                    txt_correo.Text = "";
                    txt_cuenta.Text = "";
                    txt_nombre.Text = "";
                }
            }
            catch (Exception err) 
            {
                MessageBox.Show(err.Message);
            }
           
        }
    }
}
