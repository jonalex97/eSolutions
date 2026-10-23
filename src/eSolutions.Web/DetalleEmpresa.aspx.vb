Imports System
Imports eSolutions.Web.Common
Imports eSolutions.Web.Models

''' <summary>
''' Página de destino para el canal CJ (Cajas y Ventanilla).
''' Recupera los datos de la empresa desde la Session.
''' Si la sesión no existe o expiró, redirige inmediatamente al listado según requerimiento.
''' </summary>
Public Class DetalleEmpresa
    Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            ' Si no hay sesion activa volvemos al listado
            If Session("EmpresaSeleccionada") Is Nothing Then
                Dim canalRetorno As String = If(Session("CanalActual") IsNot Nothing, Session("CanalActual").ToString(), "CJ")
                Logger.LogWarning($"Acceso sin sesión activa a DetalleEmpresa.aspx. Redirigiendo a Empresas.aspx?canal={canalRetorno}")
                Response.Redirect($"Empresas.aspx?canal={canalRetorno}", True)
                Return
            End If

            If Not IsPostBack Then
                Dim empresa As EmpresaModel = DirectCast(Session("EmpresaSeleccionada"), EmpresaModel)

                litNombreEmpresa.Text = Server.HtmlEncode(empresa.Nombre)
                litIdEmpresa.Text = empresa.Id.ToString()
                imgLogoEmpresa.ImageUrl = empresa.LogoUrl
                imgLogoEmpresa.AlternateText = empresa.Nombre
            End If
        End Sub

        Protected Sub btnRegresar_Click(sender As Object, e As EventArgs)
            Dim canalRetorno As String = If(Session("CanalActual") IsNot Nothing, Session("CanalActual").ToString(), "CJ")
            Response.Redirect($"Empresas.aspx?canal={canalRetorno}")
        End Sub
    End Class
