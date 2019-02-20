using Newtonsoft.Json;

namespace ChecadorEventos.entidades
{
    public class Eventos
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Titulo { get; set; }

        [JsonProperty("body")]
        public string Descripcion { get; set; }

        [JsonProperty("inicio_normal")]
        public string Inicio { get; set; }

        [JsonProperty("final_normal")]
        public string Final { get; set; }

    }
}
