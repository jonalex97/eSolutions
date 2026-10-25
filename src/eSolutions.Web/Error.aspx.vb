Imports System

Public Class ErrorPage
    Inherits System.Web.UI.Page

    Public Property IncidentId As String = "N/A"

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim reqId As String = Request.QueryString("incident")
        If Not String.IsNullOrEmpty(reqId) Then
            IncidentId = reqId
        Else
            IncidentId = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()
        End If
    End Sub
End Class
