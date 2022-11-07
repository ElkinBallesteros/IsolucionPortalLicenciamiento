using Autofac;
using Isolucion.ServicioActualizacion.Common;
using Isolucion.ServicioActualizacion.Negocio;
using System.Configuration;
using System.Timers;


namespace Isolucion.ServicioActualizacion
{
   public class Service
   {
      private readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
      readonly IContainer _container;
      // The timer que ejecuta elevento
      private Timer _heartbeatTimer = new Timer();

      // Boolean para determinar si se debe lanzar el proceso o no
      private volatile bool _requestServiceStop = false;

      // Intervalo de ejecucion del servicio
      private readonly long _heartbeatInterval = ConfigurationManager.AppSettings["Intervalo"].StingToLong();

      #region Constructors

      public Service()
      {
         // Se setea el intervalo de ejecucion del Servicio
         _heartbeatTimer.Interval = _heartbeatInterval * 1000;
         // Se asocia el evento del timer
         _heartbeatTimer.Elapsed += OnTimedEvent;
         // Important! No queremos que el evento se lance hasta que el proceso haya finalizado
         _heartbeatTimer.AutoReset = false;

         _container = AutofacConfigurator.GetContainer();
      }

      #endregion

      #region Private Methods

      #endregion

      #region Public Methods

      public void Start()
      {
         Logger.Info("Servicio Iniciado");
         // Event should not stop the timer
         _requestServiceStop = false;
         // Iniciarl el timer
         _heartbeatTimer.Start();
      }

      public void Stop()
      {
         Logger.Info("Servicio Detenido");

         ConstantesGlobales.EnEjecucion = false;

         // Event should stop the timer
         _requestServiceStop = true;
         // Se detiene el timer
         _heartbeatTimer.Stop();
      }

      #endregion

      private void OnTimedEvent(object source, ElapsedEventArgs e)
      {
         // Log Actividad del timer
         Logger.Info("Heartbeat .");
         // Put code here

         // Start timer again if no request to stop recieved
         if (!_requestServiceStop)
         {
            _heartbeatTimer.Start();
         }

         if (ConstantesGlobales.EnEjecucion) return;

         using (var scope = _container.BeginLifetimeScope())
         {
            var actualizacionNegocio = scope.Resolve<IServicioActualizacionNegocio>();

            actualizacionNegocio.ActualizarAplicacion();
         }
      }
   }
}
