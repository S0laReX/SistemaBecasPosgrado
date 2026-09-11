using SistemaBecasWeb.Models;
using System.Collections.Generic;
namespace SistemaBecasWeb.Repositories
{
    public interface ISolicitudRepository
    {
        List<OfertaViewModel> ObtenerOfertasVigentes();
        List<OfertaViewModel> ObtenerOfertasVigentesDetalle();

        List<UniversidadViewModel> ObtenerUniversidades();
        UniversidadViewModel ObtenerUniversidadPorId(int id);
        void CrearUniversidad(UniversidadViewModel uni);
        void ActualizarUniversidad(UniversidadViewModel uni);
        void EliminarUniversidad(int id);
        string RegistrarSolicitud(string docIdentidad, int idOferta, string resumen);
        int ContarAceptados(int idOferta);

        // Panel administrativo de solicitudes
        List<SolicitudViewModel> ObtenerSolicitudes();
        string AceptarSolicitud(int idSolicitud);

        List<ProgramaViewModel> ObtenerProgramas();
        ProgramaViewModel ObtenerProgramaPorCod(string cod);
        void CrearPrograma(ProgramaViewModel prog);
        void ActualizarPrograma(ProgramaViewModel prog);
        void EliminarPrograma(string cod);

        // CRUD Ofertas
        List<OfertaViewModel> ObtenerTodasOfertas();
        OfertaViewModel ObtenerOfertaPorId(int id);
        void CrearOferta(OfertaViewModel oferta);
        void ActualizarOferta(OfertaViewModel oferta);
        void EliminarOferta(int id);

        // Métodos para los Postulantes
        void CrearPostulante(string doc, string nom, string ape, string correo, string nivel);
        void AgregarExperiencia(string doc, string empresa, string cargo, DateTime inicio, DateTime? fin);

        List<PostulanteViewModel> ObtenerPostulantes();

        List<SedeViewModel> ObtenerSedes();
        SedeViewModel ObtenerSedePorCod(string cod);
        void CrearSede(SedeViewModel sede);
        void ActualizarSede(SedeViewModel sede);
        void EliminarSede(string cod);
    }
}
