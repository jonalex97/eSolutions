Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports eSolutions.Web.Common
Imports eSolutions.Web.Data
Imports eSolutions.Web.Models
Imports eSolutions.Web.Services

''' <summary>
''' Pagina principal para listar las empresas de pago segun el canal recibido.
''' </summary>
Public Class Empresas
    Inherits System.Web.UI.Page

        Private Property ConfiguracionActual As CanalConfiguracion
            Get
                Return TryCast(ViewState("ConfiguracionActual"), CanalConfiguracion)
            End Get
            Set(value As CanalConfiguracion)
                ViewState("ConfiguracionActual") = value
            End Set
        End Property

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            ' Carga inicial de datos solo si no es postback
            If Not IsPostBack Then
                Dim canalParam As String = Request.QueryString("canal")

                If String.IsNullOrWhiteSpace(canalParam) Then
                    MostrarCanalNoDisponible("No se suministro el parametro 'canal' en la URL. Ingrese con un canal valido (ej: ?canal=AB o ?canal=CJ).")
                    Return
                End If

                ' Consultamos la configuracion del canal
                Dim repo As New ConfiguracionRepository()
                Dim config As CanalConfiguracion = repo.ObtenerPorCanal(canalParam)

                If config Is Nothing Then
                    MostrarCanalNoDisponible($"El canal '{Server.HtmlEncode(canalParam)}' no existe en el catálogo o se encuentra inactivo.")
                    Return
                End If

                ' Guardamos la config en viewstate
                Me.ConfiguracionActual = config

                ' Menu visible segun el canal
                pnlMenuCanal.Visible = config.MuestraMenu

                lblCodigoCanal.Text = Server.HtmlEncode(config.CanalCodigo)
                lblNombreCanal.Text = Server.HtmlEncode(config.CanalNombre)
                lblDestinoInfo.Text = $"Página destino: <code>{Server.HtmlEncode(config.PaginaDestino)}</code> | Menú: <strong>{If(config.MuestraMenu, "Activo", "Inactivo")}</strong>"

                ' Registro asincrono para consultar el servicio sin bloquear la pagina
                RegisterAsyncTask(New PageAsyncTask(AddressOf CargarEmpresasAsync))
            End If
        End Sub

        Private Async Function CargarEmpresasAsync() As Task
            Dim correlationId As String = Guid.NewGuid().ToString("N").Substring(0, 8)
            Try
                Dim servicio As IEmpresasService = New EmpresasService()
                Dim catalogo As List(Of EmpresaModel) = Await servicio.ObtenerEmpresasAsync(Me.ConfiguracionActual)

                ' Persistir listado en ViewState para resolver la selección en el postback del Repeater
                ViewState("CatalogoEmpresas") = catalogo

                If catalogo IsNot Nothing AndAlso catalogo.Count > 0 Then
                    rptEmpresas.DataSource = catalogo
                    rptEmpresas.DataBind()
                    pnlMosaico.Visible = True
                    pnlError.Visible = False
                Else
                    MostrarMensajeError("No se encontraron empresas disponibles para este canal.", correlationId)
                End If

            Catch ex As TimeoutException
                MostrarMensajeError($"El servicio tardó más del tiempo configurado ({Me.ConfiguracionActual.TimeoutSegundos}s). Intente nuevamente más tarde.", correlationId)

            Catch ex As ApiException When ex.StatusCode = 401
                MostrarMensajeError("La API Key configurada para este canal fue rechazada por el servicio (401 Unauthorized). Verifique las credenciales del canal.", correlationId)

            Catch ex As ApiException
                MostrarMensajeError($"El servicio de empresas no pudo procesar la solicitud (Código HTTP: {ex.StatusCode}).", correlationId)

            Catch ex As Exception
                Logger.LogError($"Error no controlado al cargar catálogo de empresas en Empresas.aspx", ex, correlationId)
                MostrarMensajeError("Ocurrió un error inesperado al consultar el catálogo de empresas. El incidente ha sido registrado.", correlationId)
            End Try
        End Function

        ''' <summary>
        ''' Manejo del evento de selección dentro del Repeater (ItemCommand).
        ''' Guarda la empresa seleccionada en Session y redirige a la página configurada.
        ''' </summary>
        Protected Sub rptEmpresas_ItemCommand(source As Object, e As RepeaterCommandEventArgs) Handles rptEmpresas.ItemCommand
            If e.CommandName = "Seleccionar" Then
                Dim idEmpresa As Integer
                If Integer.TryParse(e.CommandArgument.ToString(), idEmpresa) Then
                    Dim catalogo As List(Of EmpresaModel) = TryCast(ViewState("CatalogoEmpresas"), List(Of EmpresaModel))
                    Dim empresaSeleccionada As EmpresaModel = Nothing

                    If catalogo IsNot Nothing Then
                        empresaSeleccionada = catalogo.FirstOrDefault(Function(em) em.Id = idEmpresa)
                    End If

                    If empresaSeleccionada IsNot Nothing Then
                        ' Persistir selección en Session
                        Session("EmpresaSeleccionada") = empresaSeleccionada
                        Session("CanalActual") = Me.ConfiguracionActual.CanalCodigo

                        Logger.LogInfo($"Usuario seleccionó la empresa ID: {empresaSeleccionada.Id} ({empresaSeleccionada.Nombre}) en canal {Me.ConfiguracionActual.CanalCodigo}. Redirigiendo a: {Me.ConfiguracionActual.PaginaDestino}")

                        ' Redirección dinámica según la configuración del canal (AB -> PagoEmpresa.aspx, CJ -> DetalleEmpresa.aspx)
                        Response.Redirect(Me.ConfiguracionActual.PaginaDestino, False)
                        Context.ApplicationInstance.CompleteRequest()
                    Else
                        MostrarMensajeError("No se pudo recuperar la información de la empresa seleccionada.", Guid.NewGuid().ToString("N").Substring(0, 8))
                    End If
                End If
            End If
        End Sub

        Private Sub MostrarCanalNoDisponible(mensaje As String)
            pnlCanalNoEncontrado.Visible = True
            pnlMosaico.Visible = False
            pnlMenuCanal.Visible = False
            pnlError.Visible = False
            lblCanalNoEncontradoMensaje.Text = Server.HtmlEncode(mensaje)
        End Sub

        Private Sub MostrarMensajeError(mensaje As String, correlationId As String)
            pnlError.Visible = True
            pnlMosaico.Visible = False
            lblMensajeError.Text = Server.HtmlEncode(mensaje)
            lblCorrelationId.Text = Server.HtmlEncode(correlationId)
        End Sub
    End Class
