using DPUruNet;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
    /// Lógica de interacción para Alumnos.xaml
    /// </summary>
    public partial class Alumnos
    {
        PersonalUAU.DigitalPersona objMethods;
        Reader objReader;
        string xml;
        List<PersonalUAU.Clases.Persona> listPerson;
        private static BackgroundWorker backgroundWorker;

        public Alumnos()
        {
            InitializeComponent();
            objMethods = new PersonalUAU.DigitalPersona();
            //For one device detected by the computer
            objReader = objMethods.GetDevice();
            InitializeObjects();
            ThreadPool.QueueUserWorkItem(delegate { ListarAlumnos(); });
        }


        private void InitializeObjects()
        {
            listPerson = new List<PersonalUAU.Clases.Persona>();
        }

        private void ListarAlumnos()
        {
            backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            backgroundWorker.DoWork += delegate (object s, DoWorkEventArgs args)
            {
                WebClient wc = new WebClient();
                var json = wc.DownloadString("http://facite.uas.edu.mx/agenda/obtener_alumnos_app.php");
                List<entidades.Alumnos> data = JsonConvert.DeserializeObject<List<entidades.Alumnos>>(json);

                this.Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    dg_alumnos.ItemsSource = data;
                }));

            };

            backgroundWorker.RunWorkerAsync();

        }

        //obtener el listado de Alumnos
        public List<entidades.Alumnos> GetAlumnos()
        {
            WebClient wc = new WebClient();
            var json = wc.DownloadString("http://facite.uas.edu.mx/agenda/obtener_alumnos_app.php");
            List<entidades.Alumnos> data = JsonConvert.DeserializeObject<List<entidades.Alumnos>>(json);
            return data;
        }

        private async void btn_perfil_Click(object sender, RoutedEventArgs e)
        {
            //obtenemos el usuario desde el datagrid por medio de la fila seleccionada
            object alumno = ((Button)sender).CommandParameter;

            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Sí, Capturar",
                NegativeButtonText = "No, Cancelar",
                AnimateShow = true,
                AnimateHide = false
            };

            var result = await this.ShowMessageAsync("Capturar Huella", "¿Desea capturar la huella del Alumno: " + alumno + "?", MessageDialogStyle.AffirmativeAndNegative, mySettings);
            if (result.ToString() == "Affirmative")
            {
                registrar_huella_php(alumno.ToString());
            }

        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private void registrar_huella_php(string alumno)
        {
            try
            {
                objMethods.ShowWindowEnrollment(objReader);
                xml = objMethods.GetFingerprint_XML();
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection();
                    values["cuenta"] = alumno;
                    values["huella"] = Base64Encode(xml);
                    var response = client.UploadValues("http://facite.uas.edu.mx/agenda/actualizar_huella.php", values);

                    var responseString = Encoding.Default.GetString(response);
                    MessageBox.Show(responseString);
                }
                //string res = PostMessageToURL(alumno, xml);
                /*if (new datos.Da_Alumnos().InsertarHuella(alumno, xml))
                {
                    MessageBox.Show("Huella registrada correctamente");
                    //ListarAlumnos();
                }*/
            }
            catch (System.Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void registrar_huella(string alumno)
        {
            try
            {
                objMethods.ShowWindowEnrollment(objReader);
                xml = objMethods.GetFingerprint_XML();
                //string res = PostMessageToURL(alumno, xml);
                if (new datos.Da_Alumnos().InsertarHuella(alumno, xml))
                {
                    MessageBox.Show("Huella registrada correctamente");
                    //ListarAlumnos();
                }
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        private void txt_buscar_KeyUp(object sender, KeyEventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var collectionView = CollectionViewSource.GetDefaultView(dg_alumnos.ItemsSource);
            collectionView.Filter = item => {
                entidades.Alumnos vitem = item as entidades.Alumnos;
                return vitem.Nombre.Contains(txt_buscar.Text);
            };
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Alumno_nuevo an = new Alumno_nuevo();
            an.ShowDialog();
        }
    }
}
