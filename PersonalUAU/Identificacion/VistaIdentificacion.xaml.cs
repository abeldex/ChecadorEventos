using DPUruNet;
using System;
using System.Collections.Generic;

using System.Windows.Threading;

namespace PersonalUAU.Identificacion
{
    /// <summary>
    /// Lógica de interacción para VistaIdentificacion.xaml
    /// </summary>
    public partial class VistaIdentificacion 
    {
        Reader objReader;
        Metodos objReaderMethods;
        List<Clases.Persona> listPersons;
        public PersonalUAU.Clases.Persona objPersona;
        public bool exitIdentify;
        string _id = "";

        public VistaIdentificacion(Reader objReader, List<Clases.Persona> listPersons, string evento, string descripcion, string fechas, string id)
        {
            InitializeComponent();
            this.objReader = objReader;
            this.listPersons = listPersons;
            this.txt_evento.Content = fechas.Remove(0,1);
            this.txt_descripcion.Content = descripcion.Remove(0,1);
            this.Title = "["+id+"] " +evento;
            this._id = id;

            initializeObjects();
            StartIdenfityThread();
            DispatcherTimer d = new DispatcherTimer();
            //intervalo de 1 segundo
            d.Interval = new TimeSpan(0, 0, 1);
            d.Tick += (s, a) =>
              {
                  String fecha = System.DateTime.Now.ToString("HH:mm:ss");
                  lblHora.Content = fecha;
              };
            d.Start();
        }

        public void initializeObjects()
        {
            objReaderMethods = new Metodos(objReader,listPersons,this);
        }

        public void StartIdenfityThread()
        {
            objReaderMethods.StartIdentify(_id);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            objReaderMethods.StopIdentify();
        }
    }
}
