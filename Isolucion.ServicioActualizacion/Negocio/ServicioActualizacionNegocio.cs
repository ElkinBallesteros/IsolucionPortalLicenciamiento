using Autofac;
using Isolucion.PortalLicenciamiento.Api.Modelo.Dto;
using Isolucion.ServicioActualizacion.Common;
using Isolucion.ServicioActualizacion.Enumeradores;
using Isolucion.ServicioActualizacion.Modelo;
using Isolucion.ServicioActualizacion.Repositorios;
using Microsoft.Web.Administration;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

//using File = Microsoft.SharePoint.Client.File;

namespace Isolucion.ServicioActualizacion.Negocio
{
   public class ServicioActualizacionNegocio : IServicioActualizacionNegocio
   {
      /// <summary>
      /// Interface para acceder a la capa de servicios
      /// </summary>
      private readonly IWebSiteRepositorio _repositorio;
      /// <summary>
      /// Instancia de logger para loguear los pasos del proceso y/o errores
      /// </summary>
      private readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
      /// <summary>
      /// Constructor de la clase
      /// </summary>
      /// <param name="servicio">The servicio.</param>
      public ServicioActualizacionNegocio(IWebSiteRepositorio repositorio)
      {
         _repositorio = repositorio;
      }

      /// <summary>
      /// Actualizars the aplicacion.
      /// </summary>
      public async Task ActualizarAplicacion()
      {
         var resumenProceso = new StringBuilder($"{DateTime.Now:yy-MM-dd hh:MM:ss} Inicia proceso de Actualizacion");

         var webSite = new WEBSITE();
         try
         {
            Logger.Info("consultando version");
            webSite = _repositorio.ObtenerInformacionWebsite();

            ObtenerInformacionCliente(webSite.Id_WebSite);

            if (webSite.Id_WebSite == 0)
            {
               Logger.Info("No se encontro la informacion para realizar el proceso");
               return;
            }

            Logger.Info($"Requiere Actualizacion {webSite.RequiereActualizacion.BooleanToString()}");
            resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Requiere Actualizacion {webSite.RequiereActualizacion.BooleanToString()}");
            //Determina si el cliente requiere actualizacion de la (s) aplicacion(es)
            if (webSite.RequiereActualizacion)
            {
               //Se adiciona esta bandera para controlar el proceso de actualizacion
               ConstantesGlobales.EnEjecucion = true;

               var componentes = ObtenerConfiguracion();

               Logger.Info($"Se encontraron  {componentes.Count} Aplicaciones para Actualizacion");
               resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Se encontraron  {componentes.Count} Aplicaciones para Actualizacion");
               //Se valida que las rutas de los sitios esten creadas si no existen las crea
               ValidarRutasFisicas(resumenProceso);

               //Descompimir los archivos
               DescomprimirArchivos(resumenProceso);

               foreach (var componente in componentes)
               {
                  //Sacar un backup de todas las aplicaciones antes de Iniciar el proceso de actualizacion
                  BackupArchivosAplicaciones(componente, resumenProceso);
                  //Se detienen los servicios y los pool de las aplicaciones
                  IniciarDetenerServicios(componente, true, resumenProceso);

                  if (ConstantesGlobales.DescargaOnline)
                  {
                     await DescargarArchivosVersion(webSite.RutaDescarga, componente.NombreArchivoZip);
                  }

                  //Actualizar la version
                  CopiarAppFiles(componente, resumenProceso);

                  //Se reinician los servicios y los pool de aplicaciones
                  IniciarDetenerServicios(componente, false, resumenProceso);

                  var log = new ActualizacionLogDto
                  {
                     CodCliente = webSite.Id_WebSite,
                     Fechas = DateTime.Now,
                     Mensaje = resumenProceso.ToString(),
                     Estado = EstadoEnum.Exitoso.ToString()
                  };
               }

               //Se actualiza el campo RequiereActualizacion a false una vez finalizado el proceso lo cual significa que la aplicacion esta actualizada
               _repositorio.ActualizarWebsite();
               // Al finalizar el proceso se pone en falso para que la siguiente ejecucion pueda entrar a verificar si puede realizar la actualizacion o no
               ConstantesGlobales.EnEjecucion = false;

               using (HttpClient client = new HttpClient())
               {
                  var log = new ActualizacionLogDto
                  {
                     CodCliente = webSite.Id_WebSite,
                     Fechas = DateTime.Now,
                     Mensaje = $"Actualizacion Exitosa {resumenProceso.ToString()}",
                     Estado = EstadoEnum.Exitoso.ToString()
                  };
                  StringContent content = new StringContent(JsonConvert.SerializeObject(log), Encoding.UTF8, "application/json");
                  var response = await client.PostAsync(new Uri($"{ConstantesGlobales.PortalLicenciamientoApiUrl}LogProceso"), content);
               }
            }
         }
         catch (Exception e)
         {
            Logger.Error(e);

            using (HttpClient client = new HttpClient())
            {
               var log = new ActualizacionLogDto
               {
                  CodCliente = webSite.Id_WebSite,
                  Fechas = DateTime.Now,
                  Mensaje = e.Message,
                  Stack_Tarace = e.StackTrace,
                  Estado = EstadoEnum.Fallido.ToString()
               };
               StringContent content = new StringContent(JsonConvert.SerializeObject(log), Encoding.UTF8, "application/json");
               var response = await client.PostAsync(new Uri($"{ConstantesGlobales.PortalLicenciamientoApiUrl}LogProceso"), content);
            }


            _repositorio.ActualizarWebsite();
         }
         finally

         {
            ConstantesGlobales.EnEjecucion = false;
         }
      }

      /// <summary>
      /// Inicia o detiene los sitios web y los pool de aplicaciones asociados
      /// </summary>
      /// <param name="componente">Informacion de la aplicacion.</param>
      /// <param name="detener">true para detener, false para iniciar.</param>
      private void IniciarDetenerServicios(ComponenteDto componente, bool detener, StringBuilder resumenProceso)
      {
         if (detener)
         {
            switch ((TipoAplicacion)componente.TipoComponente)
            {
               case TipoAplicacion.Web:
                  //Se detienen los sitios web y los apppol de la (s) aplicacion (es)
                  StopWebSite(componente.NombresSistema.Split(','));
                  Logger.Info($"Sitios web {string.Join(",", componente.NombresSistema.Split(','))} detenidos");
                  resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Sitios web {string.Join(",", componente.NombresSistema.Split(','))} detenidos");

                  StopPool(componente.NombresPool.Split(','));
                  Logger.Info($"ApplicationPool {string.Join(",", componente.NombresPool.Split(','))} detenidos");
                  resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} ApplicationPool {string.Join(",", componente.NombresPool.Split(','))} detenidos");
                  break;
               case TipoAplicacion.Win:
                  Logger.Info($"Servicio {componente.NomComponente} detenido");
                  resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Servicio {componente.NomComponente} detenido");
                  StopWinApp(componente.NomComponente);
                  break;
            }
         }
         else
         {
            switch ((TipoAplicacion)componente.TipoComponente)
            {
               case TipoAplicacion.Web:
                  //Se detienen los sitios web y los apppol de la (s) aplicacion (es)
                  StartWebSite(componente.NombresSistema.Split(','));
                  Logger.Info($"Sitios web {string.Join(",", componente.NombresSistema.Split(','))} Iniciadas");
                  resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Sitios web {string.Join(",", componente.NombresSistema.Split(','))} Iniciadas");

                  StartPool(componente.NombresPool.Split(','));
                  Logger.Info($"ApplicationPool {string.Join(",", componente.NombresPool.Split(','))} Iniciados");
                  resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} ApplicationPool {string.Join(",", componente.NombresPool.Split(','))} Iniciados");
                  break;
               case TipoAplicacion.Win:
                  StartWinApp(componente.NomComponente);
                  Logger.Info($"Servicio {componente.NomComponente} iniciado");
                  resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Servicio {componente.NomComponente} iniciado");
                  break;
            }
         }
      }

      /// <summary>
      /// Obtiene la configuracion de las aplicaciones para iniciar el proceso de actualizacion.
      /// </summary>
      /// <returns></returns>
      private List<ComponenteDto> ObtenerConfiguracion()
      {
         var componentes = new List<ComponenteDto>();

         foreach (var app in ConstantesGlobales.Aplicaciones)
         {
            var com = new ComponenteDto { NomComponente = app };

            switch (app)
            {
               case "IsolucionWeb":
                  com.TipoComponente = 1;
                  com.NombresSistema = $"{ConstantesGlobales.EstandarWebNombres},{ConstantesGlobales.EstandarTestWebNombres}";
                  com.NombresPool = $"{ConstantesGlobales.poolEstandar},{ConstantesGlobales.poolEstandarTEST}";
                  break;
               case "Isolucion.Api":
                  com.TipoComponente = 1;
                  com.NombresSistema = ConstantesGlobales.ApiDataConectorWebNombres;
                  com.NombresPool = $"{ConstantesGlobales.poolApiDataConnector}";
                  break;
               case "Isolucion.ApiGestionCambio":
                  com.TipoComponente = 1;
                  com.NombresSistema = ConstantesGlobales.ApiGestionDelCambioNombres;
                  com.NombresPool = ConstantesGlobales.poolApGestionCamio;
                  break;
               case "Isolucion.ApiRiesgoDafp":
                  com.TipoComponente = 1;
                  com.NombresSistema = ConstantesGlobales.ApiRiesgoDafpNombres;
                  com.NombresPool = $"{ConstantesGlobales.poolApiRiesgosDafp}";
                  break;
               case "Isolucion.ApiSyncFusion":
                  com.TipoComponente = 1;
                  break;
               case "Isolucion.Servicio":
                  com.TipoComponente = 2;
                  com.NombreServicio = ConstantesGlobales.IsolucionServicioNombre;
                  break;
               case "xProjectAnalyzer":
                  com.TipoComponente = 2;
                  com.NombreServicio = ConstantesGlobales.xProjectAnalyzerNombre;
                  break;
               case "GenericEventHandler":
                  com.TipoComponente = 2;
                  com.NombreServicio = ConstantesGlobales.GenericEventHandler;
                  break;
            }

            componentes.Add(com);
         }

         return componentes;
      }
      /// <summary>
      /// Realiza la validacion de las rutas fisicas necesarias para copiar los archivos
      /// </summary>
      private void ValidarRutasFisicas(StringBuilder resumenProceso)
      {
         Logger.Info($"Validando Rutas fisicas de las aplicaciones");
         resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Validando Rutas fisicas de las aplicaciones");

         if (!string.IsNullOrWhiteSpace(RutasConfiguracion.PathDescargaLocal) &&
             !Directory.Exists(RutasConfiguracion.PathDescargaLocal))
         {
            Directory.CreateDirectory(RutasConfiguracion.PathDescargaLocal);
            Logger.Info($"Ruta {RutasConfiguracion.PathDescargaLocal} No encontrada Se creo la carpeta {RutasConfiguracion.PathDescargaLocal}");
            resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Ruta {RutasConfiguracion.PathDescargaLocal} No encontrada Se creo la carpeta {RutasConfiguracion.PathDescargaLocal}");
         }

         if (!string.IsNullOrWhiteSpace(RutasConfiguracion.PathEstandar) &&
             !Directory.Exists(RutasConfiguracion.PathEstandar))
         {
            Directory.CreateDirectory(RutasConfiguracion.PathEstandar);
            Logger.Info($"Ruta {RutasConfiguracion.PathEstandar} No encontrada Se creo la carpeta {RutasConfiguracion.PathEstandar}");
            resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Ruta {RutasConfiguracion.PathEstandar} No encontrada Se creo la carpeta {RutasConfiguracion.PathEstandar}");
         }

         if (!string.IsNullOrWhiteSpace(RutasConfiguracion.PathEstandarTEST) &&
             !Directory.Exists(RutasConfiguracion.PathEstandarTEST))
         {
            Directory.CreateDirectory(RutasConfiguracion.PathEstandarTEST);
            Logger.Info($"Ruta {RutasConfiguracion.PathEstandarTEST} No encontrada Se creo la carpeta {RutasConfiguracion.PathEstandarTEST}");
            resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Ruta {RutasConfiguracion.PathEstandarTEST} No encontrada Se creo la carpeta {RutasConfiguracion.PathEstandarTEST}");
         }

         if (!string.IsNullOrWhiteSpace(RutasConfiguracion.PathApi) && !Directory.Exists(RutasConfiguracion.PathApi))
         {
            Directory.CreateDirectory(RutasConfiguracion.PathApi);
            Logger.Info($"Ruta {RutasConfiguracion.PathApi} No encontrada Se creo la carpeta {RutasConfiguracion.PathApi}");
            resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Ruta {RutasConfiguracion.PathApi} No encontrada Se creo la carpeta {RutasConfiguracion.PathApi}");
         }

         if (!string.IsNullOrWhiteSpace(RutasConfiguracion.PathApiRiesgoDafp) &&
             !Directory.Exists(RutasConfiguracion.PathApiRiesgoDafp))
         {
            Directory.CreateDirectory(RutasConfiguracion.PathApiRiesgoDafp);
            Logger.Info($"Ruta {RutasConfiguracion.PathApiRiesgoDafp} No encontrada Se creo la carpeta {RutasConfiguracion.PathApiRiesgoDafp}");
            resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Ruta {RutasConfiguracion.PathApiRiesgoDafp} No encontrada Se creo la carpeta {RutasConfiguracion.PathApiRiesgoDafp}");
         }

         if (!string.IsNullOrWhiteSpace(RutasConfiguracion.PathApiGestionCambio) &&
             !Directory.Exists(RutasConfiguracion.PathApiGestionCambio))
         {
            Directory.CreateDirectory(RutasConfiguracion.PathApiGestionCambio);
            Logger.Info($"Ruta {RutasConfiguracion.PathApiGestionCambio} No encontrada Se creo la carpeta {RutasConfiguracion.PathApiGestionCambio}");
            resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Ruta {RutasConfiguracion.PathApiGestionCambio} No encontrada Se creo la carpeta {RutasConfiguracion.PathApiGestionCambio}");
         }

         if (!string.IsNullOrWhiteSpace(RutasConfiguracion.PathServicio) &&
             !Directory.Exists(RutasConfiguracion.PathServicio))
         {
            Directory.CreateDirectory(RutasConfiguracion.PathServicio);
            Logger.Info($"Ruta {RutasConfiguracion.PathServicio} No encontrada Se creo la carpeta {RutasConfiguracion.PathServicio}");
            resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Ruta {RutasConfiguracion.PathServicio} No encontrada Se creo la carpeta {RutasConfiguracion.PathServicio}");
         }

         if (!string.IsNullOrWhiteSpace(RutasConfiguracion.PathGenericHandler) &&
             !Directory.Exists(RutasConfiguracion.PathGenericHandler))
         {
            Directory.CreateDirectory(RutasConfiguracion.PathGenericHandler);
            Logger.Info($"Ruta {RutasConfiguracion.PathGenericHandler} No encontrada Se creo la carpeta {RutasConfiguracion.PathGenericHandler}");
            resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Ruta {RutasConfiguracion.PathGenericHandler} No encontrada Se creo la carpeta {RutasConfiguracion.PathGenericHandler}");
         }

         if (!string.IsNullOrWhiteSpace(RutasConfiguracion.PathxProjectAnalyzer) &&
             !Directory.Exists(RutasConfiguracion.PathxProjectAnalyzer))
         {
            Directory.CreateDirectory(RutasConfiguracion.PathxProjectAnalyzer);
            Logger.Info($"Ruta {RutasConfiguracion.PathxProjectAnalyzer} No encontrada Se creo la carpeta {RutasConfiguracion.PathxProjectAnalyzer}");
            resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Ruta {RutasConfiguracion.PathxProjectAnalyzer} No encontrada Se creo la carpeta {RutasConfiguracion.PathxProjectAnalyzer}");
         }

         if (!string.IsNullOrWhiteSpace(RutasConfiguracion.PathIndexador) &&
             !Directory.Exists(RutasConfiguracion.PathIndexador))
         {
            Directory.CreateDirectory(RutasConfiguracion.PathIndexador);
            Logger.Info($"Ruta {RutasConfiguracion.PathIndexador} No encontrada Se creo la carpeta {RutasConfiguracion.PathIndexador}");
            resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Ruta {RutasConfiguracion.PathIndexador} No encontrada Se creo la carpeta {RutasConfiguracion.PathIndexador}");
         }
      }

      private async Task DescargarArchivosVersion(string rutaDescarga, string nombreArchivo)
      {

         //HttpWebRequest wr = (HttpWebRequest)WebRequest.Create($"{rutaDescarga}");
         //HttpWebResponse ws = (HttpWebResponse)wr.GetResponse();
         //Stream str = ws.GetResponseStream();
         //using (HttpClient client2 = new HttpClient())
         //{
         //   HttpResponseMessage response = client2.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
         //}

         //using (WebClient client = new WebClient())
         //{
         //   client.DownloadFile($"{rutaDescarga}{nombreArchivo}", $"{RutasConfiguracion.PatDescarga}ApiGestionCambio.zip");
         //}


         using (HttpClient client = new HttpClient())
         {
            client.MaxResponseContentBufferSize = 9999999;
            string url = $"{rutaDescarga}";
            using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
            {

               byte[] inBuf = new byte[100000];
               int bytesToRead = (int)inBuf.Length;
               int bytesRead = 0;
               while (bytesToRead > 0)
               {
                  int n = streamToReadFrom.Read(inBuf, bytesRead, bytesToRead);
                  if (n == 0)
                     break;
                  bytesRead += n;
                  bytesToRead -= n;
               }

               FileStream fstr = new FileStream($"{RutasConfiguracion.PatDescarga}Iso.pdf",
                  FileMode.OpenOrCreate, FileAccess.Write);
               fstr.Write(inBuf, 0, bytesRead);
               streamToReadFrom.Close();
               fstr.Close();
            }
         }
         //   ClientContext cxt = new ClientContext(rutaDescarga);
         //List list = cxt.Web.Lists.GetByTitle("Documents");

         //using (var httpClient = new HttpClient())
         //{
         //   var contentsJson = await httpClient.GetStringAsync(rutaDescarga);
         //   var contents = (JObject)JsonConvert.DeserializeObject(contentsJson);
         //   //nextPageToken = (string)contents["nextPageToken"];
         //   foreach (var file in (JArray)contents["files"])
         //   {
         //      var id = (string)file["id"];
         //      var name = (string)file["name"];
         //      Console.WriteLine($"{id}:{name}");
         //   }
         //}

         //using (HttpClient client = new HttpClient())
         //{
         //   string url = $"{rutaDescarga}{nombreArchivo}";
         //   using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
         //   using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
         //   {
         //      string fileToWriteTo = Path.GetTempFileName();
         //      using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
         //      {
         //         await streamToReadFrom.CopyToAsync(streamToWriteTo);
         //      }
         //   }
         //}

         using (var client1 = new HttpClient())
         {
            //client.MaxResponseContentBufferSize = long.MaxValue;
            //var response = await client1.GetAsync($"{rutaDescarga}{nombreArchivo}");
            //response.EnsureSuccessStatusCode();
            ////var bytes = await response.Content.ReadAsByteArrayAsync();
            //var input = await response.Content.ReadAsStreamAsync();
            //var fs = new System.IO.FileStream($"{RutasConfiguracion.PatDescarga}Isolucion.ApiRiesgoDafp.zip", FileMode.Create, FileAccess.Write);

            //using (var webClient = new WebClient())
            //{
            //   webClient.DownloadFileAsync(new Uri($"{rutaDescarga}{nombreArchivo}"), $"{RutasConfiguracion.PatDescarga}Isolucion.ApiRiesgoDafp.zip");
            //}

            //int readCount;
            //byte[] buffer = new byte[1024];
            //while ((readCount = input.Read(buffer, 0, 1024)) != 0)
            //{
            //   fs.Write(buffer, 0, readCount);
            //   fs.Flush();
            //   Console.WriteLine("copying");
            //}

         }

         //using (var client = new HttpClient())
         //{
         //   //client.MaxResponseContentBufferSize = long.MaxValue;
         //   using (var response = await client.GetAsync($"{rutaDescarga}{nombreArchivo}"))
         //   {
         //      response.EnsureSuccessStatusCode();
         //      var bytes = await response.Content.ReadAsByteArrayAsync();
         //      MemoryStream ms = new MemoryStream(bytes, writable: false);
         //      var fs = new System.IO.FileStream($"{RutasConfiguracion.PatDescarga}Isolucion.ApiRiesgoDafp.zip", FileMode.Create, FileAccess.Write);
         //      File.WriteAllBytes($"{RutasConfiguracion.PatDescarga}Isolucion.ApiRiesgoDafp.zip", ms.ToArray());
         //      //ms.WriteTo(fs);
         //      //ms.Seek(0, SeekOrigin.Begin);
         //      //ms.CopyTo(fs);
         //      //await response.FlushAsync();
         //      //await stramPathTo.FlushAsync();
         //      await fs.FlushAsync();
         //      ms.Close();
         //      fs.Close();
         //   }
         //}
      }

      /// <summary>
      /// detiene los sitios web
      /// </summary>
      /// <exception cref="InvalidOperationException"></exception>
      private void StopWebSite(string[] siteNames)
      {
         var server = new ServerManager();
         var sites = (from serv in server.Sites
                      join site in siteNames on serv.Name equals site
                      select serv).ToList();
         foreach (var site in sites)
         {
            if (site != null)
            {
               //stop the site...
               site.Stop();
               if (site.State == ObjectState.Stopped)
               {
                  //do deployment tasks...
               }
               else
               {
                  throw new InvalidOperationException($"Could not stop website! {site.Name}");
               }
            }
            else
            {
               throw new InvalidOperationException($"Could not find website! {site.Name}");
            }
         }
      }

      /// <summary>
      /// Inicia los sitios web
      /// </summary>
      /// <param name="siteNames"></param>
      /// <exception cref="InvalidOperationException"></exception>
      private void StartWebSite(string[] siteNames)
      {
         var server = new ServerManager();
         var sites = (from serv in server.Sites
                      join site in siteNames on serv.Name equals site
                      select serv).ToList();

         foreach (var site in sites)
         {
            if (site != null)
            {
               //restart the site...
               site.Start();
            }
            else
            {
               throw new InvalidOperationException("Could not find website!");
            }
         }
      }

      /// <summary>
      /// detiene los apppol
      /// </summary>
      /// <param name="poolName"></param>
      private void StopPool(string[] poolNames)
      {
         var serverManager = new ServerManager();
         foreach (var poolName in poolNames)
         {
            var appPool = serverManager.ApplicationPools.FirstOrDefault(ap => ap.Name.Equals(poolName));
            if (appPool?.State == ObjectState.Started)
               appPool.Stop();
         }
      }

      /// <summary>
      /// Inicia los apppol
      /// </summary>
      /// <param name="poolName"></param>
      private void StartPool(string[] poolNames)
      {
         var serverManager = new ServerManager();
         foreach (var poolName in poolNames)
         {
            var appPool = serverManager.ApplicationPools.FirstOrDefault(ap => ap.Name.Equals(poolName));
            if (appPool?.State == ObjectState.Stopped)
               appPool.Start();
         }
      }

      /// <summary>
      /// Deteniene un servicio de windows
      /// </summary>
      /// <param name="winAppNombre"></param>
      private void StopWinApp(string winAppNombre)
      {
         var services = ServiceController.GetServices();

         var service = services.Where(s => s.ServiceName == winAppNombre)?.FirstOrDefault();
         if (service?.Status == ServiceControllerStatus.Stopped ||
             service?.Status == ServiceControllerStatus.StopPending)
         {
            service.Stop();
         }
      }

      /// <summary>
      /// Inicia un servicio de Windows
      /// </summary>
      /// <param name="winAppNombre"></param>
      private void StartWinApp(string winAppNombre)
      {
         var services = ServiceController.GetServices();

         var service = services.Where(s => s.ServiceName == winAppNombre)?.FirstOrDefault();
         if (service?.Status == ServiceControllerStatus.Stopped ||
             service?.Status == ServiceControllerStatus.StopPending)
         {
            service.Start();
         }
      }

      /// <summary>
      /// Descomprime los archivos de las aplicaciones
      /// </summary>
      private void DescomprimirArchivos(StringBuilder resumenProceso)
      {
         Logger.Info($"Eliminar carpetas de versiones anteriores");

         var carpetasViejas = Directory.GetDirectories(RutasConfiguracion.PathDescargaLocal);
         if (carpetasViejas.Any())
         {
            foreach (var folder in carpetasViejas)
            {
               var path = Path.Combine(RutasConfiguracion.PathDescargaLocal, folder);
               if (Directory.Exists(path))
               {
                  var dir = new DirectoryInfo(path);
                  dir.Delete(true);
               }
            }
         }

         Logger.Info($"descarga local {RutasConfiguracion.PathDescargaLocal}");
         var archivoVersion = Directory.GetFiles(RutasConfiguracion.PathDescargaLocal);
         if (archivoVersion.Any())
         {
            Logger.Info($"Descomprimiendo archivos aplicaciones");
            resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Descomprimiendo archivos aplicaciones");

            ZipFile.ExtractToDirectory(archivoVersion.FirstOrDefault(), RutasConfiguracion.PathDescargaLocal);
            File.Delete(archivoVersion.FirstOrDefault());
         }

         var zipFiles = Directory.GetFiles(RutasConfiguracion.PathDescargaLocal);

         foreach (var file in zipFiles)
         {
            Logger.Info($"Descomprimiendo {file}");
            resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} escomprimiendo {file}");
            ZipFile.ExtractToDirectory(file, RutasConfiguracion.PathDescargaLocal);
            File.Delete(file);
            Logger.Info($"Descompresion Finalizada");
            resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Descompresion Finalizada");
         }
      }

      /// <summary>
      /// Elimina los archivos marcados como excepciones
      /// </summary>
      /// <param name="excepciones"></param>
      private void EliminarArchivosExcepciones(string[] excepciones, string nombreAplicacion)
      {
         Logger.Info($"Eliminando Arcivos Excepciones Aplicacion: {nombreAplicacion}");
         foreach (var exc in excepciones)
         {
            string nombreArchivo = $"{RutasConfiguracion.PathDescargaLocal}{nombreAplicacion}\\{exc}";
            if (File.Exists(nombreArchivo))
            {
               File.Delete(nombreArchivo);
            }
         }
      }

      /// <summary>
      /// Copia los archivos de la nueva version
      /// </summary>
      /// <param name="componente"></param>
      /// <param name="resumenProceso"></param>
      /// <param name="nombreAplicacion"></param>
      private void CopiarAppFiles(ComponenteDto componente, StringBuilder resumenProceso)
      {
         Logger.Info($"Actualizando Solucion {componente.NomComponente}");
         resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Actualizando Solucion {componente.NomComponente}");
         if (componente.Excepciones != null)
            EliminarArchivosExcepciones(componente.Excepciones.Split(','), componente.NombresSistema);

         switch (componente.NomComponente)
         {
            case "IsolucionWeb":
               Copiar($"{RutasConfiguracion.PathDescargaLocal}{componente.NomComponente}", RutasConfiguracion.PathEstandar, resumenProceso);
               if (Directory.Exists($"{RutasConfiguracion.PathEstandarTEST}{componente.NomComponente}"))
                  Copiar(RutasConfiguracion.PathDescargaLocal, $"{RutasConfiguracion.PathEstandarTEST}{componente.NomComponente}", resumenProceso);
               break;
            case "Isolucion.Api":
               Copiar($"{RutasConfiguracion.PathDescargaLocal}{componente.NomComponente}", RutasConfiguracion.PathApi, resumenProceso);
               break;
            case "Isolucion.ApiGestionCambio":
               Copiar($"{RutasConfiguracion.PathDescargaLocal}{componente.NomComponente}", RutasConfiguracion.PathApiGestionCambio, resumenProceso);
               break;
            case "Isolucion.ApiRiesgoDafp":
               Copiar($"{RutasConfiguracion.PathDescargaLocal}{componente.NomComponente}", RutasConfiguracion.PathApiRiesgoDafp, resumenProceso);
               break;
            case "Isolucion.ApiSyncFusion":
               Copiar($"{RutasConfiguracion.PathDescargaLocal}{componente.NomComponente}", RutasConfiguracion.PathEstandar, resumenProceso);
               break;
            case "Isolucion.Servicio":
               Copiar($"{RutasConfiguracion.PathDescargaLocal}{componente.NomComponente}", RutasConfiguracion.PathServicio, resumenProceso);
               break;
            case "xProjectAnalyzer":
               Copiar($"{RutasConfiguracion.PathDescargaLocal}{componente.NomComponente}", RutasConfiguracion.PathxProjectAnalyzer, resumenProceso);
               break;
            case "GenericEventHandler":
               Copiar($"{RutasConfiguracion.PathDescargaLocal}{componente.NomComponente}", RutasConfiguracion.PathGenericHandler, resumenProceso);
               break;
         }

         Logger.Info($"Actualizacion Solucion {componente.NomComponente} Finalizada");
         resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss} Actualizacion Solucion {componente.NomComponente} Finalizada");
      }

      private void BackupArchivosAplicaciones(ComponenteDto componente, StringBuilder resumenProceso)
      {
         Logger.Info($"inicia backup {componente.NomComponente}");
         if (!Directory.Exists(RutasConfiguracion.Backup))
            Directory.CreateDirectory(RutasConfiguracion.Backup);

         var path = $"{RutasConfiguracion.Backup}\\{DateTime.Now:yyyy-MM-dd}\\";

         if (!Directory.Exists($"{path}{componente.NomComponente}"))
            Directory.CreateDirectory($"{path}{componente.NomComponente}");

         switch (componente.NomComponente)
         {
            case "IsolucionWeb":
               Copiar(RutasConfiguracion.PathEstandar, $"{path}{componente.NomComponente}", resumenProceso);
               if (Directory.Exists(RutasConfiguracion.PathEstandarTEST))
               {
                  if (!Directory.Exists($"{path}{componente.NomComponente}"))
                     Directory.CreateDirectory($"{path}{componente.NomComponente}");
                  Copiar(RutasConfiguracion.PathEstandarTEST, $"{path}{componente.NomComponente}", resumenProceso);
               }

               break;
            case "Isolucion.Api":
               Copiar(RutasConfiguracion.PathApi, $"{path}{componente.NomComponente}", resumenProceso);
               break;
            case "Isolucion.ApiGestionCambio":
               Copiar(RutasConfiguracion.PathApiGestionCambio, $"{path}{componente.NomComponente}", resumenProceso);
               break;
            case "Isolucion.ApiRiesgoDafp":
               Copiar(RutasConfiguracion.PathApiRiesgoDafp, $"{path}{componente.NomComponente}", resumenProceso);
               break;
            case "Isolucion.ApiSyncFusion":
               Copiar(RutasConfiguracion.PathEstandar, $"{path}{componente.NomComponente}", resumenProceso);
               break;
            case "Isolucion.Servicio":
               Copiar(RutasConfiguracion.PathServicio, $"{path}{componente.NomComponente}", resumenProceso);
               break;
            case "xProjectAnalyzer":
               Copiar(RutasConfiguracion.PathxProjectAnalyzer, $"{path}{componente.NomComponente}", resumenProceso);
               break;
            case "GenericEventHandler":
               Copiar(RutasConfiguracion.PathGenericHandler, $"{path}{componente.NomComponente}", resumenProceso);
               break;
         }
         Logger.Info($"Finaliza backup {componente.NomComponente}");
      }

      /// <summary>
      /// Copia una carpeta y su contenido a otra ruta especificada
      /// </summary>
      /// <param name="carpetaOrigen">The carpeta origen.</param>
      /// <param name="carpetaDestino">The carpeta destino.</param>
      /// <param name="resumenProceso"></param>
      public void Copiar(string carpetaOrigen, string carpetaDestino, StringBuilder resumenProceso)
      {
         Logger.Info($"Inicia copiado archivos de {carpetaOrigen} a {carpetaDestino}");
         var origen = new DirectoryInfo(carpetaOrigen);
         var destino = new DirectoryInfo(carpetaDestino);

         Logger.Info($"Moviendo archivos de {origen} a {destino}");
         CopiarTodo(origen, destino, resumenProceso);
      }

      /// <summary>
      /// Usando recursividad para copiar contenido de una carpeta (archivos + subcarpetas).
      /// </summary>
      /// <param name="source">The source.</param>
      /// <param name="target">The target.</param>
      /// <param name="resumenProceso"></param>
      public void CopiarTodo(DirectoryInfo source, DirectoryInfo target, StringBuilder resumenProceso)
      {
         if (!Directory.Exists(target.FullName))
            Directory.CreateDirectory(target.FullName);

         // Copiar cada archivo en el nuevo directorio.
         foreach (FileInfo fi in source.GetFiles())
         {
            resumenProceso.AppendLine($"{DateTime.Now:yy-MM-dd hh:MM:ss}  Ruta {RutasConfiguracion.PathIndexador} No encontrada Se creo la carpeta {RutasConfiguracion.PathIndexador}");
            fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
         }

         // Copiar cada subcarpeta usando recursividad.
         foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
         {
            DirectoryInfo nextTargetSubDir =
               target.CreateSubdirectory(diSourceSubDir.Name);
            CopiarTodo(diSourceSubDir, nextTargetSubDir, resumenProceso);
         }
      }

      private async Task ObtenerInformacionCliente(int codCliente)
      {
         try
         {
            OperatingSystem os = Environment.OSVersion;
            var sIFXLicence = string.Empty;
            var smartFlowLicence = string.Empty;

            using (var sifxReader = new StreamReader($"{ConfigurationManager.AppSettings["LicencesPath"]}SFIXF.icn"))
            {
               sIFXLicence = sifxReader.ReadToEnd();
            }

            using (var smarthFlowReader = new StreamReader($"{ConfigurationManager.AppSettings["LicencesPath"]}SmartFlowIsol.icn"))
            {
               smartFlowLicence = smarthFlowReader.ReadToEnd();
            }

            var info = new InformacionClienteDto
            {
               CodCliente = codCliente,
               InformacionClienteDetalle = JsonConvert.SerializeObject(new DetalleInformacionClienteDto
               {
                  CodCliente = codCliente,
                  OSVersion = os.Version.ToString(),
                  OSPlatoform = os.Platform.ToString(),
                  OSServicePack = os.ServicePack,
                  VersionString = os.VersionString,
                  SQLVersion = _repositorio.ObtenerSQLVersion(),
                  SIFXLicence = sIFXLicence,
                  SmartFlowLicence = smartFlowLicence
               })
            };

            using (HttpClient client = new HttpClient())
            {
               StringContent content = new StringContent(JsonConvert.SerializeObject(info), Encoding.UTF8, "application/json");
               var response = await client.PostAsync(new Uri($"{ConstantesGlobales.PortalLicenciamientoApiUrl}GuardarInformacionCliente"), content);
               Logger.Info(response.StatusCode);
               Logger.Info(response.Content);
            }
         }
         catch (Exception e)
         {
            Logger.Error($"Error al enviar la informacion del cliente {e}");
            throw e;
         }
      }
   }
}
