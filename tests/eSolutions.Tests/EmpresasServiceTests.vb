Imports System
Imports System.Collections.Generic
Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports eSolutions.Web.Common
Imports eSolutions.Web.Models
Imports eSolutions.Web.Services

Namespace eSolutions.Tests
    ''' <summary>
    ''' Pruebas unitarias para EmpresasService usando mock de HttpMessageHandler.
    ''' </summary>
    <TestClass>
    Public Class EmpresasServiceTests

        <TestMethod>
        Public Async Function ObtenerEmpresasAsync_RespuestaExitosa_RetornaListaDeserializada() As Task
            ' Arrange
            Dim jsonEsperado As String = "[{""id"":101,""nombre"":""Agua Potable"",""logoUrl"":""http://test/logo1.png""},{""id"":102,""nombre"":""Energia Electrica"",""logoUrl"":""http://test/logo2.png""}]"
            Dim encabezadoApiKeyCapturado As String = Nothing

            Dim mockHandler As New MockHttpMessageHandler(Function(req)
                If req.Headers.Contains("X-Api-Key") Then
                    encabezadoApiKeyCapturado = req.Headers.GetValues("X-Api-Key").ToString()
                End If

                Dim response As New HttpResponseMessage(HttpStatusCode.OK) With {
                    .Content = New StringContent(jsonEsperado, Encoding.UTF8, "application/json")
                }
                Return response
            End Function)

            Dim httpClient As New HttpClient(mockHandler)
            Dim servicio As New EmpresasService(httpClient)

            Dim config As New CanalConfiguracion() With {
                .CanalCodigo = "AB",
                .CanalNombre = "Canal Autoservicio Bancario",
                .ServicioUrl = "http://simulador.local/empresas?canal=AB",
                .ApiKey = "KEY-SEC-AB-2026",
                .TimeoutSegundos = 5
            }

            ' Act
            Dim resultado As List(Of EmpresaModel) = Await servicio.ObtenerEmpresasAsync(config)

            ' Assert
            Assert.IsNotNull(resultado, "El resultado no debe ser nulo.")
            Assert.AreEqual(2, resultado.Count, "Debe deserializar exactamente 2 empresas.")
            Assert.AreEqual(101, resultado(0).Id)
            Assert.AreEqual("Agua Potable", resultado(0).Nombre)
            Assert.AreEqual(102, resultado(1).Id)
            Assert.AreEqual("Energia Electrica", resultado(1).Nombre)
        End Function

        <TestMethod>
        <ExpectedException(GetType(ApiException))>
        Public Async Function ObtenerEmpresasAsync_LlaveInvalida401_LanzaApiException() As Task
            ' Arrange
            Dim mockHandler As New MockHttpMessageHandler(Function(req)
                Dim response As New HttpResponseMessage(HttpStatusCode.Unauthorized) With {
                    .Content = New StringContent("{""error"":""401 Unauthorized""}", Encoding.UTF8, "application/json")
                }
                Return response
            End Function)

            Dim httpClient As New HttpClient(mockHandler)
            Dim servicio As New EmpresasService(httpClient)

            Dim config As New CanalConfiguracion() With {
                .CanalCodigo = "AB",
                .ServicioUrl = "http://simulador.local/empresas?canal=AB",
                .ApiKey = "LLAVE-INCORRECTA",
                .TimeoutSegundos = 5
            }

            ' Act
            Await servicio.ObtenerEmpresasAsync(config)
            ' Assert en ExpectedException
        End Function

        <TestMethod>
        <ExpectedException(GetType(ApiException))>
        Public Async Function ObtenerEmpresasAsync_ErrorServidor500_LanzaApiException() As Task
            ' Arrange
            Dim mockHandler As New MockHttpMessageHandler(Function(req)
                Dim response As New HttpResponseMessage(HttpStatusCode.InternalServerError) With {
                    .Content = New StringContent("{""error"":""Internal Server Error""}", Encoding.UTF8, "application/json")
                }
                Return response
            End Function)

            Dim httpClient As New HttpClient(mockHandler)
            Dim servicio As New EmpresasService(httpClient)

            Dim config As New CanalConfiguracion() With {
                .CanalCodigo = "CJ",
                .ServicioUrl = "http://simulador.local/empresas?canal=CJ",
                .ApiKey = "KEY-SEC-CJ-2026",
                .TimeoutSegundos = 5
            }

            ' Act
            Await servicio.ObtenerEmpresasAsync(config)
            ' Assert en ExpectedException
        End Function

        <TestMethod>
        <ExpectedException(GetType(ArgumentNullException))>
        Public Async Function ObtenerEmpresasAsync_ConfiguracionNull_LanzaArgumentNullException() As Task
            Dim httpClient As New HttpClient(New MockHttpMessageHandler(Function(req) New HttpResponseMessage(HttpStatusCode.OK)))
            Dim servicio As New EmpresasService(httpClient)

            Await servicio.ObtenerEmpresasAsync(Nothing)
        End Function

    End Class

    ''' <summary>
    ''' Mock para interceptar peticiones http en pruebas.
    ''' </summary>
    Public Class MockHttpMessageHandler
        Inherits HttpMessageHandler

        Private ReadOnly _handler As Func(Of HttpRequestMessage, HttpResponseMessage)

        Public Sub New(handler As Func(Of HttpRequestMessage, HttpResponseMessage))
            _handler = handler
        End Sub

        Protected Overrides Function SendAsync(request As HttpRequestMessage, cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
            Return Task.FromResult(_handler(request))
        End Function
    End Class
End Namespace
