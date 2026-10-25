Imports System
Imports eSolutions.Web.Common
Imports eSolutions.Web.Models

''' <summary>
''' Página de destino para el canal AB.
''' Recupera los datos de la empresa desde la Session.
''' Si la sesión no existe o expiró, redirige inmediatamente al listado según requerimiento.
''' </summary>
Public Class PagoEmpresa
    Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            ' Validar que exista la empresa en sesion, sino regresamos a la lista
            If Session("EmpresaSeleccionada") Is Nothing Then
                Dim canalRetorno As String = If(Session("CanalActual") IsNot Nothing, Session("CanalActual").ToString(), "AB")
                Logger.LogWarning($"Acceso sin sesión activa a PagoEmpresa.aspx. Redirigiendo a Empresas.aspx?canal={canalRetorno}")
                Response.Redirect($"Empresas.aspx?canal={canalRetorno}", True)
                Return
            End If

            If Not IsPostBack Then
                Dim empresa As EmpresaModel = DirectCast(Session("EmpresaSeleccionada"), EmpresaModel)
                Dim canal As String = If(Session("CanalActual") IsNot Nothing, Session("CanalActual").ToString(), "AB")

                litNombreEmpresa.Text = Server.HtmlEncode(empresa.Nombre)
                litIdEmpresa.Text = empresa.Id.ToString()
                litCanal.Text = Server.HtmlEncode(canal)
                imgLogoEmpresa.ImageUrl = empresa.LogoUrl
                imgLogoEmpresa.AlternateText = empresa.Nombre
            End If
        End Sub

        Protected Sub btnProcesarPago_Click(sender As Object, e As EventArgs)
            If Page.IsValid Then
                Dim empresa As EmpresaModel = DirectCast(Session("EmpresaSeleccionada"), EmpresaModel)
                Dim numeroReferencia As String = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()

                pnlResultadoPago.Visible = True
                litResultadoPago.Text = $"Se acreditó el monto de <strong>${Server.HtmlEncode(txtMonto.Text)}</strong> para <strong>{Server.HtmlEncode(empresa.Nombre)}</strong> (Contrato: {Server.HtmlEncode(txtNumeroContrato.Text)}). Comprobante No: <code>{numeroReferencia}</code>."

                Logger.LogInfo($"Pago procesado con éxito para empresa '{empresa.Nombre}' (ID: {empresa.Id}). Comprobante: {numeroReferencia}")
            End If
        End Sub

        Protected Sub btnRegresar_Click(sender As Object, e As EventArgs)
            Dim canalRetorno As String = If(Session("CanalActual") IsNot Nothing, Session("CanalActual").ToString(), "AB")
            Response.Redirect($"Empresas.aspx?canal={canalRetorno}")
        End Sub
    End Class
