using Newtonsoft.Json;

namespace ChecadorEventos.entidades
{
    public class Alumnos
    {
        [JsonProperty("numCuenta")]
        public string Cuenta { get; set; }

        [JsonProperty("NombreAlumno")]
        public string Nombre { get; set; }

        [JsonProperty("Correo")]
        public string Correo { get; set; }

        [JsonProperty("Huella")]
        public string Huella { get; set; }
    }
}
