using DPUruNet;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PersonalUAU.Identificacion
{
    public class Metodos
    {
        private const int DPFJ_PROBABILITY_ONE = 0x7fffffff;
        private Thread identifyThreadHandle;
        private bool reset = false;
        Reader objReader;
        DigitalPersona objReaderMethods;
        SelectorHuella.Metodos objDeviceReader;
        List<Clases.Persona> listPersons;
        VistaIdentificacion menuPrincipal;

        public Metodos(Reader objReader,List<Clases.Persona> listPersons,VistaIdentificacion menuPrincipal)
        {
            this.objReader = objReader;
            this.listPersons = listPersons;
            this.menuPrincipal = menuPrincipal;
            InitializeObjects();
        }

        private void InitializeObjects()
        {
            objReaderMethods = new DigitalPersona();
            objDeviceReader = new SelectorHuella.Metodos();
        }

        string _evento = "";

        public void StartIdentify(string id)
        {
            _evento = id;
            identifyThreadHandle = new Thread(IdentifyThread);
            identifyThreadHandle.IsBackground = true;
            identifyThreadHandle.Start();
        }

        private void IdentifyThread()
        {

            while (!reset)
            {
                Fid fid = null;

                if (!CaptureFinger(ref fid))
                {
                    //break;
                }

                if (objReader == null)
                {
                    objReader = objDeviceReader.IndexDevice();
                    objDeviceReader.InitializeDevice(ref objReader);
                }

                if (fid == null)
                {
                    continue;
                }

                DataResult<Fmd> resultConversion = FeatureExtraction.CreateFmdFromFid(fid, Constants.Formats.Fmd.ANSI);
                

                if (resultConversion.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    //break;

                    if (objReader != null)
                    {
                        objReader.Dispose();
                        objReader = null;
                    }
                    return;
                }
                    
                int thresholdScore = DPFJ_PROBABILITY_ONE * 1 / 100000;

                Fmd aux = resultConversion.Data;
                Fmd temp;
                foreach (Clases.Persona item in listPersons)
                {
                    try
                    {
                        temp = Fmd.DeserializeXml(item.huella);

                        CompareResult identifyResult = Comparison.Compare(aux, 0, temp, 0);

                        if (identifyResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                        {
                            break;
                        }

                        var responseString = "";

                        if (identifyResult.Score < thresholdScore)
                        {
                            //guardar la asistencia en la base de datos
                            try
                            {
                                using (var client = new WebClient())
                                {
                                    DateTime checada = DateTime.Now;    
                                    var values = new NameValueCollection();
                                    values["cuenta"] = item.id;
                                    values["evento"] = _evento;
                                    values["checada"] = checada.ToString("yyyy-MM-dd H:mm:ss");
                                    var response = client.UploadValues("http://facite.uas.edu.mx/agenda/registrar_checada.php", values);
                                    responseString = Encoding.Default.GetString(response);
                                }
                            }
                            catch (Exception err)
                            {
                                MessageBox.Show(err.Message);
                            }
                            
                            SendMessage(responseString, item.Nombre, "Se registró a las "+ DateTime.Now.ToShortTimeString(), "", "", "");

                            Thread.Sleep(3000);
                            SendMessage("Coloque el dedo en el checador...", "", "", "", "","");
                            menuPrincipal.Dispatcher.BeginInvoke(new Action(delegate ()
                            {
                                menuPrincipal.lbl_fechas.Content = "";
                                menuPrincipal.lbl_resto.Content = "";
                            }));
                            break;
                        }
                        else
                        {
                            //SendMessage("No se encuentra registrada la huella", "", "", "","","");
                            //
                            Thread.Sleep(2000);
                            SendMessage("Coloque el dedo en el checador...", "", "", "", "","");
                            menuPrincipal.Dispatcher.BeginInvoke(new Action(delegate ()
                            {
                                menuPrincipal.lbl_fechas.Content = "";
                                menuPrincipal.lbl_resto.Content = "";
                            }));
                            //break;
                        }
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.Message);
                    }
                    
                }
            }

            if (objReader != null)
                objReader.Dispose();
        }

        public bool CaptureFinger(ref Fid fid)
        {
            try
            {
                Constants.ResultCode result = objReader.GetStatus();

                if ((result != Constants.ResultCode.DP_SUCCESS))
                {
                    //MessageBox.Show("Get Status Error:  " + result);
                    if (objReader != null)
                    {
                        objReader.Dispose();
                        objReader = null;
                    }
                    return false;
                }

                if ((objReader.Status.Status == Constants.ReaderStatuses.DP_STATUS_BUSY))
                {
                    Thread.Sleep(50);
                    return true;
                }
                else if ((objReader.Status.Status == Constants.ReaderStatuses.DP_STATUS_NEED_CALIBRATION))
                {
                    objReader.Calibrate();
                }
                else if ((objReader.Status.Status != Constants.ReaderStatuses.DP_STATUS_READY))
                {
                    //MessageBox.Show("Get Status:  " + Lector.Status.Status);

                    if (objReader != null)
                    {
                        objReader.Dispose();
                        objReader = null;
                    }
                    return false;
                }

                CaptureResult captureResult = objReader.Capture(Constants.Formats.Fid.ANSI, Constants.CaptureProcessing.DP_IMG_PROC_DEFAULT, 5000, objReader.Capabilities.Resolutions[0]);

                if (captureResult.ResultCode != Constants.ResultCode.DP_SUCCESS)
                {
                    //MessageBox.Show("Error:  " + captureResult.ResultCode);

                    if (objReader != null)
                    {
                        objReader.Dispose();
                        objReader = null;
                    }
                    return false;
                }

                if (captureResult.Quality == Constants.CaptureQuality.DP_QUALITY_CANCELED)
                {
                    return false;
                }

                if ((captureResult.Quality == Constants.CaptureQuality.DP_QUALITY_NO_FINGER || captureResult.Quality == Constants.CaptureQuality.DP_QUALITY_TIMED_OUT))
                {
                    return true;
                }

                if ((captureResult.Quality == Constants.CaptureQuality.DP_QUALITY_FAKE_FINGER))
                {
                    //MessageBox.Show("Quality Error:  " + captureResult.Quality);

                    return true;
                }

                fid = captureResult.Data;

                return true;
            }
            catch
            {
                //MessageBox.Show("An error has occurred.");
                if (objReader != null)
                {
                    objReader.Dispose();
                    objReader = null;
                }
                return false;
            }
        }

        private delegate void SendMessageCallback(string payload);

        private void SendMessage(string payload, string nombre, string membresia, string fechai, string fechaf, string restantes)
        {
            menuPrincipal.Dispatcher.BeginInvoke(new Action(delegate()
            {
                menuPrincipal.lblMensaje.Content = payload;
                menuPrincipal.lblNombre.Content = nombre;
                menuPrincipal.lbl_fechas.Content = membresia;
                menuPrincipal.lbl_resto.Content = restantes;
            }));

        }

        public void StopIdentify()
        {
            if (objReader != null)
            {
                reset = true;
                objReader.CancelCapture();

                if (identifyThreadHandle != null)
                {
                    identifyThreadHandle.Join(5000);
                }
            }
            else
            {
                reset = true;
                if (identifyThreadHandle != null)
                {
                    identifyThreadHandle.Join(5000);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public Bitmap CreateBitmap(byte[] bytes, int width, int height)
        {
            byte[] rgbBytes = new byte[bytes.Length * 3];

            for (int i = 0; i <= bytes.Length - 1; i++)
            {
                rgbBytes[(i * 3)] = bytes[i];
                rgbBytes[(i * 3) + 1] = bytes[i];
                rgbBytes[(i * 3) + 2] = bytes[i];
            }
            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            for (int i = 0; i <= bmp.Height - 1; i++)
            {
                IntPtr p = new IntPtr(data.Scan0.ToInt32() + data.Stride * i);
                System.Runtime.InteropServices.Marshal.Copy(rgbBytes, i * bmp.Width * 3, p, bmp.Width * 3);
            }

            bmp.UnlockBits(data);

            return bmp;
        }

    }
}
