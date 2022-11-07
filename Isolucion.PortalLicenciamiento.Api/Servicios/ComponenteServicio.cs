using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using AutoMapper;
using Isolucion.PortalLicenciamiento.Api.Enumeradores;
using Isolucion.PortalLicenciamiento.Api.Modelo;
using Isolucion.PortalLicenciamiento.Api.Modelo.Dto;
using Isolucion.PortalLicenciamiento.Api.Repositorios;

namespace Isolucion.PortalLicenciamiento.Api.Servicios
{
   public class ComponenteServicio : IComponenteServicio
   {
      private readonly IMapper _automapper;
      private readonly IComponenteRepositorio _repositorio;
      private readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
      public ComponenteServicio(IComponenteRepositorio repositorio, IMapper automapper)
      {
         _repositorio = repositorio;
         _automapper = automapper;
      }

      public VersionDto ObtenerVersion(int idWebSite)
      {
         try
         {
            return _automapper.Map<VersionDto>(_repositorio.ObtenerVersion(idWebSite));
         }
         catch (Exception e)
         {
            Logger.Error(e);
            return null;
         }
      }

      public int LogProcesoObtenerComponentes(ActualizacionLogDto log)
      {
         try
         {
            if (log.Estado == EstadoEnum.Fallido.ToString())
            {
               SenMail(log);
            }

            return _repositorio.LogProcesoObtenerComponentes(_automapper.Map<ActualizacionLog>(log));
         }
         catch (Exception e)
         {
            Logger.Error(e);
            return 0;
         }
      }

      public void GuardarInformacionCliente(InformacionCliente info)
      {
         try
         {
            _repositorio.GuardarInformacionCliente(info);
         }
         catch (Exception e)
         {
            Logger.Error(e);
         }
      }
      private void SenMail(ActualizacionLogDto log)
      {
         try
         {
            var emailsTo = ConfigurationManager.AppSettings["EmailFrom"].Split(',');
            MailAddress from = new MailAddress(ConfigurationManager.AppSettings["EmailTo"]);
            var client = new SmtpClient("mail.clubchampionteam.com", 25);

            foreach (var emailTo in emailsTo)
            {
               MailAddress to = new MailAddress(emailTo);
               var message = new MailMessage(from, to);
               //var dir = string.Format("{0}{1}", Request.PhysicalApplicationPath, "/content/plantilla.html");
               //string body = string.Empty;

               //using (var sr = new StreamReader(dir))
               //{
               //   var text = sr.ReadToEnd();
               //   body = string.Format(text, contact.Nombre, contact.Email, contact.Telefono, contact.Mensaje);
               //}

               message.Body = log.Stack_Tarace;
               message.Subject = $"Ocurrio un error almomento de actualizar la aplicacion del cliente: , {log.CodCliente}";
               message.IsBodyHtml = false;
               message.SubjectEncoding = Encoding.UTF8;
               client.Send(message);
            }




         }
         catch (Exception e)
         {
            Logger.Error(e);
         }
      }
   }
}