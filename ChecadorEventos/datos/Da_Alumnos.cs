using MySql.Data.MySqlClient;
using System.Data;
using System;
using System.Collections.Generic;

namespace ChecadorEventos.datos
{
    public class Da_Alumnos
    {
        public bool InsertarHuella(string cuenta, string xml)
        {
            using (var cn = new MySqlConnection(conexion.LeerCC))
            {
                try
                {
                    using (var cmd = new MySqlCommand("UPDATE Alumnos SET Huella = @huella WHERE numCuenta=@cuenta", cn))
                    {
                        cmd.Parameters.Add(new MySqlParameter("huella", xml));
                        cmd.Parameters.Add(new MySqlParameter("cuenta", cuenta));
                        //abrimos conexion y ejecutamos
                        cn.Open();
                        // Ejecutamos el comando y regresamos el resultado (True = correcto, False = error)
                        return Convert.ToBoolean(cmd.ExecuteNonQuery());
                    }
                }
                catch (Exception error)
                {

                    throw new Exception(error.Message);
                }
            }
        }
    }
}
