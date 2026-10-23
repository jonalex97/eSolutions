Imports System

Namespace Common
    ''' <summary>
    ''' Excepción especializada para errores HTTP provenientes de la API REST de Empresas.
    ''' </summary>
    <Serializable>
    Public Class ApiException
        Inherits Exception

        Public Property StatusCode As Integer
        Public Property Canal As String

        Public Sub New(message As String)
            MyBase.New(message)
        End Sub

        Public Sub New(message As String, statusCode As Integer, Optional canal As String = "")
            MyBase.New(message)
            Me.StatusCode = statusCode
            Me.Canal = canal
        End Sub

        Public Sub New(message As String, innerException As Exception, Optional statusCode As Integer = 500, Optional canal As String = "")
            MyBase.New(message, innerException)
            Me.StatusCode = statusCode
            Me.Canal = canal
        End Sub
    End Class
End Namespace
