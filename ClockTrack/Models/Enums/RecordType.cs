using System.ComponentModel.DataAnnotations;

namespace ClockTrack.Models.Enums
{
    public enum RecordType
    {
        Consultivo = 0,
        Contencioso = 1,
        [Display(Name = "Proposta Específica")]
        PropostaEspecifica = 2,
        Deslocamento = 3

    }
}
