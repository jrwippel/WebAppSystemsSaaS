using ClockTrack.Models;

namespace ClockTrack.Helper
{
    public interface ISessao
    {
        void CriarSessaoDoUsuario(Attorney attorney);
        void RemoverSessaoDoUsuario();
        Attorney BuscarSessaoDoUsuario();

    }
}
