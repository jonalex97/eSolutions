<%@ Page Title="Error Inesperado" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeBehind="Error.aspx.vb" Inherits="eSolutions.Web.ErrorPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row justify-content-center my-5">
        <div class="col-12 col-md-8 col-lg-6 text-center">
            <div class="card border-0 shadow-sm p-4">
                <div class="card-body">
                    <i class="bi bi-exclamation-triangle-fill fs-1 text-warning mb-3"></i>
                    <h3 class="fw-bold mb-2">Ha ocurrido un problema técnico</h3>
                    <p class="text-muted mb-4">
                        La solicitud no pudo ser completada. Por seguridad y protección de la información institucional, los detalles técnicos han sido registrados en la bitácora interna del servidor.
                    </p>
                    <div class="alert alert-light border small text-muted mb-4 text-start">
                        <div><strong>Código de incidente:</strong> <code><%: IncidentId %></code></div>
                        <div><strong>Hora del evento:</strong> <%: DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") %> UTC</div>
                    </div>
                    <a href="Empresas.aspx?canal=AB" class="btn btn-primary px-4 fw-semibold">
                        <i class="bi bi-house me-1"></i> Volver al Inicio
                    </a>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
