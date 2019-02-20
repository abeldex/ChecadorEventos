using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChecadorEventos.datos
{
    class Tablas
    {
        //creacion del objeto de la base de datos
        private conexionSQLITE conexion;

        public Tablas()
        {
            //inicializacion de la base de datos
            conexion = new conexionSQLITE();
        }

        public void VerificarTablaLetras()
        {
            try
            {
                if (conexion.AbrirConexion())
                {
                    //Create the command
                    SQLiteCommand sqlite_cmd = conexion.RetornarConexion().CreateCommand();
                    // Let the SQLiteCommand object know our SQL-Query:
                    sqlite_cmd.CommandText = @"CREATE TABLE IF NOT EXISTS
                                              [Alumnos] (
                                              [numCuenta] TEXT NOT NULL PRIMARY KEY,
                                              [NombreAlumno] TEXT NOT NULL,
                                              [Correo] TEXT NULL,
                                              [Huella] BLOB NULL)";
                    // Now lets execute the SQL ;-)
                    sqlite_cmd.ExecuteNonQuery();
                    conexion.CerrarConexion();
                }

            }
            catch (Exception)
            {
                throw;
            }
        }


    }


}
