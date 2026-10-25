Imports System
Imports System.Collections.Generic
Imports System.Net
Imports System.Net.Http
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web
Imports System.Web.Script.Serialization
Imports eSolutions.Web.Common
Imports eSolutions.Web.Models

Namespace Services
    ''' <summary>
    ''' Servicio para consumir el API de empresas de servicios.
    ''' </summary>
    Public Class EmpresasService
        Implements IEmpresasService

        ' Cliente compartido para reutilizar sockets
        Private Shared ReadOnly _clienteCompartido As HttpClient
        Private ReadOnly _httpClient As HttpClient

        Shared Sub New()
            ' Aseguramos soporte para TLS 1.2 / 1.3
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 Or SecurityProtocolType.Tls13

            Dim handler As New HttpClientHandler() With {
                .AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
            }
            _clienteCompartido = New HttpClient(handler)
        End Sub

        Public Sub New()
            _httpClient = _clienteCompartido
        End Sub

        ' Constructor para pruebas unitarias con mock de httpclient
        Public Sub New(cliente As HttpClient)
            If cliente Is Nothing Then
                Throw New ArgumentNullException(NameOf(cliente))
            End If
            _httpClient = cliente
        End Sub

        ''' <summary>
        ''' Consulta asíncronamente el catálogo de empresas para el canal configurado.
        ''' </summary>
        ''' <param name="config">Objeto con la configuración del canal (URL, API Key, Timeout).</param>
        ''' <returns>Lista fuertemente tipada de objetos EmpresaModel.</returns>
        Public Async Function ObtenerEmpresasAsync(config As CanalConfiguracion) As Task(Of List(Of EmpresaModel)) Implements IEmpresasService.ObtenerEmpresasAsync
            If config Is Nothing Then
                Throw New ArgumentNullException(NameOf(config))
            End If

            Dim correlationId As String = Guid.NewGuid().ToString("N").Substring(0, 8)
            Dim urlCompleta As String = ResolverUrl(config.ServicioUrl)
            Dim timeoutSegundos As Integer = If(config.TimeoutSegundos > 0, config.TimeoutSegundos, 10)

            Logger.LogInfo($"Iniciando consumo REST para canal '{config.CanalCodigo}' hacia URL: {urlCompleta}", correlationId)

            Using cts As New CancellationTokenSource(TimeSpan.FromSeconds(timeoutSegundos))
                Using request As New HttpRequestMessage(HttpMethod.Get, urlCompleta)
                    ' Inclusión del encabezado obligatorio X-Api-Key
                    If Not String.IsNullOrWhiteSpace(config.ApiKey) Then
                        request.Headers.Add("X-Api-Key", config.ApiKey)
                    End If

                    request.Headers.Add("Accept", "application/json")

                    Try
                        ' ConfigureAwait(False) para evitar bloqueos en SynchronizationContext
                        Using response As HttpResponseMessage = Await _httpClient.SendAsync(request, cts.Token).ConfigureAwait(False)
                            If response.StatusCode = HttpStatusCode.Unauthorized Then
                                Dim mensaje401 As String = $"El servicio respondió 401 Unauthorized: la API Key configurada para el canal '{config.CanalCodigo}' es inválida."
                                Logger.LogError(mensaje401, Nothing, correlationId)
                                Throw New ApiException(mensaje401, CInt(HttpStatusCode.Unauthorized), config.CanalCodigo)
                            End If

                            If Not response.IsSuccessStatusCode Then
                                Dim cuerpoError As String = Await response.Content.ReadAsStringAsync().ConfigureAwait(False)
                                Dim mensajeHttp As String = $"El servicio respondió con código HTTP {CInt(response.StatusCode)} ({response.StatusCode}). Detalle: {cuerpoError}"
                                Logger.LogError(mensajeHttp, Nothing, correlationId)
                                Throw New ApiException(mensajeHttp, CInt(response.StatusCode), config.CanalCodigo)
                            End If

                            Dim jsonRespuesta As String = Await response.Content.ReadAsStringAsync().ConfigureAwait(False)

                            If String.IsNullOrWhiteSpace(jsonRespuesta) Then
                                Return New List(Of EmpresaModel)()
                            End If

                            Dim serializer As New JavaScriptSerializer()
                            Dim listado As List(Of EmpresaModel) = serializer.Deserialize(Of List(Of EmpresaModel))(jsonRespuesta)

                            Logger.LogInfo($"Consumo exitoso para canal '{config.CanalCodigo}'. Empresas obtenidas: {If(listado IsNot Nothing, listado.Count, 0)}", correlationId)
                            Return If(listado, New List(Of EmpresaModel)())
                        End Using

                    Catch ex As OperationCanceledException When cts.IsCancellationRequested
                        Dim mensajeTimeout As String = $"Tiempo de espera agotado ({timeoutSegundos}s) al consultar el servicio del canal '{config.CanalCodigo}'."
                        Logger.LogError(mensajeTimeout, ex, correlationId)
                        Throw New TimeoutException(mensajeTimeout, ex)

                    Catch ex As ApiException
                        Throw

                    Catch ex As HttpRequestException
                        Dim mensajeRed As String = $"Error de red/conectividad al consultar el servicio del canal '{config.CanalCodigo}': {ex.Message}"
                        Logger.LogError(mensajeRed, ex, correlationId)
                        Throw New ApiException(mensajeRed, ex, 503, config.CanalCodigo)

                    Catch ex As Exception
                        Dim mensajeInesperado As String = $"Error inesperado al procesar la respuesta del servicio para el canal '{config.CanalCodigo}': {ex.Message}"
                        Logger.LogError(mensajeInesperado, ex, correlationId)
                        Throw New ApiException(mensajeInesperado, ex, 500, config.CanalCodigo)
                    End Try
                End Using
            End Using
        End Function

        ''' <summary>
        ''' Resuelve rutas relativas o virtuales (~/...) en URLs absolutas válidas para HttpClient.
        ''' </summary>
        Private Function ResolverUrl(urlServicio As String) As String
            If String.IsNullOrWhiteSpace(urlServicio) Then
                Return "http://localhost/empresas"
            End If

            If urlServicio.StartsWith("http://", StringComparison.OrdinalIgnoreCase) OrElse
               urlServicio.StartsWith("https://", StringComparison.OrdinalIgnoreCase) Then
                Return urlServicio
            End If

            ' Si es una ruta relativa o virtual (~/...)
            If HttpContext.Current IsNot Nothing AndAlso HttpContext.Current.Request IsNot Nothing Then
                Dim request = HttpContext.Current.Request
                Dim autoridad As String = request.Url.GetLeftPart(UriPartial.Authority)

                If urlServicio.StartsWith("~") Then
                    Return autoridad & VirtualPathUtility.ToAbsolute(urlServicio)
                ElseIf urlServicio.StartsWith("/") Then
                    Return autoridad & urlServicio
                Else
                    Return autoridad & "/" & urlServicio
                End If
            End If

            Return urlServicio
        End Function
    End Class
End Namespace
