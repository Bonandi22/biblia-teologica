namespace BibliaTeologica.Application.Common.Interfaces;

/// <summary>Fila de envio assíncrono: o chamador não espera o SMTP, logo não há canal de tempo.</summary>
public interface IFilaEmail
{
    void Enfileirar(string para, string assunto, string corpoHtml);
}
