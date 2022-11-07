!!!IMPORTANTE

1. Recuerde actualizar la cadena de conexion ActualizacionContexto  en el web.config 
2. Recuerde actualizar la ruta de los logs logDirectory en el archivo NLog.config
3. recuerde actualizar las siguientes llaves del App.config deacuerdo con cada cliente y segun lo estipulado en el documento de entrega.

-	Intervalo -> determina cada cuanto se ejecuta el servicio (este valor se representa en milisegundos y se multiplica por 1000).
-  PortalLicenciamientoApiUrl -> reemplazar con la url del servicio Isolucion. ServicioActualizacion.
-  Pathfile -> Ruta donde se descargaran los archivos comprimidos una vez se implemente la integración con el repositorio compartido de archivos. 
-  DescargaOnline -> Bandera para determinar si la descarga del archivo de actualización es online desde un repositorio de archivos o descargada manualmente y puesta en una ruta especifica en el equipo donde se encuentra instalado el servicio 
-  PathDescargaLocal -> Ruta donde se debe copiar el archivo zip con los archivos comprimidos de las aplicaciones.
-  PathEstandar -> ruta fisica de la aplicación Isolucion web.
-  PathEstandarTEST -> ruta fisica de la aplicación Isolucion pruebas.
-  PathApi -> ruta fisica de la aplicación
-  PathApiRiesgoDafp -> ruta fisica de la aplicación
-  PathApiGestionCambio -> ruta fisica de la aplicación
-  PathServicio -> ruta fisica de la aplicación
-  PathGenericHandler -> ruta fisica de la aplicación
-  PathIndexador -> ruta fisica de la aplicación
-  PathBackup -> ruta física donde se almacenaran los backups de las aplicaciones cada vez que se ejecuta el proceso de actualizacion.
-  poolEstandar -> nombre del pool de aplicaciones de la aplicación isolucion web
-  poolEstandarTEST -> nombre del pool de aplicaciones de la aplicación isolucion web pruebas
-  poolApiDataConnector -> -> nombre del pool de aplicaciones de la aplicación
-  poolApiRiesgosDafp -> -> nombre del pool de aplicaciones de la aplicación
-  poolApGestionCamio  -> -> nombre del pool de aplicaciones de la aplicación
-  EstandarWebNombres -> Nombre(s) de el(los) sitio(s) web de isolucion web
-  EstandarTestWebNombres -> Nombre(s) de el(los) sitio(s) web de isolucion web pruebas
-  ApiDataConectorWebNombres -> Nombre(s) de el(los) sitio(s) web
-  ApiRiesgoDafpNombres -> Nombre(s) de el(los) sitio(s) web
-  ApiGestionDelCambioNombres -> Nombre(s) de el(los) sitio(s) web
-  IsolucionServicioNombre ->Nombre(s) de el(los) servicio(s)
-  xProjectAnalyzerNombre ->Nombre(s) de el(los) servicio(s)
-  GenericEventHandler ->Nombre(s) de el(los) servicio(s)
-  LicencesPath -> ruta donde se encuentran las licencias de cada cliente
